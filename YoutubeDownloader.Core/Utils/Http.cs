using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using PowerKit.Extensions;
using YoutubeDownloader.Core.Youtube;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    // On Android, the default handler (AndroidMessageHandler) overwrites the "Cookie" header
    // set by YoutubeExplode with the contents of its own cookie container whenever cookies are
    // enabled, which would break authentication. Cookies are managed by YoutubeExplode itself,
    // so the handler's cookie handling is disabled.
    private static readonly HttpMessageHandler Handler = new HttpClientHandler
    {
        UseCookies = false,
    };

    public static HttpClient Client { get; } =
        new(Handler, false)
        {
            DefaultRequestHeaders =
            {
                // Required by some of the services we're using
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "YoutubeDownloader",
                        Assembly.GetExecutingAssembly().TryGetVersionString()
                    ),
                },
            },
        };

    // Client for YoutubeExplode, which adapts its requests to the current behavior of YouTube.
    // The returned client must be disposed by the caller.
    public static HttpClient CreateYoutubeClient(IReadOnlyList<Cookie>? cookies = null) =>
        new(new YoutubeRequestHandler(Handler, cookies ?? []), true);
}
