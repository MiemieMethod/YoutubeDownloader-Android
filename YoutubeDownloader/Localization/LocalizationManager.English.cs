using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> EnglishLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL or search query",
            [nameof(QueryTooltip)] =
                "Any valid YouTube URL or ID is accepted. Prepend a question mark (?) to perform search by text.",
            [nameof(ProcessQueryTooltip)] = "Process query (Enter)",
            [nameof(AuthTooltip)] = "Authentication",
            [nameof(SettingsTooltip)] = "Settings",
            [nameof(DashboardPlaceholder)] = """
                Copy-paste a **URL** or enter a **search query** to start downloading
                Put each item on a **new line** to add multiple items
                """,
            [nameof(ContextMenuRemoveSuccessful)] = "Remove successful downloads",
            [nameof(ContextMenuRemoveInactive)] = "Remove inactive downloads",
            [nameof(ContextMenuRestartFailed)] = "Restart failed downloads",
            [nameof(ContextMenuCancelAll)] = "Cancel all downloads",
            [nameof(DownloadStatusEnqueued)] = "Pending...",
            [nameof(DownloadStatusCompleted)] = "Done",
            [nameof(DownloadStatusCanceled)] = "Canceled",
            [nameof(DownloadStatusFailed)] = "Failed",
            [nameof(PlayTooltip)] = "Play",
            [nameof(CancelDownloadTooltip)] = "Cancel download",
            [nameof(RestartDownloadTooltip)] = "Restart download",
            // Settings
            [nameof(SettingsTitle)] = "Settings",
            [nameof(ThemeLabel)] = "Theme",
            [nameof(ThemeTooltip)] = "Preferred user interface theme",
            [nameof(LanguageLabel)] = "Language",
            [nameof(LanguageTooltip)] = "Preferred display language for the user interface",
            [nameof(AutoUpdateLabel)] = "Check for updates",
            [nameof(AutoUpdateTooltip)] = """
                Check for new versions on every launch.
                **Note:** it's recommended to keep the app up to date to ensure that it's compatible with the latest version of YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Persist authentication",
            [nameof(PersistAuthTooltip)] = """
                Save authentication cookies to a file so that they can be persisted between sessions.
                **Warning**: although the cookies are stored with encryption, they may still be recovered by an attacker who has access to your system.
                """,
            [nameof(InjectAltLanguagesLabel)] = "Inject alternative languages",
            [nameof(InjectAltLanguagesTooltip)] =
                "Inject audio tracks in alternative languages (if available) into downloaded files",
            [nameof(InjectSubtitlesLabel)] = "Inject subtitles",
            [nameof(InjectSubtitlesTooltip)] =
                "Inject subtitles (if available) into downloaded files",
            [nameof(InjectTagsLabel)] = "Inject media tags",
            [nameof(InjectTagsTooltip)] = "Inject media tags (if available) into downloaded files",
            [nameof(SkipExistingFilesLabel)] = "Skip existing files",
            [nameof(SkipExistingFilesTooltip)] =
                "When downloading multiple videos, skip those that already have matching files in the output directory",
            [nameof(FileNameTemplateLabel)] = "File name template",
            [nameof(FileNameTemplateTooltip)] = """
                Template used for generating file names for downloaded videos.

                Available tokens:
                **$num** — video's position in the list (if applicable)
                **$id** — video ID
                **$title** — video title
                **$author** — video author
                """,
            [nameof(ParallelLimitLabel)] = "Parallel limit",
            [nameof(ParallelLimitTooltip)] = "How many downloads can be active at the same time",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Authentication",
            [nameof(AuthenticatedText)] = "You are currently authenticated",
            [nameof(LogOutButton)] = "Log out",
            [nameof(AuthenticationPlaceholderText)] = """
                Loading...

                If the page does not appear, make sure that Android System WebView is installed and up to date.
                """,
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Copy",
            [nameof(LiveLabel)] = "Live",
            [nameof(AudioLabel)] = "Audio",
            [nameof(UpscaledLabel)] = "Upscaled",
            [nameof(FormatLabel)] = "Format",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Container",
            [nameof(VideoQualityLabel)] = "Video quality",
            // Common buttons
            [nameof(CloseButton)] = "CLOSE",
            [nameof(DownloadButton)] = "DOWNLOAD",
            [nameof(CancelButton)] = "CANCEL",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Thank you for supporting Ukraine!",
            [nameof(UkraineSupportMessage)] = """
                As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

                Click LEARN MORE to find ways that you can help.
                """,
            [nameof(LearnMoreButton)] = "LEARN MORE",
            [nameof(FFmpegMissingTitle)] = "FFmpeg is missing",
            [nameof(FFmpegMissingMessage)] = """
                The FFmpeg executable bundled with {0} could not be found or launched. It is required for downloading videos.

                Please reinstall the app to fix this problem.
                """,
            [nameof(NothingFoundTitle)] = "Nothing found",
            [nameof(NothingFoundMessage)] =
                "Couldn't find any videos based on the query or URL you provided",
            [nameof(ErrorTitle)] = "Error",
            // Android
            [nameof(DownloadDirectoryLabel)] = "Download folder",
            [nameof(DownloadDirectoryTooltip)] = "Folder where downloaded files are saved",
            [nameof(DownloadDirectoryBrowseTooltip)] = "Choose folder",
            [nameof(DownloadDirectoryResetTooltip)] = "Reset to default folder",
            [nameof(FileNameLabel)] = "File name",
            [nameof(ShareTooltip)] = "Share",
            [nameof(MoreOptionsTooltip)] = "More options",
            [nameof(SelectAllTooltip)] = "Select all",
            [nameof(CopyButton)] = "COPY",
            [nameof(NoAppToOpenFileMessage)] = "No app found to open this file",
            [nameof(StoragePermissionDeniedMessage)] = "Storage permission is required to save downloaded files",
            [nameof(UpdateAvailableMessage)] = "{0} v{1} is available",
            [nameof(UpdateCheckFailedMessage)] = "Failed to check for updates",
            [nameof(DownloadsNotificationChannelName)] = "Downloads",
            [nameof(DownloadsNotificationText)] = "Active downloads: {0}",
        };
}
