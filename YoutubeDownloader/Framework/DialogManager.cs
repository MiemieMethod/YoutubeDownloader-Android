using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Framework;

public class DialogManager(ViewManager viewManager) : IDisposable
{
    private readonly SemaphoreSlim _dialogLock = new(1, 1);

    private static Page? GetRootPage() => Application.Current?.Windows.FirstOrDefault()?.Page;

    private static Page? GetTopPage()
    {
        var rootPage = GetRootPage();
        return rootPage?.Navigation.ModalStack.LastOrDefault() ?? rootPage;
    }

    private static async Task<bool?> ShowMessageBoxAsync(MessageBoxViewModel dialog)
    {
        if (GetTopPage() is not { } page)
            return null;

        var title = dialog.Title ?? string.Empty;
        var message = dialog.Message ?? string.Empty;

        if (dialog.IsDefaultButtonVisible && dialog.IsCancelButtonVisible)
        {
            return await page.DisplayAlertAsync(
                title,
                message,
                dialog.DefaultButtonText,
                dialog.CancelButtonText
            );
        }

        await page.DisplayAlertAsync(
            title,
            message,
            dialog.DefaultButtonText ?? dialog.CancelButtonText ?? "OK"
        );

        return dialog.IsDefaultButtonVisible;
    }

    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
    {
        await _dialogLock.WaitAsync();
        try
        {
            if (dialog is MessageBoxViewModel messageBox)
                return (T?)(object?)await ShowMessageBoxAsync(messageBox);

            var rootPage =
                GetRootPage()
                ?? throw new InvalidOperationException("The application window is not available.");

            var view =
                viewManager.TryBindView(dialog)
                ?? throw new InvalidOperationException(
                    $"View for dialog '{dialog.GetType().Name}' is not registered."
                );

            await dialog.InitializeAsync();
            await rootPage.Navigation.PushModalAsync(view, true);

            try
            {
                return await dialog.WaitForCloseAsync();
            }
            finally
            {
                if (rootPage.Navigation.ModalStack.Contains(view))
                    await rootPage.Navigation.PopModalAsync(true);
            }
        }
        finally
        {
            _dialogLock.Release();
        }
    }

    public void Dispose() => _dialogLock.Dispose();
}
