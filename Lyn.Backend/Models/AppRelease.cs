using System.ComponentModel.DataAnnotations;
using Lyn.Shared.Enum;

namespace Lyn.Backend.Models;

/// <summary>
/// Download file as BLOB for different versions and platforms
/// </summary>
public class AppRelease
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "FileName is required")]
    [MaxLength(100, ErrorMessage = "FileName can't be more than 100 characters")]
    public string FileName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Version is required")]
    [MaxLength(50, ErrorMessage = "Version can't be more than 50 characters")]
    public string Version { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Release Type is required")]
    public ReleaseType Type { get; set; }
    
    [Required(ErrorMessage = "FileGuidId is required")]
    [MaxLength(200, ErrorMessage = "FileGuidId can't be more than 200 characters")]
    public Guid FileGuidId { get; set; }
    
    [Required(ErrorMessage = "S3Key is required")]
    [MaxLength(500, ErrorMessage = "S3Key can't be more than 500 characters")]
    public string StorageKey { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    [Required(ErrorMessage = "ContentType is required")]
    [MaxLength(100, ErrorMessage = "ContentType can't be more than 100 characters")]
    public string ContentType { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ReleaseNotes is required")]
    [MaxLength(5000, ErrorMessage = "ReleaseNotes can't be more than 5000 characters")]
    public string ReleaseNotes { get; set; } = string.Empty;
    
    // Type fil
    [Required(ErrorMessage = "FileExtension is required")]
    [MaxLength(10, ErrorMessage = "FileExtension can't be more than 10 characters")]
    public string FileExtension { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Antall nedlastinger (for statistikk)
    public int DownloadCount { get; set; }
}