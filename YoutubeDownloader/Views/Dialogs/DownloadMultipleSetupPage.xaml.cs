using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class DownloadMultipleSetupPage : ContentPage
{
    public DownloadMultipleSetupPage() => InitializeComponent();

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as DownloadMultipleSetupViewModel)?.CloseCommand.Execute(null);
        return true;
    }
}
