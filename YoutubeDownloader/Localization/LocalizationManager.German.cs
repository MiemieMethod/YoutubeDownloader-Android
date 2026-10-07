using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> GermanLocalization = new Dictionary<
        string,
        string
    >
    {
        // Dashboard
        [nameof(QueryPlaceholderText)] = "URL oder Suchanfrage",
        [nameof(QueryTooltip)] =
            "Jede gültige YouTube-URL oder -ID wird akzeptiert. Stellen Sie ein Fragezeichen (?) voran, um nach Text zu suchen.",
        [nameof(ProcessQueryTooltip)] = "Anfrage verarbeiten (Enter)",
        [nameof(AuthTooltip)] = "Authentifizierung",
        [nameof(SettingsTooltip)] = "Einstellungen",
        [nameof(DashboardPlaceholder)] = """
            **URL** einfügen oder **Suchanfrage** eingeben um den Download zu starten
            Geben Sie jeden Eintrag in einer **neuen Zeile** ein, um mehrere Einträge hinzuzufügen
            """,
        [nameof(ContextMenuRemoveSuccessful)] = "Erfolgreiche Downloads entfernen",
        [nameof(ContextMenuRemoveInactive)] = "Inaktive Downloads entfernen",
        [nameof(ContextMenuRestartFailed)] = "Fehlgeschlagene Downloads neu starten",
        [nameof(ContextMenuCancelAll)] = "Alle Downloads abbrechen",
        [nameof(DownloadStatusEnqueued)] = "Ausstehend...",
        [nameof(DownloadStatusCompleted)] = "Fertig",
        [nameof(DownloadStatusCanceled)] = "Abgebrochen",
        [nameof(DownloadStatusFailed)] = "Fehlgeschlagen",
        [nameof(PlayTooltip)] = "Abspielen",
        [nameof(CancelDownloadTooltip)] = "Download abbrechen",
        [nameof(RestartDownloadTooltip)] = "Download neu starten",
        // Settings
        [nameof(SettingsTitle)] = "Einstellungen",
        [nameof(ThemeLabel)] = "Design",
        [nameof(ThemeTooltip)] = "Bevorzugtes Oberflächendesign",
        [nameof(LanguageLabel)] = "Sprache",
        [nameof(LanguageTooltip)] = "Bevorzugte Anzeigesprache für die Benutzeroberfläche",
        [nameof(AutoUpdateLabel)] = "Nach Updates suchen",
        [nameof(AutoUpdateTooltip)] = """
            Bei jedem Start nach neuen Versionen suchen.
            **Hinweis:** Es wird empfohlen, die App aktuell zu halten, damit sie mit der neuesten Version von YouTube kompatibel bleibt.
            """,
        [nameof(PersistAuthLabel)] = "Authentifizierung speichern",
        [nameof(PersistAuthTooltip)] = """
            Authentifizierungs-Cookies in einer Datei speichern für sitzungsübergreifende Persistenz.
            **Warnung**: Die Cookies werden mit Verschlüsselung gespeichert, können aber dennoch von einem Angreifer mit Zugriff auf Ihr System wiederhergestellt werden.
            """,
        [nameof(InjectAltLanguagesLabel)] = "Alternative Sprachen einbetten",
        [nameof(InjectAltLanguagesTooltip)] =
            "Audiotracks in alternativen Sprachen (falls verfügbar) in heruntergeladene Dateien einbetten",
        [nameof(InjectSubtitlesLabel)] = "Untertitel einbetten",
        [nameof(InjectSubtitlesTooltip)] =
            "Untertitel (falls verfügbar) in heruntergeladene Dateien einbetten",
        [nameof(InjectTagsLabel)] = "Medien-Tags einbetten",
        [nameof(InjectTagsTooltip)] =
            "Medien-Tags (falls verfügbar) in heruntergeladene Dateien einbetten",
        [nameof(SkipExistingFilesLabel)] = "Vorhandene Dateien überspringen",
        [nameof(SkipExistingFilesTooltip)] =
            "Beim Herunterladen mehrerer Videos solche überspringen, für die bereits passende Dateien im Ausgabeverzeichnis vorhanden sind",
        [nameof(FileNameTemplateLabel)] = "Dateinamen-Vorlage",
        [nameof(FileNameTemplateTooltip)] = """
            Vorlage für die Generierung von Dateinamen heruntergeladener Videos.

            Verfügbare Token:
            **$num** — Position des Videos in der Liste (falls zutreffend)
            **$id** — Video-ID
            **$title** — Videotitel
            **$author** — Videoautor
            """,
        [nameof(ParallelLimitLabel)] = "Paralleles Limit",
        [nameof(ParallelLimitTooltip)] = "Wie viele Downloads gleichzeitig aktiv sein können",
        // Auth Setup
        [nameof(AuthenticationTitle)] = "Authentifizierung",
        [nameof(AuthenticatedText)] = "Sie sind derzeit authentifiziert",
        [nameof(LogOutButton)] = "Abmelden",
        [nameof(AuthenticationPlaceholderText)] = """
            Wird geladen...

            Wenn die Seite nicht angezeigt wird, stellen Sie sicher, dass Android System WebView installiert und aktuell ist.
            """,
        // Download Single Setup
        [nameof(CopyMenuItem)] = "Kopieren",
        [nameof(LiveLabel)] = "Live",
        [nameof(AudioLabel)] = "Audio",
        [nameof(UpscaledLabel)] = "Hochskaliert",
        [nameof(FormatLabel)] = "Format",
        // Download Multiple Setup
        [nameof(ContainerLabel)] = "Container",
        [nameof(VideoQualityLabel)] = "Videoqualität",
        // Common buttons
        [nameof(CloseButton)] = "SCHLIESSEN",
        [nameof(DownloadButton)] = "HERUNTERLADEN",
        [nameof(CancelButton)] = "ABBRECHEN",
        // Dialog messages
        [nameof(UkraineSupportTitle)] = "Danke für Ihre Unterstützung der Ukraine!",
        [nameof(UkraineSupportMessage)] = """
            Während Russland einen Vernichtungskrieg gegen mein Land führt, bin ich jedem dankbar, der weiterhin zur Ukraine in unserem Kampf für die Freiheit steht.

            Klicken Sie auf MEHR ERFAHREN um Wege zu finden, wie Sie helfen können.
            """,
        [nameof(LearnMoreButton)] = "MEHR ERFAHREN",
        [nameof(FFmpegMissingTitle)] = "FFmpeg fehlt",
        [nameof(FFmpegMissingMessage)] = """
            Die mit {0} gebündelte FFmpeg-Datei konnte nicht gefunden oder gestartet werden. Sie wird zum Herunterladen von Videos benötigt.

            Bitte installieren Sie die App neu, um dieses Problem zu beheben.
            """,
        [nameof(NothingFoundTitle)] = "Nichts gefunden",
        [nameof(NothingFoundMessage)] =
            "Es konnten keine Videos basierend auf der angegebenen Anfrage oder URL gefunden werden",
        [nameof(ErrorTitle)] = "Fehler",
        // Android
        [nameof(DownloadDirectoryLabel)] = "Download-Ordner",
        [nameof(DownloadDirectoryTooltip)] = "Ordner, in dem heruntergeladene Dateien gespeichert werden",
        [nameof(DownloadDirectoryBrowseTooltip)] = "Ordner auswählen",
        [nameof(DownloadDirectoryResetTooltip)] = "Auf Standardordner zurücksetzen",
        [nameof(FileNameLabel)] = "Dateiname",
        [nameof(ShareTooltip)] = "Teilen",
        [nameof(MoreOptionsTooltip)] = "Weitere Optionen",
        [nameof(SelectAllTooltip)] = "Alle auswählen",
        [nameof(CopyButton)] = "KOPIEREN",
        [nameof(NoAppToOpenFileMessage)] = "Keine App zum Öffnen dieser Datei gefunden",
        [nameof(StoragePermissionDeniedMessage)] = "Zum Speichern heruntergeladener Dateien ist die Speicherberechtigung erforderlich",
        [nameof(UpdateAvailableMessage)] = "{0} v{1} ist verfügbar",
        [nameof(UpdateCheckFailedMessage)] = "Suche nach Updates fehlgeschlagen",
        [nameof(DownloadsNotificationChannelName)] = "Downloads",
        [nameof(DownloadsNotificationText)] = "Aktive Downloads: {0}",
    };
}
