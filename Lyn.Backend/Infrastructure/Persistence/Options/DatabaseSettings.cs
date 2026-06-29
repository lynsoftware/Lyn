using System.ComponentModel.DataAnnotations;

namespace Lyn.Backend.Infrastructure.Persistence.Options;

/// <summary>
/// Database-innstillinger fra appsettings (ConnectionStrings-seksjonen). Validert ved
/// oppstart slik at appen kraesjer umiddelbart hvis connection string mangler,
/// i stedet for forst ved forste DB-kall.
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "ConnectionStrings";

    [Required(ErrorMessage = "DefaultConnection is required")]
    public string DefaultConnection { get; set; } = string.Empty;
}