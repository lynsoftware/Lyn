namespace Lyn.Backend.Models;

/// <summary>
/// Modell for statistics related to PasswordGenerator
/// </summary>
public class PasswordGeneratorUsageStatistic
{
    public int Id { get; set; }
    public int PasswordsGenerated { get; set; }
    public int WindowsDownloads { get; set; }
    public int ApkDownloads { get; set; }
}