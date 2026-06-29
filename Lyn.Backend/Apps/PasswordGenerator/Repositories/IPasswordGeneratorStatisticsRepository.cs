namespace Lyn.Backend.Apps.PasswordGenerator.Repositories;

public interface IPasswordGeneratorStatisticsRepository
{
    /// <summary>
    /// Increments Passwords generated
    /// </summary>
    Task IncrementPasswordGeneratedAsync();
}