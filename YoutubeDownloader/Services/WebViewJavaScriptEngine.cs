using System.Text.Json;
using Android.Webkit;
using YoutubeDownloader.Core.Youtube;
using AndroidWebView = Android.Webkit.WebView;

namespace YoutubeDownloader.Services;

// Runs JavaScript inside a hidden web view, which is powered by Chromium's V8 engine.
// Used to solve the challenges imposed by YouTube's player (see PlayerChallengeSolver).
public class WebViewJavaScriptEngine : IJavaScriptEngine
{
    private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan EvaluationTimeout = TimeSpan.FromMinutes(2);

    private readonly SemaphoreSlim _lock = new(1, 1);

    private volatile AndroidWebView? _webView;
    private volatile TaskCompletionSource<string?>? _pendingEvaluation;

    private async Task<AndroidWebView> GetWebViewAsync(CancellationToken cancellationToken)
    {
        if (_webView is not null)
            return _webView;

        var pageLoad = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var webView = await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var view = new AndroidWebView(Platform.AppContext);

            // The web view only runs the scripts passed to it, so it doesn't need access to anything else
            view.Settings.JavaScriptEnabled = true;
            view.Settings.BlockNetworkLoads = true;
            view.Settings.AllowFileAccess = false;
            view.Settings.AllowContentAccess = false;

            view.SetWebViewClient(new Client(this, pageLoad));
            view.LoadDataWithBaseURL(
                null,
                "<!DOCTYPE html><html><head></head><body></body></html>",
                "text/html",
                "utf-8",
                null
            );

            return view;
        });

        _webView = webView;

        try
        {
            await pageLoad.Task.WaitAsync(PageLoadTimeout, cancellationToken);
        }
        catch
        {
            await DestroyWebViewAsync();
            throw;
        }

        return webView;
    }

    private async Task DestroyWebViewAsync()
    {
        if (_webView is not { } webView)
            return;

        _webView = null;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(webView.Destroy);
        }
        catch
        {
            // Ignore
        }
    }

    private static string? DecodeResult(string? rawResult)
    {
        // The result is serialized as JSON
        if (string.IsNullOrWhiteSpace(rawResult))
            return null;

        using var document = JsonDocument.Parse(rawResult);
        return document.RootElement.ValueKind == JsonValueKind.String
            ? document.RootElement.GetString()
            : null;
    }

    public async Task<string?> EvaluateAsync(
        string script,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var webView = await GetWebViewAsync(cancellationToken);

            var evaluation = new TaskCompletionSource<string?>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            _pendingEvaluation = evaluation;

            await MainThread.InvokeOnMainThreadAsync(() =>
                webView.EvaluateJavascript(script, new ValueCallback(evaluation))
            );

            return DecodeResult(await evaluation.Task.WaitAsync(EvaluationTimeout, cancellationToken));
        }
        catch
        {
            // Start from scratch next time, in case the web view is in a broken state
            await DestroyWebViewAsync();
            throw;
        }
        finally
        {
            _pendingEvaluation = null;
            _lock.Release();
        }
    }

    // Called on the main thread
    private void OnRenderProcessGone(AndroidWebView? view)
    {
        // The web view can't be used anymore, even if the renderer was killed while idle
        // (e.g. by the system while the app was in background), so a new one is created next time
        if (view is not null && ReferenceEquals(_webView, view))
            _webView = null;

        _pendingEvaluation?.TrySetException(
            new InvalidOperationException("The web view used to run JavaScript has crashed.")
        );

        try
        {
            view?.Destroy();
        }
        catch
        {
            // Ignore
        }
    }

    private class ValueCallback(TaskCompletionSource<string?> evaluation)
        : Java.Lang.Object,
            IValueCallback
    {
        public void OnReceiveValue(Java.Lang.Object? value) =>
            evaluation.TrySetResult(value?.ToString());
    }

    private class Client(WebViewJavaScriptEngine engine, TaskCompletionSource pageLoad)
        : WebViewClient
    {
        public override void OnPageFinished(AndroidWebView? view, string? url)
        {
            base.OnPageFinished(view, url);
            pageLoad.TrySetResult();
        }

        // Without handling this, the whole app would be terminated if the web view's renderer crashes
        // (for example, due to running out of memory)
        public override bool OnRenderProcessGone(AndroidWebView? view, RenderProcessGoneDetail? detail)
        {
            engine.OnRenderProcessGone(view);
            pageLoad.TrySetException(
                new InvalidOperationException("The web view used to run JavaScript has crashed.")
            );

            return true;
        }
    }
}
