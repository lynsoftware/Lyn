using Lyn.Shared.Configuration;

namespace Lyn.Shared.Helpers;

public static class FileHelper
{
    /// <summary>
    /// Gets the MIME content type based on file extension
    /// </summary>
    public static string GetContentTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".apk" => "application/vnd.android.package-archive",
            ".exe" => "application/vnd.microsoft.portable-executable",
            ".msi" => "application/x-msi",
            ".dmg" => "application/x-apple-diskimage",
            ".zip" => "application/zip",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
    
    /// <summary>
    /// Formats file size in bytes to B, KB, MB or GB
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
    
    /// <summary>
    /// Validates image extension with allowed extensions
    /// </summary>
    public static bool IsValidImageExtension(string filename, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;
        
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    /// <summary>
    /// Validates MIME type with allowed MIME types
    /// </summary>
    public static bool IsValidImageType(string contentType, string[] allowedMimeTypes)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;
        
        return allowedMimeTypes.Contains(contentType.ToLowerInvariant());
    }
    
}