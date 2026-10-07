using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> FrenchLocalization = new Dictionary<
        string,
        string
    >
    {
        // Dashboard
        [nameof(QueryPlaceholderText)] = "URL ou requête de recherche",
        [nameof(QueryTooltip)] =
            "Toute URL ou ID YouTube valide est acceptée. Ajoutez un point d'interrogation (?) pour rechercher par texte.",
        [nameof(ProcessQueryTooltip)] = "Traiter la requête (Entrée)",
        [nameof(AuthTooltip)] = "Authentification",
        [nameof(SettingsTooltip)] = "Paramètres",
        [nameof(DashboardPlaceholder)] = """
            Collez une **URL** ou entrez une **requête de recherche** pour commencer
            Placez chaque élément sur une **nouvelle ligne** pour ajouter plusieurs éléments
            """,
        [nameof(ContextMenuRemoveSuccessful)] = "Supprimer les téléchargements réussis",
        [nameof(ContextMenuRemoveInactive)] = "Supprimer les téléchargements inactifs",
        [nameof(ContextMenuRestartFailed)] = "Relancer les téléchargements échoués",
        [nameof(ContextMenuCancelAll)] = "Annuler tous les téléchargements",
        [nameof(DownloadStatusEnqueued)] = "En attente...",
        [nameof(DownloadStatusCompleted)] = "Terminé",
        [nameof(DownloadStatusCanceled)] = "Annulé",
        [nameof(DownloadStatusFailed)] = "Échec",
        [nameof(PlayTooltip)] = "Lire",
        [nameof(CancelDownloadTooltip)] = "Annuler le téléchargement",
        [nameof(RestartDownloadTooltip)] = "Relancer le téléchargement",
        // Settings
        [nameof(SettingsTitle)] = "Paramètres",
        [nameof(ThemeLabel)] = "Thème",
        [nameof(ThemeTooltip)] = "Thème d'interface préféré",
        [nameof(LanguageLabel)] = "Langue",
        [nameof(LanguageTooltip)] = "Langue d'affichage préférée pour l'interface utilisateur",
        [nameof(AutoUpdateLabel)] = "Rechercher des mises à jour",
        [nameof(AutoUpdateTooltip)] = """
            Rechercher de nouvelles versions à chaque lancement.
            **Remarque :** il est recommandé de garder l'application à jour pour assurer sa compatibilité avec la dernière version de YouTube.
            """,
        [nameof(PersistAuthLabel)] = "Conserver l'authentification",
        [nameof(PersistAuthTooltip)] = """
            Enregistrer les cookies d'authentification dans un fichier pour les conserver entre les sessions.
            **Avertissement** : bien que les cookies soient stockés avec chiffrement, ils peuvent toujours être récupérés par un attaquant ayant accès à votre système.
            """,
        [nameof(InjectAltLanguagesLabel)] = "Injecter les langues alternatives",
        [nameof(InjectAltLanguagesTooltip)] =
            "Injecter des pistes audio en langues alternatives (si disponibles) dans les fichiers téléchargés",
        [nameof(InjectSubtitlesLabel)] = "Injecter les sous-titres",
        [nameof(InjectSubtitlesTooltip)] =
            "Injecter les sous-titres (si disponibles) dans les fichiers téléchargés",
        [nameof(InjectTagsLabel)] = "Injecter les balises média",
        [nameof(InjectTagsTooltip)] =
            "Injecter les balises média (si disponibles) dans les fichiers téléchargés",
        [nameof(SkipExistingFilesLabel)] = "Ignorer les fichiers existants",
        [nameof(SkipExistingFilesTooltip)] =
            "Lors du téléchargement de plusieurs vidéos, ignorer celles qui ont déjà des fichiers correspondants dans le répertoire de sortie",
        [nameof(FileNameTemplateLabel)] = "Modèle de nom de fichier",
        [nameof(FileNameTemplateTooltip)] = """
            Modèle utilisé pour générer les noms de fichiers des vidéos téléchargées.

            Jetons disponibles :
            **$num** — position de la vidéo dans la liste (si applicable)
            **$id** — ID de la vidéo
            **$title** — titre de la vidéo
            **$author** — auteur de la vidéo
            """,
        [nameof(ParallelLimitLabel)] = "Limite parallèle",
        [nameof(ParallelLimitTooltip)] =
            "Combien de téléchargements peuvent être actifs en même temps",
        // Auth Setup
        [nameof(AuthenticationTitle)] = "Authentification",
        [nameof(AuthenticatedText)] = "Vous êtes actuellement authentifié",
        [nameof(LogOutButton)] = "Se déconnecter",
        [nameof(AuthenticationPlaceholderText)] = """
            Chargement...

            Si la page ne s'affiche pas, assurez-vous qu'Android System WebView est installé et à jour.
            """,
        // Download Single Setup
        [nameof(CopyMenuItem)] = "Copier",
        [nameof(LiveLabel)] = "En direct",
        [nameof(AudioLabel)] = "Audio",
        [nameof(UpscaledLabel)] = "Suréchantillonné",
        [nameof(FormatLabel)] = "Format",
        // Download Multiple Setup
        [nameof(ContainerLabel)] = "Conteneur",
        [nameof(VideoQualityLabel)] = "Qualité vidéo",
        // Common buttons
        [nameof(CloseButton)] = "FERMER",
        [nameof(DownloadButton)] = "TÉLÉCHARGER",
        [nameof(CancelButton)] = "ANNULER",
        // Dialog messages
        [nameof(WelcomeTitle)] = "Bienvenue dans {0} !",
        [nameof(WelcomeMessage)] = """
            Cette application est gratuite et open source. Si elle vous est utile, soutenez-la en ajoutant une étoile à son dépôt sur GitHub et en la faisant connaître autour de vous.

            Les rapports de bugs, les suggestions et les contributions au code sont les bienvenus. Améliorons cette application ensemble !

            {0}
            """,
        [nameof(OpenProjectButton)] = "OUVRIR GITHUB",
        [nameof(FFmpegMissingTitle)] = "FFmpeg est manquant",
        [nameof(FFmpegMissingMessage)] = """
            L'exécutable FFmpeg fourni avec {0} est introuvable ou n'a pas pu être lancé. Il est requis pour télécharger des vidéos.

            Veuillez réinstaller l'application pour résoudre ce problème.
            """,
        [nameof(NothingFoundTitle)] = "Rien trouvé",
        [nameof(NothingFoundMessage)] =
            "Impossible de trouver des vidéos correspondant à la requête ou l'URL fournie",
        [nameof(ErrorTitle)] = "Erreur",
        // Android
        [nameof(DownloadDirectoryLabel)] = "Dossier de téléchargement",
        [nameof(DownloadDirectoryTooltip)] = "Dossier dans lequel les fichiers téléchargés sont enregistrés",
        [nameof(DownloadDirectoryBrowseTooltip)] = "Choisir un dossier",
        [nameof(DownloadDirectoryResetTooltip)] = "Rétablir le dossier par défaut",
        [nameof(FileNameLabel)] = "Nom du fichier",
        [nameof(ShareTooltip)] = "Partager",
        [nameof(MoreOptionsTooltip)] = "Plus d'options",
        [nameof(SelectAllTooltip)] = "Tout sélectionner",
        [nameof(CopyButton)] = "COPIER",
        [nameof(NoAppToOpenFileMessage)] = "Aucune application trouvée pour ouvrir ce fichier",
        [nameof(SignInRequiredMessage)] = "YouTube exige une connexion pour accéder à cette vidéo (par exemple pour confirmer que vous n'êtes pas un robot). Connectez-vous à votre compte Google à l'aide du bouton d'authentification (icône de personne avec une clé) en haut de l'écran principal, puis réessayez.",
        [nameof(StoragePermissionDeniedMessage)] = "L'autorisation d'accès au stockage est requise pour enregistrer les fichiers téléchargés",
        [nameof(UpdateAvailableMessage)] = "{0} v{1} est disponible",
        [nameof(UpdateCheckFailedMessage)] = "Échec de la recherche de mises à jour",
        [nameof(DownloadsNotificationChannelName)] = "Téléchargements",
        [nameof(DownloadsNotificationText)] = "Téléchargements actifs : {0}",
    };
}
