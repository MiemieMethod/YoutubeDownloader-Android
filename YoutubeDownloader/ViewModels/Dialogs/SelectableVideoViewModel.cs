using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class SelectableVideoViewModel(IVideo video, bool isSelected) : ObservableObject
{
    public IVideo Video { get; } = video;

    public string Details =>
        Video.Duration is { } duration
            ? $"{Video.Author.ChannelTitle} · {duration.ToString(duration.TotalHours >= 1 ? @"h\:mm\:ss" : @"m\:ss")}"
            : Video.Author.ChannelTitle;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = isSelected;

    [RelayCommand]
    private void Toggle() => IsSelected = !IsSelected;
}
