using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Youtube;

internal record YoutubePlayer(string Id, string Source, int SignatureTimestamp);

internal record PlayerChallengeSolutions(
    IReadOnlyDictionary<string, string> N,
    IReadOnlyDictionary<string, string> Signatures
);

/// <summary>
/// Solves the challenges imposed by YouTube's player (the signature cipher and the "n" parameter
/// used for throttling) using yt-dlp's EJS solver (https://github.com/yt-dlp/ejs), which is
/// executed by a platform-provided JavaScript engine.
/// </summary>
public static class PlayerChallengeSolver
{
    private const string NotInitializedMarker = "__yd_not_initialized__";
    private const string MissingPlayerMarker = "__yd_missing_player__";

    private static readonly TimeSpan PlayerCacheDuration = TimeSpan.FromHours(1);

    private static readonly SemaphoreSlim PlayerLock = new(1, 1);
    private static readonly SemaphoreSlim EngineLock = new(1, 1);

    private static YoutubePlayer? _player;
    private static DateTimeOffset _playerResolvedAt;
    private static string? _initScript;

    /// <summary>
    /// JavaScript engine used to run the solver.
    /// Clients that require solving challenges can't be used if it's not set.
    /// </summary>
    public static IJavaScriptEngine? JavaScriptEngine { get; set; }

    internal static bool IsAvailable => JavaScriptEngine is not null;

    private static async Task<string> GetStringAsync(
        HttpMessageInvoker http,
        string url,
        CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", YoutubeRequestHandler.BrowserUserAgent);

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    internal static async Task<YoutubePlayer> GetPlayerAsync(
        HttpMessageInvoker http,
        CancellationToken cancellationToken = default
    )
    {
        await PlayerLock.WaitAsync(cancellationToken);

        try
        {
            if (
                _player is not null
                && DateTimeOffset.UtcNow - _playerResolvedAt < PlayerCacheDuration
            )
            {
                return _player;
            }

            var iframeApi = await GetStringAsync(
                http,
                "https://www.youtube.com/iframe_api",
                cancellationToken
            );

            var playerId = Regex
                .Match(iframeApi, @"player\\?/([0-9a-fA-F]{8})\\?/")
                .Groups[1]
                .Value;

            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new InvalidOperationException(
                    "Failed to resolve the version of YouTube's player."
                );
            }

            var player = _player;
            if (player is null || !string.Equals(player.Id, playerId, StringComparison.Ordinal))
            {
                var source = await GetStringAsync(
                    http,
                    $"https://www.youtube.com/s/player/{playerId}/player_ias.vflset/en_US/base.js",
                    cancellationToken
                );

                var signatureTimestampRaw = Regex
                    .Match(source, @"(?:signatureTimestamp|sts)\s*:\s*(\d{5})")
                    .Groups[1]
                    .Value;

                if (
                    !int.TryParse(
                        signatureTimestampRaw,
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out var signatureTimestamp
                    )
                )
                {
                    throw new InvalidOperationException(
                        "Failed to extract the signature timestamp from YouTube's player."
                    );
                }

                player = new YoutubePlayer(playerId, source, signatureTimestamp);
            }

            _player = player;
            _playerResolvedAt = DateTimeOffset.UtcNow;

            return player;
        }
        finally
        {
            PlayerLock.Release();
        }
    }

