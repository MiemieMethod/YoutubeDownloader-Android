using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class DownloadSingleSetupPage : ContentPage
{
    public DownloadSingleSetupPage() => InitializeComponent();

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as DownloadSingleSetupViewModel)?.CloseCommand.Execute(null);
        return true;
    }
}
