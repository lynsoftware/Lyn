using Amazon.S3;
using Amazon.S3.Model;
using Lyn.Backend.Services.Interface;
using Lyn.Shared.Enum;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public class S3StorageService(
    IAmazonS3 s3Client,
    IConfiguration configuration,
    ILogger<S3StorageService> logger) : IStorageService
{
    private readonly string _bucketName = configuration["AWS:BucketName"] 
                                          ?? throw new InvalidOperationException("AWS:BucketName not configured");
    
    /// <inheritdoc />
    public async Task<Result> UploadAsync(Stream? stream, string storageKey, string contentType,
        CancellationToken ct = default)
    {
        // Validerer stream først
        if (stream is null || !stream.CanRead)
        {
            logger.LogError("Invalid stream provided for upload: {Key}", storageKey);
            return Result.Failure("Invalid file stream");
        }
        
        // Network streams så fungerrer ikke alltid Length, derfor sjekker vi med CanSeek også
        if (stream.CanSeek && stream.Length == 0)
        {
            logger.LogError("Empty stream provided for upload: {Key}", storageKey);
            return Result.Failure("File is empty");
        }
        
        try
        {
            // Oppretter et PutObjectRequest for å putte i bøtta
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = storageKey,
                InputStream = stream,
                ContentType = contentType
            };
            
            // Putter objektet i bøtta
            await s3Client.PutObjectAsync(putRequest, ct);
            
            logger.LogInformation("Successfully uploaded file to S3: {Key}", storageKey);
            
            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "S3 error uploading file: {Key}. Error: {ErrorCode}", 
                storageKey, ex.ErrorCode);
            return Result.Failure($"Failed to upload file: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error uploading file to S3: {Key}", storageKey);
            return Result.Failure("An unexpected error occurred while uploading the file");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<Stream>> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = storageKey
            };
            
            var response = await s3Client.GetObjectAsync(request, ct);
            
            // Sjekker at filen ikke er tom
            if (response.ContentLength == 0)
            {
                logger.LogWarning("Empty file downloaded from S3: {Key}", storageKey);
                return Result<Stream>.Failure("File is empty");
            }
            
            logger.LogInformation("Successfully downloaded file from S3: {Key}", storageKey);
            
            return Result<Stream>.Success(response.ResponseStream);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("File not found in S3: {Key}", storageKey);
            
            return Result<Stream>.Failure("File not found", ErrorTypeEnum.NotFound);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "S3 error downloading file: {Key}. Error: {ErrorCode}", 
                storageKey, ex.ErrorCode);
            
            return Result<Stream>.Failure($"Failed to download file: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error downloading file from S3: {Key}", storageKey);
            
            return Result<Stream>.Failure("An unexpected error occurred while downloading the file");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        try
        {
            await s3Client.DeleteObjectAsync(_bucketName, storageKey, ct);
            
            logger.LogInformation("Successfully deleted file from S3: {Key}", storageKey);
            
            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "S3 error deleting file: {Key}. Error: {ErrorCode}", 
                storageKey, ex.ErrorCode);
            return Result.Failure($"Failed to delete file: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting file from S3: {Key}", storageKey);
            return Result.Failure("An unexpected error occurred while deleting the file");
        }
    }

    public async Task<Result<bool>> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(_bucketName, key, ct);
            
            return Result<bool>.Success(true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<bool>.Success(false);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "S3 error checking file existence: {Key}. Error: {ErrorCode}", 
                key, ex.ErrorCode);
            
            return Result<bool>.Failure($"Failed to check file existence: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error checking file existence in S3: {Key}", key);
            
            return Result<bool>.Failure("An unexpected error occurred while checking file existence");
        }
    }
}