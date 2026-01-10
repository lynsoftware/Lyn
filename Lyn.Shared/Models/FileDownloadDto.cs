namespace Lyn.Shared.Models;

/// <summary>
/// DTO for transfering a download file between layers
/// </summary>
public class FileDownloadDto
{
    public byte[] FileData { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}