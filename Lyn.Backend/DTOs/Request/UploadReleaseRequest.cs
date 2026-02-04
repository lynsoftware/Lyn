using System.ComponentModel.DataAnnotations;
using Lyn.Shared.Enum;

namespace Lyn.Backend.DTOs.Request;

/// <summary>
/// Versjon, Release Type, File og ReleaseNotes
/// </summary>
public class UploadReleaseRequest
{
    [Required(ErrorMessage = "Version is required")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Version must be between 1-20 characters")]
    public string Version { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Release Type is required")]
    [EnumDataType(typeof(ReleaseType), ErrorMessage = "Invalid Release Type")]
    public ReleaseType Type { get; set; }
    
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;
    
    [StringLength(5000, ErrorMessage = "Release notes cannot exceed 5000 characters")]
    public string ReleaseNotes { get; set; } = string.Empty;
}