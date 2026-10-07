using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.Core.Youtube;

/// <summary>
/// Handles requests made by YoutubeExplode to adapt them to the current behavior of YouTube.
/// </summary>
/// <remarks>
/// YoutubeExplode fetches stream data using clients (VISIONOS, ANDROID) that don't support
/// cookies, so YouTube rejects requests from signed-in users with "400 Bad Request"
/// (https://github.com/Tyrrrz/YoutubeExplode/issues/969). For signed-in users, this handler
/// fetches stream data using the TV client instead (same as yt-dlp), which supports cookies but
/// requires solving the challenges imposed by YouTube's player (see <see cref="PlayerChallengeSolver" />).
/// </remarks>
internal partial class YoutubeRequestHandler : HttpMessageHandler
{
    internal const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36";

    private const string Origin = "https://www.youtube.com";

    // Consent to the use of cookies on YouTube (same as YoutubeExplode)
    private const string ConsentCookieName = "SOCS";
    private const string ConsentCookieValue = "CAISEwgDEgk4MTM4MzYzNTIaAmVuIAEaBgiApPzGBg";

    // Same as yt-dlp's "tv_downgraded" client, which it uses by default for signed-in users
    private const string TvClientName = "TVHTML5";
    private const string TvClientNameId = "7";
    private const string TvClientVersion = "5.20260707";
    private const string TvUserAgent = "Mozilla/5.0 (ChromiumStylePlatform) Cobalt/Version";

    private const int PlayerCacheCapacity = 16;
    private static readonly TimeSpan PlayableCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan UnplayableCacheDuration = TimeSpan.FromMinutes(1);

    private static readonly Dictionary<
        string,
        (DateTimeOffset ExpiresAt, PlayerResult Result)
    > PlayerCache = new(StringComparer.Ordinal);

    private readonly HttpMessageInvoker _http;
    private readonly IReadOnlyList<KeyValuePair<string, string>> _cookies;
    private readonly bool _isAuthenticated;
    private readonly string _identity;

