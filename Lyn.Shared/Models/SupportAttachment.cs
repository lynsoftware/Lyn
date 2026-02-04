using System.ComponentModel.DataAnnotations;


namespace Lyn.Shared.Models;

public class SupportAttachment 
{
    // =================== Primary key =================== 
    public int Id { get; set; }
    
    // =================== Foreign key=================== 
    public int SupportTicketId { get; set; }
    
    /// <summary>
    /// Unik identifikator for filen (brukes i StorageKey)
    /// </summary>
    public Guid FileId { get; set; }
    
    /// <summary>
    /// Filnavn lagret i S3 (f.eks. "guid.png")
    /// </summary>
    [Required(ErrorMessage = "Filename is required")]
    [MaxLength(100, ErrorMessage = "Filename cannot be more than 100 characters")]
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Originalt filnavn fra brukeren (f.eks. "screenshot.png")
    /// </summary>
    [Required(ErrorMessage = "Original filename is required")]
    [MaxLength(255, ErrorMessage = "Original filename cannot be more than 255 characters")]
    public string OriginalFileName { get; set; } = string.Empty; 
    
    /// <summary>
    /// MIME type (f.eks. "image/png")
    /// </summary>
    [Required(ErrorMessage = "ContentType is required")]
    [MaxLength(100, ErrorMessage = "Content type cannot be more than 100 characters")]
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Filtype med punktum (f.eks. ".png")
    /// </summary>
    [Required(ErrorMessage = "File extension is required")]
    [MaxLength(10, ErrorMessage = "File extension cannot be more than 10 characters")]
    public string FileExtension { get; set; } = string.Empty;
    
    /// <summary>
    /// Filstørrelse i bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Full S3 key (f.eks. "support-attachments/guid.png")
    /// </summary>
    [Required(ErrorMessage = "StorageKey is required")]
    [MaxLength(500, ErrorMessage = "StorageKey cannot be more than 500 characters")]
    public string StorageKey { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // =================== Navigation property =================== 
    public SupportTicket SupportTicket { get; set; } = null!;
}