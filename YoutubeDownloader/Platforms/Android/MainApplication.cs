using Android.App;
using Android.OS;
using Android.Runtime;

namespace YoutubeDownloader;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : MauiApplication(handle, ownership)
{
    public override void OnCreate()
    {
        // Network operations are performed in background, but libraries may still dispose HTTP
        // responses on the main thread, which makes the system's HTTP stack read the rest of the
        // response body (briefly) to reuse the connection. By default, Android crashes the operation
        // with NetworkOnMainThreadException in that case, so this is allowed instead.
        StrictMode.SetThreadPolicy(
            new StrictMode.ThreadPolicy.Builder(StrictMode.GetThreadPolicy()).PermitNetwork()!.Build()
        );

        base.OnCreate();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
