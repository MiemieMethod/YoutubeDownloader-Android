using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.Core.Youtube;

/// <summary>
/// Exception thrown when YouTube refuses to serve a video without signing in
/// (for example, when it asks to "confirm you're not a bot").
/// </summary>
public class SignInRequiredException(string message) : VideoUnavailableException(message);
