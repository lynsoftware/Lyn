using System.ComponentModel.DataAnnotations;
using Lyn.Backend.Models.Enums;

namespace Lyn.Backend.Models;

/// <summary>
/// Download file as BLOB for different versions and platforms
/// </summary>
public class AppDownload
{
    public int Id { get; set; }

    [Required(ErrorMessage = "FileName is required")]
    [MaxLength(100, ErrorMessage = "FileName can't be more than 100 characters")]
    public string FileName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Version is required")]
    [MaxLength(50, ErrorMessage = "Version can't be more than 50 characters")]
    public string Version { get; set; } = string.Empty;
    
    public DownloadPlatform Platform { get; set; }
    
    [Required(ErrorMessage = "FileData is required")]
    public byte[] FileData { get; set; } = [];
    
    // Path til fil/blob senere
    // [Required(ErrorMessage = "FilePath is required")]
    // [MaxLength(500, ErrorMessage = "FilePath can't be more than 500 characters")]
    // public string FilePath { get; set; } = string.Empty; 
    
    public long FileSizeBytes { get; set; }
    
    [Required(ErrorMessage = "ContentType is required")]
    [MaxLength(100, ErrorMessage = "ContentType can't be more than 100 characters")]
    public string ContentType { get; set; } = string.Empty;
    
    // Type fil
    [Required(ErrorMessage = "FileExtension is required")]
    [MaxLength(10, ErrorMessage = "FileExtension can't be more than 10 characters")]
    public string FileExtension { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Antall nedlastinger (for statistikk)
    public int DownloadCount { get; set; } = 0;
}