using Lyn.Shared.Result;

namespace Lyn.Backend.Services.Interface;

public interface IStorageService
{
    /// <summary>
    /// Laster opp et objekt til S3 Bucket som et PutObjectRequest. Validerer stream
    /// </summary>
    /// <param name="stream">Filen som en stream</param>
    /// <param name="storageKey">Filstien og navnet på filen som blir objektet i bøtta</param>
    /// <param name="contentType">Hvordan type fil det er</param>
    /// <param name="ct">Kanserlleringstoken hvis bruker avbryter</param>
    /// <returns>Result med Success eller Failure</returns>
    Task<Result> UploadAsync(Stream? stream, string storageKey, string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Henter et objekt fra en S3 Bucket
    /// </summary>
    /// <param name="storageKey">Filstien og navnet på filen som blir hentet fra bøtta</param>
    /// <param name="ct"></param>
    /// <returns>En Stream med filen</returns>
    Task<Result<Stream>> DownloadAsync(string storageKey, CancellationToken ct = default);
    
    
    /// <summary>
    /// Sletter en S3 fil fra S3 Bucket
    /// </summary>
    /// <param name="storageKey">Filstien og navnet på filen som blir slettet i bøtta</param>
    /// <param name="ct"></param>
    /// <returns>Result med Success eller Failure</returns>
    Task<Result> DeleteAsync(string storageKey, CancellationToken ct = default);
}