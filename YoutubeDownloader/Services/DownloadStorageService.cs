using StringBuilder = System.Text.StringBuilder;
using Encoding = System.Text.Encoding;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Webkit;
using AndroidUri = Android.Net.Uri;
using AndroidEnvironment = Android.OS.Environment;

namespace YoutubeDownloader.Services;

// Saves downloaded files to the shared storage, where they are accessible to the user and other apps:
// - user-selected folder (Storage Access Framework), if configured
// - "Download/YoutubeDownloader" via MediaStore on Android 10+
// - "Download/YoutubeDownloader" via direct file access on older versions
public class DownloadStorageService(SettingsService settingsService)
{
    public const string DefaultDirectoryName = "YoutubeDownloader";

    private static Context Context => Platform.AppContext;

    private static bool IsMediaStoreAvailable => OperatingSystem.IsAndroidVersionAtLeast(29);

    private static string DefaultRelativePath =>
        $"{AndroidEnvironment.DirectoryDownloads}/{DefaultDirectoryName}";

    public static string DefaultDirectoryDisplayName => DefaultRelativePath;

    public string DirectoryDisplayName =>
        settingsService.DownloadDirectoryUri is not null
            ? settingsService.DownloadDirectoryName ?? settingsService.DownloadDirectoryUri
            : DefaultDirectoryDisplayName;

    private AndroidUri? TryGetTreeUri()
    {
        if (string.IsNullOrWhiteSpace(settingsService.DownloadDirectoryUri))
            return null;

        return AndroidUri.Parse(settingsService.DownloadDirectoryUri);
    }

    public static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

