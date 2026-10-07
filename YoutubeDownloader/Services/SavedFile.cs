namespace YoutubeDownloader.Services;

// File that has been saved to the shared storage.
// Either the content URI (MediaStore / Storage Access Framework) or the file path (legacy storage) is set.
public record SavedFile(string Name, string MimeType, string? ContentUri, string? FilePath);
