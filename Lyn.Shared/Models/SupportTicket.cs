using System.ComponentModel.DataAnnotations;
using Lyn.Shared.Enum;

namespace Lyn.Shared.Models;

public class SupportTicket 
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [MaxLength(255, ErrorMessage = "Email cannot be more than 255 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(100, ErrorMessage = "Title cannot be more than 100 characters")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Category is required")]
    [MaxLength(50, ErrorMessage = "Category cannot be more than 50 characters")]
    public string Category { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Description cannot be more than 2000 characters")]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.New;
    
    public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Normal;
    
    // For interne notater fra support team
    [MaxLength(5000, ErrorMessage = "Internal notes cannot be more than 5000 characters")]
    public string? InternalNotes { get; set; }
    
    // Svar til brukeren fra support team
    [MaxLength(5000, ErrorMessage = "Response cannot be more than 5000 characters")]
    public string? Response { get; set; }
    
    // Navigation properties
    public List<SupportAttachment> Attachments { get; set; } = new();
}