using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> SpanishLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL o consulta de búsqueda",
            [nameof(QueryTooltip)] =
                "Se acepta cualquier URL o ID de YouTube válido. Antepone un signo de interrogación (?) para buscar por texto.",
            [nameof(ProcessQueryTooltip)] = "Procesar consulta (Enter)",
            [nameof(AuthTooltip)] = "Autenticación",
            [nameof(SettingsTooltip)] = "Configuración",
            [nameof(DashboardPlaceholder)] = """
                Pega una **URL** o ingresa una **consulta de búsqueda** para comenzar
                Escribe cada elemento en una **nueva línea** para agregar múltiples elementos
                """,
            [nameof(ContextMenuRemoveSuccessful)] = "Eliminar descargas exitosas",
            [nameof(ContextMenuRemoveInactive)] = "Eliminar descargas inactivas",
            [nameof(ContextMenuRestartFailed)] = "Reiniciar descargas fallidas",
            [nameof(ContextMenuCancelAll)] = "Cancelar todas las descargas",
            [nameof(DownloadStatusEnqueued)] = "Pendiente...",
            [nameof(DownloadStatusCompleted)] = "Listo",
            [nameof(DownloadStatusCanceled)] = "Cancelado",
            [nameof(DownloadStatusFailed)] = "Fallido",
            [nameof(PlayTooltip)] = "Reproducir",
            [nameof(CancelDownloadTooltip)] = "Cancelar descarga",
            [nameof(RestartDownloadTooltip)] = "Reiniciar descarga",
            // Settings
            [nameof(SettingsTitle)] = "Configuración",
            [nameof(ThemeLabel)] = "Tema",
            [nameof(ThemeTooltip)] = "Tema de interfaz preferido",
            [nameof(LanguageLabel)] = "Idioma",
            [nameof(LanguageTooltip)] =
                "Idioma de visualización preferido para la interfaz de usuario",
            [nameof(AutoUpdateLabel)] = "Buscar actualizaciones",
            [nameof(AutoUpdateTooltip)] = """
                Buscar nuevas versiones en cada inicio.
                **Nota:** se recomienda mantener la aplicación actualizada para garantizar su compatibilidad con la última versión de YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Conservar autenticación",
            [nameof(PersistAuthTooltip)] = """
                Guardar las cookies de autenticación en un archivo para persistirlas entre sesiones.
                **Advertencia**: aunque las cookies se almacenan con cifrado, un atacante con acceso a su sistema podría recuperarlas.
                """,
            [nameof(InjectAltLanguagesLabel)] = "Insertar idiomas alternativos",
            [nameof(InjectAltLanguagesTooltip)] =
                "Insertar pistas de audio en idiomas alternativos (si están disponibles) en los archivos descargados",
            [nameof(InjectSubtitlesLabel)] = "Insertar subtítulos",
            [nameof(InjectSubtitlesTooltip)] =
                "Insertar subtítulos (si están disponibles) en los archivos descargados",
            [nameof(InjectTagsLabel)] = "Insertar etiquetas multimedia",
            [nameof(InjectTagsTooltip)] =
                "Insertar etiquetas multimedia (si están disponibles) en los archivos descargados",
            [nameof(SkipExistingFilesLabel)] = "Omitir archivos existentes",
            [nameof(SkipExistingFilesTooltip)] =
                "Al descargar múltiples videos, omitir los que ya tengan archivos correspondientes en el directorio de salida",
            [nameof(FileNameTemplateLabel)] = "Plantilla de nombre de archivo",
            [nameof(FileNameTemplateTooltip)] = """
                Plantilla para generar nombres de archivo de los videos descargados.

                Tokens disponibles:
                **$num** — posición del video en la lista (si aplica)
                **$id** — ID del video
                **$title** — título del video
                **$author** — autor del video
                """,
            [nameof(ParallelLimitLabel)] = "Límite paralelo",
            [nameof(ParallelLimitTooltip)] =
                "Cuántas descargas pueden estar activas al mismo tiempo",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Autenticación",
            [nameof(AuthenticatedText)] = "Actualmente estás autenticado",
            [nameof(LogOutButton)] = "Cerrar sesión",
            [nameof(AuthenticationPlaceholderText)] = """
                Cargando...

                Si la página no aparece, asegúrate de que Android System WebView esté instalado y actualizado.
                """,
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Copiar",
            [nameof(LiveLabel)] = "En vivo",
            [nameof(AudioLabel)] = "Audio",
            [nameof(UpscaledLabel)] = "Reescalado",
            [nameof(FormatLabel)] = "Formato",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Contenedor",
            [nameof(VideoQualityLabel)] = "Calidad de video",
            // Common buttons
            [nameof(CloseButton)] = "CERRAR",
            [nameof(DownloadButton)] = "DESCARGAR",
            [nameof(CancelButton)] = "CANCELAR",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "¡Gracias por apoyar a Ucrania!",
            [nameof(UkraineSupportMessage)] = """
                Mientras Rusia libra una guerra genocida contra mi país, estoy agradecido con todos los que continúan apoyando a Ucrania en nuestra lucha por la libertad.

                Haz clic en MÁS INFORMACIÓN para encontrar formas en que puedes ayudar.
                """,
            [nameof(LearnMoreButton)] = "MÁS INFORMACIÓN",
            [nameof(FFmpegMissingTitle)] = "Falta FFmpeg",
            [nameof(FFmpegMissingMessage)] = """
                No se pudo encontrar o iniciar el ejecutable de FFmpeg incluido con {0}. Es necesario para descargar videos.

                Reinstala la aplicación para solucionar este problema.
                """,
            [nameof(NothingFoundTitle)] = "Nada encontrado",
            [nameof(NothingFoundMessage)] =
                "No se encontraron videos basados en la consulta o URL proporcionada",
            [nameof(ErrorTitle)] = "Error",
            // Android
            [nameof(DownloadDirectoryLabel)] = "Carpeta de descargas",
            [nameof(DownloadDirectoryTooltip)] = "Carpeta donde se guardan los archivos descargados",
            [nameof(DownloadDirectoryBrowseTooltip)] = "Elegir carpeta",
            [nameof(DownloadDirectoryResetTooltip)] = "Restablecer la carpeta predeterminada",
            [nameof(FileNameLabel)] = "Nombre del archivo",
            [nameof(ShareTooltip)] = "Compartir",
            [nameof(MoreOptionsTooltip)] = "Más opciones",
            [nameof(SelectAllTooltip)] = "Seleccionar todo",
            [nameof(CopyButton)] = "COPIAR",
            [nameof(NoAppToOpenFileMessage)] = "No se encontró ninguna aplicación para abrir este archivo",
            [nameof(StoragePermissionDeniedMessage)] = "Se requiere permiso de almacenamiento para guardar los archivos descargados",
            [nameof(UpdateAvailableMessage)] = "{0} v{1} está disponible",
            [nameof(UpdateCheckFailedMessage)] = "Error al buscar actualizaciones",
            [nameof(DownloadsNotificationChannelName)] = "Descargas",
            [nameof(DownloadsNotificationText)] = "Descargas activas: {0}",
        };
}
