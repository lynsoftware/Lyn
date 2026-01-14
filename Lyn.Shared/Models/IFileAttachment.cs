namespace Lyn.Shared.Models;

public interface IFileAttachment
{
    int Id { get; set; }
    string FileName { get; set; }
    string OriginalFileName { get; set; }
    string ContentType { get; set; }
    string FileExtension { get; set; }
    long FileSize { get; set; }
    string FilePath { get; set; }
    DateTime UploadedAt { get; set; }
    string? FileHash { get; set; }
}