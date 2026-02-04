namespace Lyn.Web.DTOs;

public class FileDownloadDto
{
    public required byte[] FileData { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}