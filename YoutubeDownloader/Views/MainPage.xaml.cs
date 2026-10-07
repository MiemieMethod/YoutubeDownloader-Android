using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.Views;

public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();

    private MainViewModel? ViewModel => BindingContext as MainViewModel;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel is { } viewModel)
            await viewModel.InitializeAsync();
    }

    private async void MoreOptionsButton_OnClicked(object? sender, EventArgs args)
    {
        if (ViewModel?.Dashboard is not { } dashboard)
            return;

        var localization = dashboard.LocalizationManager;

        var actions = new (string Title, Action Execute)[]
        {
            (
                localization.ContextMenuRemoveSuccessful,
                () => dashboard.RemoveSuccessfulDownloadsCommand.Execute(null)
            ),
            (
                localization.ContextMenuRemoveInactive,
                () => dashboard.RemoveInactiveDownloadsCommand.Execute(null)
            ),
            (
                localization.ContextMenuRestartFailed,
                () => dashboard.RestartFailedDownloadsCommand.Execute(null)
            ),
            (
                localization.ContextMenuCancelAll,
                () => dashboard.CancelAllDownloadsCommand.Execute(null)
            ),
        };

        var result = await DisplayActionSheetAsync(
            null,
            localization.CancelButton,
            null,
            actions.Select(a => a.Title).ToArray()
        );

        actions.FirstOrDefault(a => a.Title == result).Execute?.Invoke();
    }

    private void RestartButton_OnClicked(object? sender, EventArgs args)
    {
        if (
            sender is BindableObject { BindingContext: DownloadViewModel download }
            && ViewModel?.Dashboard is { } dashboard
        )
        {
            dashboard.RestartDownloadCommand.Execute(download);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        // Keep the app (and the downloads) running in the background instead of destroying the activity
        Platform.CurrentActivity?.MoveTaskToBack(true);
        return true;
    }
}
