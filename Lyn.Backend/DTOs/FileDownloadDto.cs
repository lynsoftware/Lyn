namespace Lyn.Backend.DTOs;

/// <summary>
/// DTO for transfering a download file between layers. Stream, ContentType og FileName
/// </summary>
public class FileDownloadDto
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}