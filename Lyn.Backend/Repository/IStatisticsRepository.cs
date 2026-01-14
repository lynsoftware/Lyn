namespace Lyn.Backend.Repository;

public interface IStatisticsRepository
{
    /// <summary>
    /// Increments Passwords generated
    /// </summary>
    Task IncrementPasswordGeneratedAsync();
}