    private static string ReadResource(string name)
    {
        using var stream =
            typeof(PlayerChallengeSolver).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Resource '{name}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // Loads the solver and defines a wrapper around it, which keeps the preprocessed player
    // in memory, so that the full player source only needs to be passed once per player version.
    private static string GetInitScript() =>
        _initScript ??= $$"""
            {{ReadResource("Ejs.yt.solver.lib.min.js")}}
            Object.assign(globalThis, lib);
            {{ReadResource("Ejs.yt.solver.core.min.js")}}
            var __ydPlayers = {};
            function __ydSolve(playerId, player, requests) {
              try {
                var input;
                if (player !== null) {
                  input = { type: "player", player: player, output_preprocessed: true, requests: requests };
                } else if (Object.prototype.hasOwnProperty.call(__ydPlayers, playerId)) {
                  input = { type: "preprocessed", preprocessed_player: __ydPlayers[playerId], requests: requests };
                } else {
                  return JSON.stringify({ type: "error", error: "{{MissingPlayerMarker}}" });
                }
                var output = jsc(input);
                if (output && output.preprocessed_player) {
                  __ydPlayers = {};
                  __ydPlayers[playerId] = output.preprocessed_player;
                  delete output.preprocessed_player;
                }
                return JSON.stringify(output);
              } catch (e) {
                return JSON.stringify({ type: "error", error: String((e && e.stack) || e) });
              }
            }
            "ok";
            """;

    private static bool IsValidSolution(string challenge, string? solution) =>
        !string.IsNullOrWhiteSpace(solution)
        && !string.Equals(challenge, solution, StringComparison.Ordinal)
        && !solution.StartsWith("enhanced_except_", StringComparison.Ordinal);

    internal static async Task<PlayerChallengeSolutions> SolveAsync(
        YoutubePlayer player,
        IReadOnlyCollection<string> nChallenges,
        IReadOnlyCollection<string> signatureChallenges,
        CancellationToken cancellationToken = default
    )
    {
        var nSolutions = new Dictionary<string, string>(StringComparer.Ordinal);
        var signatureSolutions = new Dictionary<string, string>(StringComparer.Ordinal);

        if (nChallenges.Count <= 0 && signatureChallenges.Count <= 0)
            return new PlayerChallengeSolutions(nSolutions, signatureSolutions);

        var engine =
            JavaScriptEngine
            ?? throw new InvalidOperationException("JavaScript engine is not available.");

        var requests = new JsonArray();
        var requestTargets = new List<Dictionary<string, string>>();

        void AddRequest(
            string type,
            IReadOnlyCollection<string> challenges,
            Dictionary<string, string> target
        )
        {
            if (challenges.Count <= 0)
                return;

            requests.Add(
                (JsonNode)new JsonObject
                {
                    ["type"] = type,
                    ["challenges"] = new JsonArray(
                        challenges.Select(c => (JsonNode?)JsonValue.Create(c)).ToArray()
                    ),
                }
            );

            requestTargets.Add(target);
        }

        AddRequest("n", nChallenges, nSolutions);
        AddRequest("sig", signatureChallenges, signatureSolutions);

        var playerIdJson = JsonValue.Create(player.Id).ToJsonString();
        var requestsJson = requests.ToJsonString();

        var solveScript =
            $"typeof __ydSolve === 'function' ? __ydSolve({playerIdJson}, null, {requestsJson}) : '{NotInitializedMarker}'";

        string? output;

        await EngineLock.WaitAsync(cancellationToken);

        try
        {
            output = await engine.EvaluateAsync(solveScript, cancellationToken);

            if (string.Equals(output, NotInitializedMarker, StringComparison.Ordinal))
            {
                await engine.EvaluateAsync(GetInitScript(), cancellationToken);
                output = await engine.EvaluateAsync(solveScript, cancellationToken);
            }

            if (output?.Contains(MissingPlayerMarker, StringComparison.Ordinal) == true)
            {
                var playerSourceJson = JsonValue.Create(player.Source).ToJsonString();

                output = await engine.EvaluateAsync(
                    $"__ydSolve({playerIdJson}, {playerSourceJson}, {requestsJson})",
                    cancellationToken
                );
            }
        }
        finally
        {
            EngineLock.Release();
        }

        if (string.IsNullOrWhiteSpace(output))
            throw new InvalidOperationException("JavaScript challenge solver returned no output.");

        using var document = JsonDocument.Parse(output);
        var root = document.RootElement;

        if (
            !root.TryGetProperty("type", out var type)
            || !string.Equals(type.GetString(), "result", StringComparison.Ordinal)
        )
        {
            var error = root.TryGetProperty("error", out var errorElement)
                ? errorElement.ToString()
                : output;

            throw new InvalidOperationException(
                $"Failed to solve the challenges of YouTube's player {player.Id}: {error}"
            );
        }

        if (
            root.TryGetProperty("responses", out var responses)
            && responses.ValueKind == JsonValueKind.Array
        )
        {
            var index = 0;
            foreach (var response in responses.EnumerateArray())
            {
                if (index >= requestTargets.Count)
                    break;

                var target = requestTargets[index++];

                if (
                    !response.TryGetProperty("type", out var responseType)
                    || !string.Equals(responseType.GetString(), "result", StringComparison.Ordinal)
                    || !response.TryGetProperty("data", out var data)
                    || data.ValueKind != JsonValueKind.Object
                )
                {
                    continue;
                }

                foreach (var solution in data.EnumerateObject())
                {
                    if (solution.Value.ValueKind != JsonValueKind.String)
                        continue;

                    var value = solution.Value.GetString();
                    if (IsValidSolution(solution.Name, value))
                        target[solution.Name] = value!;
                }
            }
        }

        return new PlayerChallengeSolutions(nSolutions, signatureSolutions);
    }
}
