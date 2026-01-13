using System.ComponentModel.DataAnnotations;

namespace Lyn.Shared.Models;

public class SupportModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cant be longer than 100 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a category")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = string.Empty;
}