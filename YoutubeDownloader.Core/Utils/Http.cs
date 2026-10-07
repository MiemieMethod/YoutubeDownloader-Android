using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using PowerKit.Extensions;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    // On Android, the default handler (AndroidMessageHandler) overwrites the "Cookie" header
    // set by YoutubeExplode with the contents of its own cookie container whenever cookies are
    // enabled, which would break authentication. Cookies are managed by YoutubeExplode itself,
    // so the handler's cookie handling is disabled.
    public static HttpClient Client { get; } =
        new(new HttpClientHandler { UseCookies = false })
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
}
