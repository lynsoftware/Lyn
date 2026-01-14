namespace Lyn.Shared.Configuration;

public class FileSupportTicketUploadConstants
{
    // =========================== Image sizes ============================
    // 5MB each file
    public const long TicketMaxFileSizeBytes = 5 * 1024 * 1024; 
    // Max 5 files per request
    public const int TicketMaxFileCount = 5; 
    // 25MB total request
    public const long TicketMaxTotalRequestSize = 25 * 1024 * 1024; 
    
    
    // =========================== Allowed Images ============================
    // MIME types
    public static readonly string[] AllowedImageTypes = 
    {
        "image/jpeg",
        "image/jpg", 
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml"
    };
    
    // File extensions / file types
    public static readonly string[] AllowedImageExtensions = 
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".svg"
    };
    
}