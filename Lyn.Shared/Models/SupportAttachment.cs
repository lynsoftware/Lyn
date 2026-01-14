using System.ComponentModel.DataAnnotations;


namespace Lyn.Shared.Models;

public class SupportAttachment : IFileAttachment
{
    public int Id { get; set; }
    
    public int SupportTicketId { get; set; }
    
    // Det vi lagrer det som i Blob/Storage
    [Required(ErrorMessage = "Filename is required")]
    [MaxLength(255, ErrorMessage = "Filename cannot be more than 255 characters")]
    public string FileName { get; set; } = string.Empty;
    
    // Det brukeren lastet opp
    [Required(ErrorMessage = "Original filename is required")]
    [MaxLength(255, ErrorMessage = "Original filename cannot be more than 255 characters")]
    public string OriginalFileName { get; set; } = string.Empty; 
    
    [Required(ErrorMessage = "ContentType is required")]
    [MaxLength(100, ErrorMessage = "Content type cannot be more than 100 characters")]
    public string ContentType { get; set; } = string.Empty;
    
    // Filtype
    [Required(ErrorMessage = "File extension is required")]
    [MaxLength(10, ErrorMessage = "File extension cannot be more than 10 characters")]
    public string FileExtension { get; set; } = string.Empty; 
    
    public long FileSize { get; set; }
    
    [Required(ErrorMessage = "FileData is required")]
    public byte[] FileData { get; set; } = [];
    
    // Path eller BlobUrl - for snere
    [Required(ErrorMessage = "FilePath is required")]
    [MaxLength(500, ErrorMessage = "FilePath cannot be more than 500 characters")]
    public string FilePath { get; set; } = string.Empty; 
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    

    
    // ==================== Metadata kun for bilder ====================
    public int? Width { get; set; }
    public int? Height { get; set; }
    
    // For SEO
    [MaxLength(500, ErrorMessage = "Alt text cannot be more than 500 characters")]
    public string? AltText { get; set; }
    
    // Sikrer at det ikke blir duplikater da like filer alltid vil få samme hash
    [MaxLength(64, ErrorMessage = "File hash cannot be more than 64 characters")]
    public string? FileHash { get; set; } // SHA256 hash
    
    // Navigation property
    public SupportTicket SupportTicket { get; set; } = null!;
}