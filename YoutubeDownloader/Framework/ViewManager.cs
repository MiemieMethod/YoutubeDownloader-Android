using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views.Dialogs;

namespace YoutubeDownloader.Framework;

public class ViewManager
{
    private static Page? TryCreateView(object viewModel) =>
        viewModel switch
        {
            AuthSetupViewModel => new AuthSetupPage(),
            DownloadMultipleSetupViewModel => new DownloadMultipleSetupPage(),
            DownloadSingleSetupViewModel => new DownloadSingleSetupPage(),
            SettingsViewModel => new SettingsPage(),
            _ => null,
        };

    public Page? TryBindView(object viewModel)
    {
        var view = TryCreateView(viewModel);
        if (view is null)
            return null;

        view.BindingContext = viewModel;
        return view;
    }
}
