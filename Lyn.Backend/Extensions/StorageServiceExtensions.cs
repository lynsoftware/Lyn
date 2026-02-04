using Lyn.Backend.Services.Interface;
using Lyn.Shared.Result;

namespace Lyn.Backend.Extensions;

public static class StorageServiceExtensions
{
    /// <summary>
    /// Laster opp en IFormFile til storage. Håndterer stream åpning og lukking automatisk.
    /// </summary>
    public static async Task<Result> UploadAsync(
        this IStorageService storageService,
        IFormFile file,
        string storageKey,
        CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream();
        
        return await storageService.UploadAsync(
            stream,
            storageKey,
            file.ContentType,
            ct);
    }
}