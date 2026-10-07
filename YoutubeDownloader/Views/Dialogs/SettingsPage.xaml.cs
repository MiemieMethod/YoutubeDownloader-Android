using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class SettingsPage : ContentPage
{
    public SettingsPage() => InitializeComponent();

    protected override bool OnBackButtonPressed()
    {
        (BindingContext as SettingsViewModel)?.CloseCommand.Execute(null);
        return true;
    }
}
