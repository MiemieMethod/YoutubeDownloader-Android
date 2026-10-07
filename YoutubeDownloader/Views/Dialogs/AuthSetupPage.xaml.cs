using System.Net;
using YoutubeDownloader.ViewModels.Dialogs;
using AndroidCookieManager = Android.Webkit.CookieManager;
using AndroidWebView = Android.Webkit.WebView;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupPage : ContentPage
{
    private const string HomePageUrl = "https://www.youtube.com";

    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={Uri.EscapeDataString(HomePageUrl)}";

    public AuthSetupPage()
    {
        InitializeComponent();
        LoginWebView.HandlerChanged += LoginWebView_OnHandlerChanged;
    }

    private AuthSetupViewModel? ViewModel => BindingContext as AuthSetupViewModel;

    private void LoginWebView_OnHandlerChanged(object? sender, EventArgs args)
    {
        if (LoginWebView.Handler?.PlatformView is not AndroidWebView webView)
            return;

        webView.Settings.JavaScriptEnabled = true;
        webView.Settings.DomStorageEnabled = true;

        // Google refuses to sign in from embedded web views, so remove the markers that identify them
        webView.Settings.UserAgentString = webView
            .Settings.UserAgentString?.Replace("; wv", "", StringComparison.Ordinal)
            .Replace("Version/4.0 ", "", StringComparison.Ordinal);

        AndroidCookieManager.Instance?.SetAcceptCookie(true);
        AndroidCookieManager.Instance?.SetAcceptThirdPartyCookies(webView, true);
    }

    private void NavigateToLoginPage()
    {
        // Clear existing cookies so that the user can sign in from scratch
        var cookieManager = AndroidCookieManager.Instance;
        cookieManager?.RemoveAllCookies(null);
        cookieManager?.Flush();

        LoadingLabel.IsVisible = true;
        LoginWebView.Source = new UrlWebViewSource { Url = LoginPageUrl };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel?.IsAuthenticated != true)
            NavigateToLoginPage();
    }

    private static IReadOnlyList<Cookie> GetCookies()
    {
        var cookies = new List<Cookie>();

        var rawCookies = AndroidCookieManager.Instance?.GetCookie(HomePageUrl);
        if (string.IsNullOrWhiteSpace(rawCookies))
            return cookies;

        foreach (var rawCookie in rawCookies.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = rawCookie.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var name = rawCookie[..separatorIndex].Trim();
            var value = rawCookie[(separatorIndex + 1)..].Trim();

            try
            {
                cookies.Add(new Cookie(name, value, "/", ".youtube.com"));
            }
            catch (CookieException)
            {
                // Skip cookies that can't be represented
            }
        }

        return cookies;
    }

    private void LoginWebView_OnNavigated(object? sender, WebNavigatedEventArgs args)
    {
        LoadingLabel.IsVisible = false;

        if (ViewModel is not { } viewModel || viewModel.IsAuthenticated)
            return;

        if (!Uri.TryCreate(args.Url, UriKind.Absolute, out var uri))
            return;

        // Wait until the user has signed in and was redirected back to YouTube
        if (
            !string.Equals(uri.Host, "youtube.com", StringComparison.OrdinalIgnoreCase)
            && !uri.Host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        var cookies = GetCookies();

        // Only accept the cookies if they contain the session information
        if (
            !cookies.Any(c =>
                string.Equals(c.Name, "SAPISID", StringComparison.Ordinal)
                || (
                    c.Name.StartsWith("__Secure-", StringComparison.Ordinal)
                    && c.Name.EndsWith("PSID", StringComparison.Ordinal)
                )
            )
        )
        {
            return;
        }

        viewModel.Cookies = cookies;
    }

    private void LogOutButton_OnClicked(object? sender, EventArgs args)
    {
        if (ViewModel is not { } viewModel)
            return;

        viewModel.Cookies = null;
        NavigateToLoginPage();
    }

    protected override bool OnBackButtonPressed()
    {
        if (ViewModel?.IsAuthenticated != true && LoginWebView.CanGoBack)
        {
            LoginWebView.GoBack();
            return true;
        }

        ViewModel?.CloseCommand.Execute(null);
        return true;
    }
}
