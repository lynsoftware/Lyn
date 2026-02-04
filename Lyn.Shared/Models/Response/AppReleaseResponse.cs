using Lyn.Shared.Enum;

namespace Lyn.Shared.Models.Response;

public class AppReleaseResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public ReleaseType Type { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}