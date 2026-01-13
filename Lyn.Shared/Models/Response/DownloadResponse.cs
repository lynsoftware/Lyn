using Lyn.Backend.Models.Enums;

namespace Lyn.Shared.Models;

public class DownloadResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DownloadPlatform Platform { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}