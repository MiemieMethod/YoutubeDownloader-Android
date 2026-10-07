using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> UkrainianLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL або пошуковий запит",
            [nameof(QueryTooltip)] =
                "Приймається будь-який дійсний URL або ID YouTube. Додайте знак питання (?) для пошуку за текстом.",
            [nameof(ProcessQueryTooltip)] = "Виконати запит (Enter)",
            [nameof(AuthTooltip)] = "Автентифікація",
            [nameof(SettingsTooltip)] = "Налаштування",
            [nameof(DashboardPlaceholder)] = """
                Вставте **URL** або введіть **пошуковий запит** для завантаження
                Додайте кожен елемент з **нового рядка**, щоб додати декілька елементів
                """,
            [nameof(ContextMenuRemoveSuccessful)] = "Видалити успішні завантаження",
            [nameof(ContextMenuRemoveInactive)] = "Видалити неактивні завантаження",
            [nameof(ContextMenuRestartFailed)] = "Перезапустити невдалі завантаження",
            [nameof(ContextMenuCancelAll)] = "Скасувати всі завантаження",
            [nameof(DownloadStatusEnqueued)] = "В черзі...",
            [nameof(DownloadStatusCompleted)] = "Готово",
            [nameof(DownloadStatusCanceled)] = "Скасовано",
            [nameof(DownloadStatusFailed)] = "Помилка",
            [nameof(PlayTooltip)] = "Відтворити",
            [nameof(CancelDownloadTooltip)] = "Скасувати завантаження",
            [nameof(RestartDownloadTooltip)] = "Перезапустити завантаження",
            // Settings
            [nameof(SettingsTitle)] = "Налаштування",
            [nameof(ThemeLabel)] = "Тема",
            [nameof(ThemeTooltip)] = "Бажана тема інтерфейсу",
            [nameof(LanguageLabel)] = "Мова",
            [nameof(LanguageTooltip)] = "Бажана мова відображення інтерфейсу користувача",
            [nameof(AutoUpdateLabel)] = "Перевіряти оновлення",
            [nameof(AutoUpdateTooltip)] = """
                Перевіряти наявність нових версій під час кожного запуску.
                **Примітка:** рекомендується підтримувати програму в актуальному стані, щоб забезпечити її сумісність з останньою версією YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Зберігати автентифікацію",
            [nameof(PersistAuthTooltip)] = """
                Зберігати файли cookie у файлі для збереження між сеансами.
                **Увага**: хоча cookies зберігаються із шифруванням, зловмисник з доступом до вашої системи може їх відновити.
                """,
            [nameof(InjectAltLanguagesLabel)] = "Вставляти альтернативні мови",
            [nameof(InjectAltLanguagesTooltip)] =
                "Вставляти аудіодоріжки альтернативними мовами (якщо доступні) у завантажені файли",
            [nameof(InjectSubtitlesLabel)] = "Вставляти субтитри",
            [nameof(InjectSubtitlesTooltip)] =
                "Вставляти субтитри (якщо доступні) у завантажені файли",
            [nameof(InjectTagsLabel)] = "Вставляти медіатеги",
            [nameof(InjectTagsTooltip)] = "Вставляти медіатеги (якщо доступні) у завантажені файли",
            [nameof(SkipExistingFilesLabel)] = "Пропускати наявні файли",
            [nameof(SkipExistingFilesTooltip)] =
                "При завантаженні кількох відео пропускати ті, для яких вже є відповідні файли",
            [nameof(FileNameTemplateLabel)] = "Шаблон імені файлу",
            [nameof(FileNameTemplateTooltip)] = """
                Шаблон для генерації імен файлів завантажених відео.

                Доступні токени:
                **$num** — позиція відео у списку (якщо застосовно)
                **$id** — ідентифікатор відео
                **$title** — назва відео
                **$author** — автор відео
                """,
            [nameof(ParallelLimitLabel)] = "Ліміт паралелізації",
            [nameof(ParallelLimitTooltip)] = "Скільки завантажень може бути активними одночасно",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Автентифікація",
            [nameof(AuthenticatedText)] = "Ви автентифіковані",
            [nameof(LogOutButton)] = "Вийти",
            [nameof(AuthenticationPlaceholderText)] = """
                Завантаження...

                Якщо сторінка не відображається, переконайтеся, що Android System WebView встановлено та оновлено.
                """,
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Копіювати",
            [nameof(LiveLabel)] = "Живе",
            [nameof(AudioLabel)] = "Аудіо",
            [nameof(UpscaledLabel)] = "Збільшене",
            [nameof(FormatLabel)] = "Формат",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Контейнер",
            [nameof(VideoQualityLabel)] = "Якість відео",
            // Common buttons
            [nameof(CloseButton)] = "ЗАКРИТИ",
            [nameof(DownloadButton)] = "ЗАВАНТАЖИТИ",
            [nameof(CancelButton)] = "СКАСУВАТИ",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Дякуємо за підтримку України!",
            [nameof(UkraineSupportMessage)] = """
                Поки Росія веде геноцидну війну проти моєї країни, я вдячний кожному, хто продовжує підтримувати Україну у нашій боротьбі за свободу.

                Натисніть ДІЗНАТИСЬ БІЛЬШЕ, щоб знайти способи допомогти.
                """,
            [nameof(LearnMoreButton)] = "ДІЗНАТИСЬ БІЛЬШЕ",
            [nameof(FFmpegMissingTitle)] = "FFmpeg відсутній",
            [nameof(FFmpegMissingMessage)] = """
                Не вдалося знайти або запустити виконуваний файл FFmpeg, що постачається з {0}. Він потрібен для завантаження відео.

                Перевстановіть програму, щоб виправити цю проблему.
                """,
            [nameof(NothingFoundTitle)] = "Нічого не знайдено",
            [nameof(NothingFoundMessage)] = "Не вдалося знайти відео за вказаним запитом або URL",
            [nameof(ErrorTitle)] = "Помилка",
            // Android
            [nameof(DownloadDirectoryLabel)] = "Тека завантажень",
            [nameof(DownloadDirectoryTooltip)] = "Тека, в яку зберігаються завантажені файли",
            [nameof(DownloadDirectoryBrowseTooltip)] = "Вибрати теку",
            [nameof(DownloadDirectoryResetTooltip)] = "Скинути до типової теки",
            [nameof(FileNameLabel)] = "Назва файлу",
            [nameof(ShareTooltip)] = "Поділитися",
            [nameof(MoreOptionsTooltip)] = "Більше опцій",
            [nameof(SelectAllTooltip)] = "Вибрати все",
            [nameof(CopyButton)] = "КОПІЮВАТИ",
            [nameof(NoAppToOpenFileMessage)] = "Не знайдено програми для відкриття цього файлу",
            [nameof(SignInRequiredMessage)] = "YouTube вимагає входу в обліковий запис для доступу до цього відео (наприклад, щоб підтвердити, що ви не бот). Увійдіть у свій обліковий запис Google за допомогою кнопки автентифікації (значок людини з ключем) у верхній частині головного екрана та спробуйте ще раз.",
            [nameof(StoragePermissionDeniedMessage)] = "Для збереження завантажених файлів потрібен дозвіл на доступ до сховища",
            [nameof(UpdateAvailableMessage)] = "Доступна нова версія {0} v{1}",
            [nameof(UpdateCheckFailedMessage)] = "Не вдалося перевірити наявність оновлень",
            [nameof(DownloadsNotificationChannelName)] = "Завантаження",
            [nameof(DownloadsNotificationText)] = "Активні завантаження: {0}",
        };
}