        return extension switch
        {
            "mp4" => "video/mp4",
            "webm" => "video/webm",
            "mp3" => "audio/mpeg",
            "ogg" => "audio/ogg",
            "m4a" => "audio/mp4",
            _ => MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension)
                ?? "application/octet-stream",
        };
    }

    // Most file systems limit file names to 255 bytes
    public static string TruncateFileName(string fileName, int maxByteCount = 240)
    {
        if (Encoding.UTF8.GetByteCount(fileName) <= maxByteCount)
            return fileName;

        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var maxNameByteCount = maxByteCount - Encoding.UTF8.GetByteCount(extension);

        var buffer = new StringBuilder();
        var byteCount = 0;
        foreach (var rune in nameWithoutExtension.EnumerateRunes())
        {
            var runeByteCount = rune.Utf8SequenceLength;
            if (byteCount + runeByteCount > maxNameByteCount)
                break;

            buffer.Append(rune.ToString());
            byteCount += runeByteCount;
        }

        return buffer.ToString().Trim() + extension;
    }

    public bool IsStoragePermissionRequired =>
        !IsMediaStoreAvailable && settingsService.DownloadDirectoryUri is null;

    public async Task<bool> EnsureStoragePermissionAsync()
    {
        if (!IsStoragePermissionRequired)
            return true;

        var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.StorageWrite>();

        return status == PermissionStatus.Granted;
    }

    private static string GetLegacyDirectoryPath() =>
        Path.Combine(
#pragma warning disable CA1422 // Only used on Android versions before 10
            AndroidEnvironment
                .GetExternalStoragePublicDirectory(AndroidEnvironment.DirectoryDownloads)!
                .AbsolutePath,
#pragma warning restore CA1422
            DefaultDirectoryName
        );

    private static IEnumerable<string> QueryDisplayNames(
        AndroidUri uri,
        string column,
        string? selection = null,
        string[]? selectionArgs = null
    )
    {
        using var cursor = Context.ContentResolver?.Query(
            uri,
            [column],
            selection,
            selectionArgs,
            null
        );

        if (cursor is null)
            yield break;

        while (cursor.MoveToNext())
        {
            if (cursor.GetString(0) is { } name)
                yield return name;
        }
    }

    public IReadOnlySet<string> GetExistingFileNames()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            if (TryGetTreeUri() is { } treeUri)
            {
                var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(
                    treeUri,
                    DocumentsContract.GetTreeDocumentId(treeUri)
                )!;

                result.UnionWith(
                    QueryDisplayNames(childrenUri, DocumentsContract.Document.ColumnDisplayName)
                );
            }
            else if (IsMediaStoreAvailable)
            {
                result.UnionWith(
                    QueryDisplayNames(
                        MediaStore.Downloads.ExternalContentUri!,
                        MediaStore.IMediaColumns.DisplayName,
                        $"{MediaStore.IMediaColumns.RelativePath} = ?",
                        [DefaultRelativePath + "/"]
                    )
                );
            }
            else
            {
                var dirPath = GetLegacyDirectoryPath();
                if (Directory.Exists(dirPath))
                    result.UnionWith(Directory.EnumerateFiles(dirPath).Select(Path.GetFileName)!);
            }
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
            // Not being able to list existing files is not critical
        }

        return result;
    }

    private static string? TryQueryDisplayName(AndroidUri uri, string column)
    {
        try
        {
            return QueryDisplayNames(uri, column).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static async Task CopyToUriAsync(
        string sourceFilePath,
        AndroidUri destinationUri,
        CancellationToken cancellationToken
    )
    {
        await using var source = File.OpenRead(sourceFilePath);
        await using var destination =
            Context.ContentResolver?.OpenOutputStream(destinationUri, "w")
            ?? throw new IOException($"Could not open '{destinationUri}' for writing.");

        await source.CopyToAsync(destination, 81920, cancellationToken);
        await destination.FlushAsync(cancellationToken);
    }

    private static async Task<SavedFile> SaveToTreeAsync(
        AndroidUri treeUri,
        string sourceFilePath,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken
    )
    {
        var resolver = Context.ContentResolver!;

        var parentUri = DocumentsContract.BuildDocumentUriUsingTree(
            treeUri,
            DocumentsContract.GetTreeDocumentId(treeUri)
        )!;

        var documentUri =
            DocumentsContract.CreateDocument(resolver, parentUri, mimeType, fileName)
            ?? throw new IOException($"Could not create file '{fileName}' in the selected folder.");

        try
        {
            await CopyToUriAsync(sourceFilePath, documentUri, cancellationToken);
        }
        catch
        {
            try
            {
                DocumentsContract.DeleteDocument(resolver, documentUri);
            }
            catch
            {
                // Ignore
            }

            throw;
        }

        // The provider may have changed the name to avoid conflicts
        var actualName =
            TryQueryDisplayName(documentUri, DocumentsContract.Document.ColumnDisplayName)
            ?? fileName;

        return new SavedFile(actualName, mimeType, documentUri.ToString(), null);
    }

    private static async Task<SavedFile> SaveToMediaStoreAsync(
        string sourceFilePath,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken
    )
    {
        var resolver = Context.ContentResolver!;

        using var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(MediaStore.IMediaColumns.MimeType, mimeType);
        values.Put(MediaStore.IMediaColumns.RelativePath, DefaultRelativePath);
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        var itemUri =
            resolver.Insert(MediaStore.Downloads.ExternalContentUri!, values)
            ?? throw new IOException($"Could not create file '{fileName}' in the downloads folder.");

        try
        {
            await CopyToUriAsync(sourceFilePath, itemUri, cancellationToken);

            using var completedValues = new ContentValues();
            completedValues.Put(MediaStore.IMediaColumns.IsPending, 0);
            resolver.Update(itemUri, completedValues, null, null);
        }
        catch
        {
            try
            {
                resolver.Delete(itemUri, null, null);
            }
            catch
            {
                // Ignore
            }

            throw;
        }

        // MediaStore may have changed the name to avoid conflicts
        var actualName =
            TryQueryDisplayName(itemUri, MediaStore.IMediaColumns.DisplayName) ?? fileName;

        return new SavedFile(actualName, mimeType, itemUri.ToString(), null);
    }

    private static string EnsureUniqueFilePath(string baseFilePath)
    {
        if (!File.Exists(baseFilePath))
            return baseFilePath;

        var dirPath = Path.GetDirectoryName(baseFilePath) ?? string.Empty;
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
        var extension = Path.GetExtension(baseFilePath);

        for (var i = 1; i <= 1000; i++)
        {
            var filePath = Path.Combine(dirPath, $"{nameWithoutExtension} ({i}){extension}");
            if (!File.Exists(filePath))
                return filePath;
        }

        return baseFilePath;
    }

    private static async Task<SavedFile> SaveToLegacyStorageAsync(
        string sourceFilePath,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken
    )
    {
        var dirPath = GetLegacyDirectoryPath();
        Directory.CreateDirectory(dirPath);

        var filePath = EnsureUniqueFilePath(Path.Combine(dirPath, fileName));

        try
        {
            await using var source = File.OpenRead(sourceFilePath);
            await using var destination = File.Create(filePath);
            await source.CopyToAsync(destination, 81920, cancellationToken);
        }
        catch
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignore
            }

            throw;
        }

        // Make the file visible to other apps (e.g. gallery and music players)
        MediaScannerConnection.ScanFile(Context, [filePath], [mimeType], null);

        return new SavedFile(Path.GetFileName(filePath), mimeType, null, filePath);
    }

    public async Task<SavedFile> SaveAsync(
        string sourceFilePath,
        string fileName,
        CancellationToken cancellationToken = default
    )
    {
        fileName = TruncateFileName(fileName);
        var mimeType = GetMimeType(fileName);

        if (TryGetTreeUri() is { } treeUri)
        {
            return await SaveToTreeAsync(
                treeUri,
                sourceFilePath,
                fileName,
                mimeType,
                cancellationToken
            );
        }

        if (IsMediaStoreAvailable)
            return await SaveToMediaStoreAsync(sourceFilePath, fileName, mimeType, cancellationToken);

        return await SaveToLegacyStorageAsync(sourceFilePath, fileName, mimeType, cancellationToken);
    }

    public static async Task OpenAsync(SavedFile file)
    {
        if (file.ContentUri is not null)
        {
            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(AndroidUri.Parse(file.ContentUri), file.MimeType);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);

            Context.StartActivity(intent);
            return;
        }

        if (file.FilePath is not null)
        {
            await Launcher.Default.OpenAsync(
                new OpenFileRequest(file.Name, new ReadOnlyFile(file.FilePath, file.MimeType))
            );
        }
    }

    public static async Task ShareAsync(SavedFile file, string? title = null)
    {
        if (file.ContentUri is not null)
        {
            var intent = new Intent(Intent.ActionSend);
            intent.SetType(file.MimeType);
            intent.PutExtra(Intent.ExtraStream, AndroidUri.Parse(file.ContentUri));
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            var chooser = Intent.CreateChooser(intent, title)!;
            chooser.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);

            Context.StartActivity(chooser);
            return;
        }

        if (file.FilePath is not null)
        {
            await Share.Default.RequestAsync(
                new ShareFileRequest(title ?? file.Name, new ShareFile(file.FilePath, file.MimeType))
            );
        }
    }

    public static Task<AndroidUri?> PickDirectoryAsync() => DirectoryPicker.PickAsync();

    public void SetDirectory(AndroidUri? treeUri)
    {
        var resolver = Context.ContentResolver!;

        // Release the permission for the previously selected folder
        if (TryGetTreeUri() is { } previousTreeUri && !Equals(previousTreeUri, treeUri))
        {
            try
            {
                resolver.ReleasePersistableUriPermission(
                    previousTreeUri,
                    ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission
                );
            }
            catch
            {
                // Ignore
            }
        }

        if (treeUri is null)
        {
            settingsService.DownloadDirectoryUri = null;
            settingsService.DownloadDirectoryName = null;
            return;
        }

        // Keep access to the folder across app restarts
        resolver.TakePersistableUriPermission(
            treeUri,
            ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission
        );

        var documentId = DocumentsContract.GetTreeDocumentId(treeUri) ?? treeUri.ToString()!;

        // Document IDs of the external storage provider look like "primary:Download/Videos"
        var separatorIndex = documentId.IndexOf(':');
        var name =
            separatorIndex >= 0 && separatorIndex < documentId.Length - 1
                ? documentId[(separatorIndex + 1)..]
                : documentId;

        settingsService.DownloadDirectoryUri = treeUri.ToString();
        settingsService.DownloadDirectoryName = name;
    }
}
