using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Youtube;

/// <summary>
/// JavaScript engine used to solve the challenges imposed by YouTube's player.
/// </summary>
/// <remarks>
/// The engine must preserve global state between evaluations.
/// </remarks>
public interface IJavaScriptEngine
{
    /// <summary>
    /// Evaluates the specified script and returns the value of its last expression,
    /// which is expected to be a string (or null).
    /// </summary>
    Task<string?> EvaluateAsync(string script, CancellationToken cancellationToken = default);
}
