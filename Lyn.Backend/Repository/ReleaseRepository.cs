using Lyn.Backend.Data;
using Lyn.Backend.Models;
using Lyn.Shared.Enum;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Repository;

public class ReleaseRepository(AppDbContext context) : IReleaseRepository
{
    // ================================= GET =================================
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string version, ReleaseType type, CancellationToken ct = default)
        => await context.AppReleases.AnyAsync(ar => ar.Version == version && ar.Type == type, ct);
    
    /// <inheritdoc />
    public async Task<AppRelease?> GetAsync(int id, CancellationToken ct = default) => 
        await context.AppReleases.FirstOrDefaultAsync(ar => ar.Id == id, ct);
    
    /// <inheritdoc />
    public async Task<List<AppRelease>> GetLatestAsync() => await context.AppReleases
        .Where(ad => ad.IsActive)
        .GroupBy(ad => ad.Type)
        .Select(group => group.OrderByDescending(ad => ad.UploadedAt)
            .First())
        .ToListAsync();
    
    // ================================= POST =================================
    /// <inheritdoc />
    public async Task CreateAsync(AppRelease appRelease, CancellationToken ct = default)
    {
        await context.AppReleases.AddAsync(appRelease, ct);
        await context.SaveChangesAsync(ct);
    }
    
    
    // ================================= UPDATE =================================
    
    public async Task IncrementDownloadCountAsync(int id, CancellationToken ct = default) =>
        await context.AppReleases
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => 
                s.SetProperty(r => r.DownloadCount, r => r.DownloadCount + 1), ct);
    
    
    // ================================= SAVE =================================
    
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}