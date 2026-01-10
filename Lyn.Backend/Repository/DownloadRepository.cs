

using Lyn.Backend.Data;
using Lyn.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Repository;

public class DownloadRepository(AppDbContext context) : IDownloadRepository
{
    // See interface for summary
    public async Task<AppDownload?> DownloadFileAsync(int id) => await context.AppDownloads
        .FirstOrDefaultAsync(ad => ad.Id == id && ad.IsActive);
    
    // See interface for summary
    public async Task<List<AppDownload>> GetLatestAsync() => await context.AppDownloads
        .Where(ad => ad.IsActive)
        .GroupBy(ad => ad.Platform)
        .Select(group => group.OrderByDescending(ad => ad.UploadedAt)
        .First())
        .ToListAsync();
    
    // See interface for summary
    public async Task AddAsync(AppDownload download) => await context.AddAsync(download);
    
    // See interface for summary
    public async Task<bool> AppDownloadsExistAsync() => await context.AppDownloads.AnyAsync();
    
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}