    public YoutubeRequestHandler(HttpMessageHandler innerHandler, IReadOnlyList<Cookie> cookies)
    {
        _http = new HttpMessageInvoker(innerHandler, false);

        _cookies = cookies
            .Where(c => !c.Expired && IsYoutubeHost(c.Domain.TrimStart('.')))
            .Select(c => new KeyValuePair<string, string>(c.Name, c.Value))
            .ToArray();

        var cookieMap = ToDictionary(_cookies);
        _isAuthenticated = GetSessionIds(cookieMap).Any(s => !string.IsNullOrWhiteSpace(s));

        // Used to separate cached responses of different accounts
        _identity = _isAuthenticated
            ? "user:"
                + Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|', GetSessionIds(cookieMap))))
                )[..16]
            : "anonymous";
    }

    private static bool IsYoutubeHost(string host) =>
        string.Equals(host, "youtube.com", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, string> ToDictionary(
        IEnumerable<KeyValuePair<string, string>> cookies
    )
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, value) in cookies)
            result[name] = value;

        return result;
    }

    private static string?[] GetSessionIds(IReadOnlyDictionary<string, string> cookies) =>
        [
            cookies.GetValueOrDefault("SAPISID"),
            cookies.GetValueOrDefault("__Secure-1PAPISID"),
            cookies.GetValueOrDefault("__Secure-3PAPISID"),
        ];

    private static IEnumerable<KeyValuePair<string, string>> ParseCookieHeader(string header)
    {
        foreach (var part in header.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = part.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var name = part[..separatorIndex].Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            yield return new KeyValuePair<string, string>(name, part[(separatorIndex + 1)..].Trim());
        }
    }

    // YoutubeExplode keeps cookies in a container that only retains up to 20 cookies per domain,
    // which is fewer than what a signed-in session uses. Restore the full set, while keeping the
    // values that were refreshed by YouTube in the meantime.
    private Dictionary<string, string> MergeCookies(HttpRequestMessage request)
    {
        var cookies = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ConsentCookieName] = ConsentCookieValue,
        };

        foreach (var (name, value) in _cookies)
            cookies[name] = value;

        if (request.Headers.TryGetValues("Cookie", out var headerValues))
        {
            foreach (var headerValue in headerValues)
            {
                foreach (var (name, value) in ParseCookieHeader(headerValue))
                    cookies[name] = value;
            }
        }

        request.Headers.Remove("Cookie");
        if (cookies.Count > 0)
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"))
            );
        }

        return cookies;
    }

    // Same as what YouTube's web app (and yt-dlp) sends
    private static string? TryGenerateAuthorization(IReadOnlyDictionary<string, string> cookies)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var parts = new List<string>();

        void Add(string scheme, string? sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return;

            var hash = Convert.ToHexStringLower(
                SHA1.HashData(Encoding.UTF8.GetBytes($"{timestamp} {sessionId} {Origin}"))
            );

            parts.Add($"{scheme} {timestamp}_{hash}");
        }

        Add(
            "SAPISIDHASH",
            cookies.GetValueOrDefault("SAPISID") ?? cookies.GetValueOrDefault("__Secure-3PAPISID")
        );
        Add("SAPISID1PHASH", cookies.GetValueOrDefault("__Secure-1PAPISID"));
        Add("SAPISID3PHASH", cookies.GetValueOrDefault("__Secure-3PAPISID"));

        return parts.Count > 0 ? string.Join(' ', parts) : null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.RequestUri is { } uri && IsYoutubeHost(uri.Host))
        {
            var cookies = MergeCookies(request);

            if (
                request.Method == HttpMethod.Post
                && request.Content is not null
                && string.Equals(uri.AbsolutePath, "/youtubei/v1/player", StringComparison.Ordinal)
            )
            {
                return await HandlePlayerRequestAsync(request, cookies, cancellationToken);
            }
        }

        return await _http.SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> HandlePlayerRequestAsync(
        HttpRequestMessage request,
        IReadOnlyDictionary<string, string> cookies,
        CancellationToken cancellationToken
    )
    {
        var requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);

        string? videoId = null;
        string? clientName = null;
        string? visitorData = null;

        try
        {
            using var document = JsonDocument.Parse(requestBody);
            var root = document.RootElement;

            videoId = root.TryGetProperty("videoId", out var videoIdElement)
                ? videoIdElement.GetString()
                : null;

            if (
                root.TryGetProperty("context", out var context)
                && context.TryGetProperty("client", out var client)
            )
            {
                clientName = client.TryGetProperty("clientName", out var clientNameElement)
                    ? clientNameElement.GetString()
                    : null;

                visitorData = client.TryGetProperty("visitorData", out var visitorDataElement)
                    ? visitorDataElement.GetString()
                    : null;
            }
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException) { }

        if (string.IsNullOrWhiteSpace(videoId))
            return await _http.SendAsync(request, cancellationToken);

        var cookieHeader = request.Headers.TryGetValues("Cookie", out var cookieHeaderValues)
            ? string.Join("; ", cookieHeaderValues)
            : null;

        // Signed-in users: use the TV client, which accepts cookies
        PlayerResult? tvResult = null;
        if (_isAuthenticated)
        {
            tvResult = await GetTvPlayerResultAsync(
                videoId,
                visitorData,
                cookieHeader,
                cookies,
                cancellationToken
            );

            if (tvResult.IsPlayable)
                return CreateResponse(request, tvResult.Json!);

            // Fall back to the original client, but without the cookies, which it doesn't support
            request.Headers.Remove("Cookie");
            request.Headers.Remove("Authorization");
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Rate limit errors are handled by YoutubeExplode
            if (tvResult is null || (int)response.StatusCode == 429)
                return response;

            var originalResult = PlayerResult.Failure(
                $"HTTP {(int)response.StatusCode} ({clientName})"
            );

            response.Dispose();
            throw CreateException(videoId, tvResult, originalResult);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        PlayerResult result;
        try
        {
            result = await ProcessPlayerResponseAsync(
                responseBody,
                ShouldSolveChallenges(clientName, responseBody),
                cancellationToken
            );
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Let YoutubeExplode handle the response on its own
            result = await ProcessPlayerResponseAsync(responseBody, false, cancellationToken);
        }

        if (result.IsPlayable)
        {
            if (string.Equals(result.Json, responseBody, StringComparison.Ordinal))
                return response;

            response.Dispose();
            return CreateResponse(request, result.Json!);
        }

        response.Dispose();

        // Signed-out users: the TV client may still work when other clients are refused
        if (!_isAuthenticated)
        {
            tvResult = await GetTvPlayerResultAsync(
                videoId,
                visitorData,
                cookieHeader,
                cookies,
                cancellationToken
            );

            if (tvResult.IsPlayable)
                return CreateResponse(request, tvResult.Json!);
        }

        throw CreateException(videoId, tvResult, result with { Client = clientName });
    }

    private static HttpResponseMessage CreateResponse(HttpRequestMessage request, string json) =>
        new(HttpStatusCode.OK)
        {
            RequestMessage = request,
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private static bool ShouldSolveChallenges(string? clientName, string responseBody) =>
        // Ciphered streams can only be deciphered using the player
        responseBody.Contains("\"signatureCipher\"", StringComparison.Ordinal)
        || responseBody.Contains("\"cipher\"", StringComparison.Ordinal)
        // Streams of web-based clients are throttled unless the "n" parameter is solved
        || clientName?.StartsWith("TVHTML5", StringComparison.OrdinalIgnoreCase) == true
        || clientName?.StartsWith("WEB", StringComparison.OrdinalIgnoreCase) == true;

    private async Task<PlayerResult> GetTvPlayerResultAsync(
        string videoId,
        string? visitorData,
        string? cookieHeader,
        IReadOnlyDictionary<string, string> cookies,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"{_identity}|{videoId}";

        lock (PlayerCache)
        {
            if (
                PlayerCache.TryGetValue(cacheKey, out var cached)
                && cached.ExpiresAt > DateTimeOffset.UtcNow
            )
            {
                return cached.Result;
            }
        }

        PlayerResult result;
        try
        {
            result = await FetchTvPlayerResultAsync(
                videoId,
                visitorData,
                cookieHeader,
                cookies,
                cancellationToken
            );
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            result = PlayerResult.Failure(ex.Message);
        }

        result = result with { Client = TvClientName };

        lock (PlayerCache)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var key in PlayerCache.Where(p => p.Value.ExpiresAt <= now).Select(p => p.Key).ToArray())
                PlayerCache.Remove(key);

            while (PlayerCache.Count >= PlayerCacheCapacity)
                PlayerCache.Remove(PlayerCache.MinBy(p => p.Value.ExpiresAt).Key);

            PlayerCache[cacheKey] = (
                now + (result.IsPlayable ? PlayableCacheDuration : UnplayableCacheDuration),
                result
            );
        }

        return result;
    }

    private async Task<PlayerResult> FetchTvPlayerResultAsync(
        string videoId,
        string? visitorData,
        string? cookieHeader,
        IReadOnlyDictionary<string, string> cookies,
        CancellationToken cancellationToken
    )
    {
        if (!PlayerChallengeSolver.IsAvailable)
            return PlayerResult.Failure("JavaScript engine is not available.");

        var player = await PlayerChallengeSolver.GetPlayerAsync(_http, cancellationToken);

        var client = new JsonObject
        {
            ["clientName"] = TvClientName,
            ["clientVersion"] = TvClientVersion,
            ["userAgent"] = TvUserAgent,
            ["hl"] = "en",
            ["timeZone"] = "UTC",
            ["utcOffsetMinutes"] = 0,
        };

        if (!string.IsNullOrWhiteSpace(visitorData))
            client["visitorData"] = visitorData;

        var body = new JsonObject
        {
            ["context"] = new JsonObject { ["client"] = client },
            ["videoId"] = videoId,
            ["playbackContext"] = new JsonObject
            {
                ["contentPlaybackContext"] = new JsonObject
                {
                    ["html5Preference"] = "HTML5_PREF_WANTS",
                    ["signatureTimestamp"] = player.SignatureTimestamp,
                },
            },
            ["contentCheckOk"] = true,
            ["racyCheckOk"] = true,
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player?prettyPrint=false"
        );

        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        request.Headers.TryAddWithoutValidation("User-Agent", TvUserAgent);
        request.Headers.TryAddWithoutValidation("X-YouTube-Client-Name", TvClientNameId);
        request.Headers.TryAddWithoutValidation("X-YouTube-Client-Version", TvClientVersion);
        request.Headers.TryAddWithoutValidation("Origin", Origin);

        if (!string.IsNullOrWhiteSpace(visitorData))
            request.Headers.TryAddWithoutValidation("X-Goog-Visitor-Id", visitorData);

        if (!string.IsNullOrWhiteSpace(cookieHeader))
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);

        if (_isAuthenticated && TryGenerateAuthorization(cookies) is { } authorization)
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
            request.Headers.TryAddWithoutValidation("X-Origin", Origin);
            request.Headers.TryAddWithoutValidation("X-Goog-AuthUser", "0");
        }

        using var response = await _http.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = TryGetErrorMessage(responseBody);
            return PlayerResult.Failure(
                $"HTTP {(int)response.StatusCode}" + (error is not null ? $": {error}" : "")
            );
        }

        return await ProcessPlayerResponseAsync(responseBody, true, cancellationToken, player);
    }

    private static string? TryGetErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            return document.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message)
                ? message.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<PlayerResult> ProcessPlayerResponseAsync(
        string responseBody,
        bool solveChallenges,
        CancellationToken cancellationToken,
        YoutubePlayer? player = null
    )
    {
        JsonObject root;
        try
        {
            root =
                JsonNode.Parse(responseBody) as JsonObject
                ?? throw new JsonException("Player response is not an object.");
        }
        catch (JsonException ex)
        {
            return PlayerResult.Failure($"Invalid player response: {ex.Message}");
        }

        var playability = root["playabilityStatus"] as JsonObject;
        var status = GetString(playability?["status"]);
        var reason = GetString(playability?["reason"]);

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
            return new PlayerResult(false, null, status, reason);

        if (!solveChallenges || root["streamingData"] is not JsonObject streamingData)
            return new PlayerResult(true, responseBody, status, reason);

        if (!PlayerChallengeSolver.IsAvailable)
        {
            // Let YoutubeExplode handle the response on its own
            return new PlayerResult(true, responseBody, status, reason);
        }

        player ??= await PlayerChallengeSolver.GetPlayerAsync(_http, cancellationToken);

        var formats = new List<(JsonArray Container, JsonObject Format, string Url, string? Signature, string SignatureParameter)>();
        var nChallenges = new HashSet<string>(StringComparer.Ordinal);
        var signatureChallenges = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in new[] { "formats", "adaptiveFormats" })
        {
            if (streamingData[key] is not JsonArray container)
                continue;

            foreach (var format in container.OfType<JsonObject>())
            {
                var url = GetString(format["url"]);
                string? signature = null;
                var signatureParameter = "signature";

                if (
                    string.IsNullOrWhiteSpace(url)
                    && (GetString(format["signatureCipher"]) ?? GetString(format["cipher"]))
                        is { } cipher
                )
                {
                    var cipherData = ParseQuery(cipher);
                    url = cipherData.GetValueOrDefault("url");
                    signature = cipherData.GetValueOrDefault("s");
                    signatureParameter = cipherData.GetValueOrDefault("sp") ?? signatureParameter;
                }

                // Streams without URL are only available through SABR, which isn't supported
                if (string.IsNullOrWhiteSpace(url))
                    continue;

                if (TryGetQueryParameter(url, "n") is { } n)
                    nChallenges.Add(n);

                if (!string.IsNullOrWhiteSpace(signature))
                    signatureChallenges.Add(signature);

                formats.Add((container, format, url, signature, signatureParameter));
            }
        }

        var dashManifestUrl = GetString(streamingData["dashManifestUrl"]);
        var dashManifestN = dashManifestUrl is not null
            ? DashManifestNRegex().Match(dashManifestUrl) is { Success: true } match
                ? match.Groups[1].Value
                : null
            : null;

        if (dashManifestN is not null)
            nChallenges.Add(dashManifestN);

        var solutions = await PlayerChallengeSolver.SolveAsync(
            player,
            nChallenges,
            signatureChallenges,
            cancellationToken
        );

        var playableFormatCount = 0;
        foreach (var (container, format, originalUrl, signature, signatureParameter) in formats)
        {
            var url = originalUrl;

            if (!string.IsNullOrWhiteSpace(signature))
            {
                if (!solutions.Signatures.TryGetValue(signature, out var solvedSignature))
                {
                    container.Remove(format);
                    continue;
                }

                url = SetQueryParameter(url, signatureParameter, solvedSignature);
            }

            if (TryGetQueryParameter(url, "n") is { } n)
            {
                if (!solutions.N.TryGetValue(n, out var solvedN))
                {
                    container.Remove(format);
                    continue;
                }

                url = SetQueryParameter(url, "n", solvedN);
            }

            format["url"] = url;
            format.Remove("signatureCipher");
            format.Remove("cipher");
            playableFormatCount++;
        }

        if (dashManifestUrl is not null)
        {
            if (dashManifestN is null)
            {
                // Nothing to solve
            }
            else if (solutions.N.TryGetValue(dashManifestN, out var solvedN))
            {
                streamingData["dashManifestUrl"] = dashManifestUrl.Replace(
                    $"/n/{dashManifestN}/",
                    $"/n/{solvedN}/",
                    StringComparison.Ordinal
                );
            }
            else
            {
                streamingData.Remove("dashManifestUrl");
            }
        }

        if (playableFormatCount <= 0 && GetString(streamingData["hlsManifestUrl"]) is null)
        {
            return new PlayerResult(
                false,
                null,
                status,
                "Failed to solve the challenges of YouTube's player for any of the streams."
            );
        }

        return new PlayerResult(true, root.ToJsonString(), status, reason);
    }

    private static string? GetString(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var result) ? result : null;

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var parameter in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = parameter.IndexOf('=');
            var name = separatorIndex >= 0 ? parameter[..separatorIndex] : parameter;
            var value = separatorIndex >= 0 ? parameter[(separatorIndex + 1)..] : "";

            result[Unescape(name)] = Unescape(value);
        }

        return result;
    }

    private static string Unescape(string value) => Uri.UnescapeDataString(value.Replace('+', ' '));

    private static string? TryGetQueryParameter(string url, string name)
    {
        var match = Regex.Match(url, $@"[?&]{Regex.Escape(name)}=([^&#]*)");
        return match.Success ? Unescape(match.Groups[1].Value) : null;
    }

    private static string SetQueryParameter(string url, string name, string value)
    {
        var parameter = $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
        var regex = new Regex($@"(?<=[?&]){Regex.Escape(name)}=[^&#]*");

        if (regex.IsMatch(url))
            return regex.Replace(url, parameter.Replace("$", "$$", StringComparison.Ordinal), 1);

        return url + (url.Contains('?', StringComparison.Ordinal) ? "&" : "?") + parameter;
    }

    private static bool IsSignInRequired(PlayerResult result) =>
        string.Equals(result.Status, "LOGIN_REQUIRED", StringComparison.OrdinalIgnoreCase)
        || string.Equals(result.Status, "AGE_CHECK_REQUIRED", StringComparison.OrdinalIgnoreCase)
        || result.Reason?.Contains("sign in", StringComparison.OrdinalIgnoreCase) == true
        || result.Reason?.Contains("not a bot", StringComparison.OrdinalIgnoreCase) == true;

    private static Exception CreateException(
        string videoId,
        PlayerResult? tvResult,
        PlayerResult originalResult
    )
    {
        var details = new[] { tvResult, originalResult }
            .Where(r => r is not null)
            .Select(r => r!.Describe())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var message = $"Video '{videoId}' is not available. Reason: {string.Join("; ", details)}";

        var isSignInRequired =
            IsSignInRequired(originalResult) || (tvResult is not null && IsSignInRequired(tvResult));

        return isSignInRequired
            ? new SignInRequiredException(message)
            : new VideoUnavailableException(message);
    }

    [GeneratedRegex(@"/n/([^/]+)/")]
    private static partial Regex DashManifestNRegex();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _http.Dispose();

        base.Dispose(disposing);
    }

    private record PlayerResult(bool IsPlayable, string? Json, string? Status, string? Reason)
    {
        public string? Client { get; init; }

        public static PlayerResult Failure(string reason) => new(false, null, null, reason);

        public string Describe() =>
            (Client is not null ? $"[{Client}] " : "")
            + (Status is not null ? $"{Status}: " : "")
            + (Reason ?? "unknown error");
    }
}
