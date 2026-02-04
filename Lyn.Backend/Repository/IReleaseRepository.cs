using Lyn.Backend.Models;
using Lyn.Shared.Enum;

namespace Lyn.Backend.Repository;

public interface IReleaseRepository
{
    // ================================= GET =================================
    /// <summary>
    /// Sjekker om en AppRelease til en spesifikk versjon allerede eksisterer
    /// </summary>
    /// <param name="version">Version (1.0.1) etc</param>
    /// <param name="type">Release Type</param>
    /// <param name="ct"></param>
    /// <returns>Bool: True hvis den eksisterer eller false hvis ikke</returns>
    Task<bool> ExistsAsync(string version, ReleaseType type, CancellationToken ct = default);

    /// <summary>
    /// Henter en AppRelease med ID
    /// </summary>
    /// <param name="id">ID-en fra frontend</param>
    /// <param name="ct"></param>
    /// <returns>AppRelease eller null</returns>
    Task<AppRelease?> GetAsync(int id, CancellationToken ct = default); 
    
    /// <summary>
    /// Get the Ids of all the latest and active files
    /// </summary>
    /// <returns></returns>
    Task<List<AppRelease>> GetLatestAsync();
    
    
    // ================================= POST =================================
    
    /// <summary>
    /// Legger til og lagrer en AppRelease
    /// </summary>
    /// <param name="appRelease">Versjonen som blir lastet opp</param>
    /// <param name="ct">CancellationToken</param>
    Task CreateAsync(AppRelease appRelease, CancellationToken ct = default);
    
    // ================================= UPDATE =================================
    
    /// <summary>
    /// Øker en spesifikk AppRelease sin nedlastning med 1 etter en nedlastning
    /// </summary>
    /// <param name="id">AppRelease id</param>
    /// <param name="ct"></param>
    Task IncrementDownloadCountAsync(int id, CancellationToken ct = default);
    
    // ================================= SAVE =================================
    Task SaveChangesAsync();
}