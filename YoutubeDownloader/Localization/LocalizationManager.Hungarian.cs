using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> HungarianLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL vagy keresés",
            [nameof(QueryTooltip)] =
                "Bármilyen érvényes YouTube URL vagy videóazonosító megadható. Szöveges kereséshez írj egy kérdőjelet (?) a keresőkifejezés elé.",
            [nameof(ProcessQueryTooltip)] = "Keresés indítása (Enter)",
            [nameof(AuthTooltip)] = "Bejelentkezés",
            [nameof(SettingsTooltip)] = "Beállítások",
            [nameof(DashboardPlaceholder)] = """
                Másold be egy videó **URL**-jét vagy írj be egy **kifejezést** a kereséshez
                Több elem hozzáadásához írj minden elemet **új sorba**
                """,
            [nameof(ContextMenuRemoveSuccessful)] = "Sikeres letöltések eltávolítása",
            [nameof(ContextMenuRemoveInactive)] = "Inaktív letöltések eltávolítása",
            [nameof(ContextMenuRestartFailed)] = "Sikertelen letöltések újraindítása",
            [nameof(ContextMenuCancelAll)] = "Összes letöltés megszakítása",
            [nameof(DownloadStatusEnqueued)] = "Függőben...",
            [nameof(DownloadStatusCompleted)] = "Kész",
            [nameof(DownloadStatusCanceled)] = "Megszakítva",
            [nameof(DownloadStatusFailed)] = "Sikertelen",
            [nameof(PlayTooltip)] = "Lejátszás",
            [nameof(CancelDownloadTooltip)] = "Letöltés megszakítása",
            [nameof(RestartDownloadTooltip)] = "Letöltés újraindítása",
            // Settings
            [nameof(SettingsTitle)] = "Beállítások",
            [nameof(ThemeLabel)] = "Téma",
            [nameof(ThemeTooltip)] = "Felhasználói felület témája",
            [nameof(LanguageLabel)] = "Nyelv",
            [nameof(LanguageTooltip)] = "Felhasználói felület nyelve",
            [nameof(AutoUpdateLabel)] = "Frissítések keresése",
            [nameof(AutoUpdateTooltip)] = """
                Új verziók keresése minden indításkor.
                **Megjegyzés:** ajánlott naprakészen tartani az alkalmazást, hogy kompatibilis maradjon a YouTube legújabb verziójával.
                """,
            [nameof(PersistAuthLabel)] = "Bejelentkezve maradok",
            [nameof(PersistAuthTooltip)] = """
                Sütik elmentése fájlba, hogy a későbbi munkamenetek során is belépve tudj maradni.
                **Figyelem**: bár a sütik titkosítva tárolódnak, hozzáértő támadók a rendszeredhez hozzáférve vissza tudják fejteni.
                """,
            [nameof(InjectAltLanguagesLabel)] = "Alternatív nyelvek beszúrása",
            [nameof(InjectAltLanguagesTooltip)] =
                "Más nyelvű audiosávok beszúrása (amennyiben léteznek) a letöltött fájlba",
            [nameof(InjectSubtitlesLabel)] = "Feliratok beszúrása",
            [nameof(InjectSubtitlesTooltip)] = "Feliratok (ha vannak) beszúrása a letöltött fájlba",
            [nameof(InjectTagsLabel)] = "Médiacímkék beszúrása",
            [nameof(InjectTagsTooltip)] = "Médiacímkék (ha vannak) beszúrása a letöltött fájlokba",
            [nameof(SkipExistingFilesLabel)] = "Létező fájlok kihagyása",
            [nameof(SkipExistingFilesTooltip)] =
                "Több videó letöltése esetén azok kihagyása, amik már léteznek a célmappában",
            [nameof(FileNameTemplateLabel)] = "Fájlnévminta",
            [nameof(FileNameTemplateTooltip)] = """
                A letöltött videók fájlnevének létrehozásához használt minta.

                Elérhető változók:
                **$num** — videó pozíciója/sorszáma a listában (ha van)
                **$id** — videó azonosítója
                **$title** — videó címe
                **$author** — videó szerzője
                """,
            [nameof(ParallelLimitLabel)] = "Egyidejű letöltések",
            [nameof(ParallelLimitTooltip)] = "Hány letöltés futhat egyidejűleg",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Bejelentkezés",
            [nameof(AuthenticatedText)] = "Be vagy jelentkezve",
            [nameof(LogOutButton)] = "Kijelentkezés",
            [nameof(AuthenticationPlaceholderText)] = """
                Betöltés...

                Ha az oldal nem jelenik meg, győződj meg róla, hogy az Android System WebView telepítve van és naprakész.
                """,
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Másolás",
            [nameof(LiveLabel)] = "Élő",
            [nameof(AudioLabel)] = "Audió",
            [nameof(UpscaledLabel)] = "Felskálázott",
            [nameof(FormatLabel)] = "Formátum",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Konténer",
            [nameof(VideoQualityLabel)] = "Videó minőség",
            // Common buttons
            [nameof(CloseButton)] = "BEZÁRÁS",
            [nameof(DownloadButton)] = "LETÖLTÉS",
            [nameof(CancelButton)] = "MÉGSE",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Köszönet Ukrajna támogatásáért!",
            [nameof(UkraineSupportMessage)] = """
                Mialatt Oroszország népirtó háborút vív hazám ellen, hálás vagyok mindenkinek, aki továbbra is Ukrajna mellett áll a szabadságért folytatott harcunkban.

                A TUDJ MEG TÖBBET gombra kattintva megtudhatod, hogyan segíthetsz.
                """,
            [nameof(LearnMoreButton)] = "Tudj meg többet",
            [nameof(FFmpegMissingTitle)] = "Az FFmpeg hiányzik",
            [nameof(FFmpegMissingMessage)] = """
                A(z) {0} alkalmazással együtt szállított FFmpeg nem található vagy nem indítható el. Szükség van rá a videók letöltéséhez.

                A probléma megoldásához telepítsd újra az alkalmazást.
                """,
            [nameof(NothingFoundTitle)] = "Nem található",
            [nameof(NothingFoundMessage)] =
                "Nem találhatók videók a megadott keresés vagy URL alapján.",
            [nameof(ErrorTitle)] = "Hiba",
            // Android
            [nameof(DownloadDirectoryLabel)] = "Letöltési mappa",
            [nameof(DownloadDirectoryTooltip)] = "A letöltött fájlok mentési mappája",
            [nameof(DownloadDirectoryBrowseTooltip)] = "Mappa kiválasztása",
            [nameof(DownloadDirectoryResetTooltip)] = "Visszaállítás az alapértelmezett mappára",
            [nameof(FileNameLabel)] = "Fájlnév",
            [nameof(ShareTooltip)] = "Megosztás",
            [nameof(MoreOptionsTooltip)] = "További lehetőségek",
            [nameof(SelectAllTooltip)] = "Összes kijelölése",
            [nameof(CopyButton)] = "MÁSOLÁS",
            [nameof(NoAppToOpenFileMessage)] = "Nem található alkalmazás a fájl megnyitásához",
            [nameof(SignInRequiredMessage)] = "A YouTube bejelentkezést kér a videó eléréséhez (például annak megerősítéséhez, hogy nem vagy robot). Jelentkezz be Google-fiókoddal a főképernyő tetején található hitelesítés gombbal (kulcsos személy ikon), majd próbáld újra.",
            [nameof(StoragePermissionDeniedMessage)] = "A letöltött fájlok mentéséhez tárhely-hozzáférési engedély szükséges",
            [nameof(UpdateAvailableMessage)] = "Elérhető a(z) {0} v{1}",
            [nameof(UpdateCheckFailedMessage)] = "Nem sikerült frissítéseket keresni",
            [nameof(DownloadsNotificationChannelName)] = "Letöltések",
            [nameof(DownloadsNotificationText)] = "Aktív letöltések: {0}",
        };
}
