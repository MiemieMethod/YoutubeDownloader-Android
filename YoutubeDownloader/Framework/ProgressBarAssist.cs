namespace YoutubeDownloader.Framework;

// MAUI's progress bar does not support the indeterminate mode out of the box,
// so it's exposed as an attached property and mapped to the native control.
public static class ProgressBarAssist
{
    public static readonly BindableProperty IsIndeterminateProperty =
        BindableProperty.CreateAttached(
            "IsIndeterminate",
            typeof(bool),
            typeof(ProgressBarAssist),
            false,
            propertyChanged: (bindable, _, _) =>
                (bindable as ProgressBar)?.Handler?.UpdateValue("IsIndeterminate")
        );

    public static bool GetIsIndeterminate(BindableObject view) =>
        (bool)view.GetValue(IsIndeterminateProperty);

    public static void SetIsIndeterminate(BindableObject view, bool value) =>
        view.SetValue(IsIndeterminateProperty, value);

    public static void Register() =>
        Microsoft.Maui.Handlers.ProgressBarHandler.Mapper.AppendToMapping(
            "IsIndeterminate",
            (handler, view) =>
            {
                if (view is ProgressBar progressBar)
                    handler.PlatformView.Indeterminate = GetIsIndeterminate(progressBar);
            }
        );
}
