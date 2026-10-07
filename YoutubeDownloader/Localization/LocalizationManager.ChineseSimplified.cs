using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> ChineseSimplifiedLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "粘贴链接或搜索",
            [nameof(QueryTooltip)] =
                "接受任何有效的 YouTube 网页链接或视频 ID。在开头添加问号 (?) 可进行文本搜索。",
            [nameof(ProcessQueryTooltip)] = "开始查询 (Enter)",
            [nameof(AuthTooltip)] = "身份验证",
            [nameof(SettingsTooltip)] = "设置",
            [nameof(DashboardPlaceholder)] = """
                复制并粘贴 **网页链接** 或 **搜索** 以开始下载
                每行输入一个项目（使用**换行**分隔）可添加多个项目
                """,
            [nameof(ContextMenuRemoveSuccessful)] = "移除已完成的下载",
            [nameof(ContextMenuRemoveInactive)] = "移除未激活的下载",
            [nameof(ContextMenuRestartFailed)] = "重下失败的下载",
            [nameof(ContextMenuCancelAll)] = "取消所有下载",
            [nameof(DownloadStatusEnqueued)] = "正在排队...",
            [nameof(DownloadStatusCompleted)] = "已完成",
            [nameof(DownloadStatusCanceled)] = "已取消",
            [nameof(DownloadStatusFailed)] = "失败",
            [nameof(PlayTooltip)] = "播放",
            [nameof(CancelDownloadTooltip)] = "取消下载",
            [nameof(RestartDownloadTooltip)] = "重新下载",
            // Settings
            [nameof(SettingsTitle)] = "设置",
            [nameof(ThemeLabel)] = "主题",
            [nameof(ThemeTooltip)] = "首选的用户界面主题",
            [nameof(LanguageLabel)] = "语言",
            [nameof(LanguageTooltip)] = "首选的用户界面显示语言",
            [nameof(AutoUpdateLabel)] = "检查更新",
            [nameof(AutoUpdateTooltip)] = """
                每次启动时检查新版本。
                **注意：** 建议保持应用为最新版本，以确保其与最新版本的 YouTube 兼容。
                """,
            [nameof(PersistAuthLabel)] = "持久化身份验证",
            [nameof(PersistAuthTooltip)] = """
                将身份验证 Cookie 保存到文件中，以便在不同会话之间持久化。
                **警告：** 尽管 Cookie 是加密存储的，但拥有系统访问权限的攻击者仍可能恢复它们。
                """,
            [nameof(InjectAltLanguagesLabel)] = "注入多音轨",
            [nameof(InjectAltLanguagesTooltip)] =
                "将替代语言的音频轨道（如果可用）注入到下载的文件中",
            [nameof(InjectSubtitlesLabel)] = "注入字幕",
            [nameof(InjectSubtitlesTooltip)] = "将字幕（如果可用）注入到下载的文件中",
            [nameof(InjectTagsLabel)] = "注入媒体标签",
            [nameof(InjectTagsTooltip)] = "将媒体标签（如果可用）注入到下载的文件中",
            [nameof(SkipExistingFilesLabel)] = "跳过已存在的文件",
            [nameof(SkipExistingFilesTooltip)] =
                "下载多个视频时，跳过输出目录中已存在匹配文件的视频",
            [nameof(FileNameTemplateLabel)] = "文件名模板",
            [nameof(FileNameTemplateTooltip)] = """
                用于生成下载视频文件名的模板。

                可用标记：
                **$num** — 视频在列表中的位置（如果适用）
                **$id** — 视频 ID
                **$title** — 视频标题
                **$author** — 视频作者
                """,
            [nameof(ParallelLimitLabel)] = "并行下载限制",
            [nameof(ParallelLimitTooltip)] = "允许同时进行的下载任务数量",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "身份验证",
            [nameof(AuthenticatedText)] = "你当前已通过身份验证",
            [nameof(LogOutButton)] = "退出登录",
            [nameof(AuthenticationPlaceholderText)] = """
                正在加载...

                如果页面未显示，请确保已安装并更新 Android System WebView。
                """,
            // Download Single Setup
            [nameof(CopyMenuItem)] = "复制",
            [nameof(LiveLabel)] = "直播",
            [nameof(AudioLabel)] = "音频",
            [nameof(UpscaledLabel)] = "超分辨率",
            [nameof(FormatLabel)] = "格式",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "封装格式",
            [nameof(VideoQualityLabel)] = "视频质量",
            // Common buttons
            [nameof(CloseButton)] = "关闭",
            [nameof(DownloadButton)] = "下载",
            [nameof(CancelButton)] = "取消",
            // Dialog messages
            [nameof(WelcomeTitle)] = "欢迎使用 {0}！",
            [nameof(WelcomeMessage)] = """
                本应用免费且开源。如果你觉得它好用，欢迎到 GitHub 仓库点个 Star 支持一下，也欢迎推荐给身边的朋友。

                同样欢迎反馈问题、提出建议或贡献代码，一起来完善本应用！

                {0}
                """,
            [nameof(OpenProjectButton)] = "前往仓库",
            [nameof(FFmpegMissingTitle)] = "缺少 FFmpeg",
            [nameof(FFmpegMissingMessage)] = """
                找不到或无法启动 {0} 内置的 FFmpeg 可执行文件。下载视频需要它。

                请重新安装应用以解决此问题。
                """,
            [nameof(NothingFoundTitle)] = "未找到内容",
            [nameof(NothingFoundMessage)] = "无法根据你提供的查询或 URL 找到任何视频",
            [nameof(ErrorTitle)] = "错误",
            // Android
            [nameof(DownloadDirectoryLabel)] = "下载文件夹",
            [nameof(DownloadDirectoryTooltip)] = "保存下载文件的文件夹",
            [nameof(DownloadDirectoryBrowseTooltip)] = "选择文件夹",
            [nameof(DownloadDirectoryResetTooltip)] = "重置为默认文件夹",
            [nameof(FileNameLabel)] = "文件名",
            [nameof(ShareTooltip)] = "分享",
            [nameof(MoreOptionsTooltip)] = "更多选项",
            [nameof(SelectAllTooltip)] = "全选",
            [nameof(CopyButton)] = "复制",
            [nameof(NoAppToOpenFileMessage)] = "未找到可打开此文件的应用",
            [nameof(SignInRequiredMessage)] = "YouTube 要求登录后才能访问此视频（例如需要确认你不是机器人）。请点击主界面顶部的身份验证按钮（带钥匙的人像图标）登录 Google 账号，然后重试。",
            [nameof(StoragePermissionDeniedMessage)] = "需要存储权限才能保存下载的文件",
            [nameof(UpdateAvailableMessage)] = "{0} v{1} 已发布",
            [nameof(UpdateCheckFailedMessage)] = "检查更新失败",
            [nameof(DownloadsNotificationChannelName)] = "下载",
            [nameof(DownloadsNotificationText)] = "正在下载：{0} 项",
        };
}
