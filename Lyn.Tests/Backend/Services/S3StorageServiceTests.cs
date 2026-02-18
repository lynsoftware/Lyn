using Amazon.S3;
using Amazon.S3.Model;
using Lyn.Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyn.Tests.Backend.Services;

public class S3StorageServiceTests
{
    private readonly S3StorageService _sut;
    private readonly Mock<IAmazonS3> _mockS3Client;

    public S3StorageServiceTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        var mockLogger = new Mock<ILogger<S3StorageService>>();
        
        // Mocker IConfiguratiuon for å returnere bøttenavnet
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration
            .Setup(c => c["AWS:BucketName"])
            .Returns("test-bucket");

        _sut = new S3StorageService(_mockS3Client.Object, mockConfiguration.Object, mockLogger.Object);
    }
    
    // ==================== UploadAsync ====================
    
    /// <summary>
    /// Tester UploadAsync med en fil som blir lastet opp vellykket til S3.
    /// Oppretter en falsk fil med MemoryStream og kaller UploadAsync med S3-klient som returnerer suksess
    /// </summary>
    [Fact]
    public async Task UploadAsync_ValidStream_ReturnsSuccess()
    {
        // Arrange
        var stream = new MemoryStream([1,2,3]);
        
        // S3Clienten returnerer en PutObjectResponse og ikke en feilmelding
        _mockS3Client.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());
        
        // Act
        var result = await _sut.UploadAsync(stream, "test/file.png", "image/png");
        
        // Assert
        Assert.True(result.IsSuccess);
    }
    
    /// <summary>
    /// Tester UploadAsync med ingen stream, altså ingen fil
    /// </summary>
     [Fact]
    public async Task UploadAsync_NullStream_ReturnsFailure()
    {
        // Act
        var result = await _sut.UploadAsync(null, "test/file.png", "image/png");

        // Assert
        Assert.True(result.IsFailure);
    }
    
    /// <summary>
    /// Tester UploadAsync med en tom fil/tom stream.
    /// </summary>
    [Fact]
    public async Task UploadAsync_EmptyStream_ReturnsFailure()
    {
        // Arrange
        var stream = new MemoryStream(); // Tom stream, 0 bytes

        // Act
        var result = await _sut.UploadAsync(stream, "test/file.png", "image/png");

        // Assert
        Assert.True(result.IsFailure);
    }
    
    /// <summary>
    /// Tester UploadAsync hvor S3-klienten returner en feil feil
    /// </summary>
    [Fact]
    public async Task UploadAsync_S3Exception_ReturnsFailure()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        _mockS3Client
            .Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("S3 error"));

        // Act
        var result = await _sut.UploadAsync(stream, "test/file.png", "image/png");

        // Assert
        Assert.True(result.IsFailure);
    }

    // ==================== DownloadAsync ====================
    
    /// <summary>
    /// Tester DownloadAsync med riktig nøkkel til en fil.
    /// Mocker at S3-klienten returnerer en vellykktet GetObjectRequest.
    /// </summary>
    [Fact]
    public async Task DownloadAsync_ValidKey_ReturnsStream()
    {
        // Arrange
        var responseStream = new MemoryStream([1, 2, 3]);
        var response = new GetObjectResponse
        {
            ResponseStream = responseStream,
            ContentLength = 3
        };
        _mockS3Client
            .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.DownloadAsync("test/file.png");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value); // Sjekker at Result-objektet har en verdi
    }
    
    /// <summary>
    /// Tester DownloadAsync hvor S3-klienten returneren en tom fil/korrupt fil
    /// </summary>
    [Fact]
    public async Task DownloadAsync_EmptyFile_ReturnsFailure()
    {
        // Arrange
        var response = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(),
            ContentLength = 0
        };
        _mockS3Client
            .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.DownloadAsync("test/file.png");

        // Assert
        Assert.True(result.IsFailure);
    }
    
    /// <summary>
    /// Tester DownlaodAsync med en nøkkel som ikke tilhører en eksisterende fil.
    /// S3-klienten returnerer da Not Found
    /// </summary>
    [Fact]
    public async Task DownloadAsync_FileNotFound_ReturnsFailure()
    {
        // Arrange
        _mockS3Client
            .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Not Found") { StatusCode = System.Net.HttpStatusCode.NotFound });

        // Act
        var result = await _sut.DownloadAsync("nonexistent/file.png");

        // Assert
        Assert.True(result.IsFailure);
    }

    // ==================== DeleteAsync ====================
    
    /// <summary>
    /// Tester sletting av en fil som eksisterer
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidKey_ReturnsSuccess()
    {
        // Arrange
        _mockS3Client
            .Setup(s => s.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse());

        // Act
        var result = await _sut.DeleteAsync("test/file.png");

        // Assert
        Assert.True(result.IsSuccess);
    }
    
    /// <summary>
    /// Tester sletting av en fil som ikke eksisterer
    /// </summary>
    [Fact]
    public async Task DeleteAsync_S3Exception_ReturnsFailure()
    {
        // Arrange
        _mockS3Client
            .Setup(s => s.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("S3 error"));

        // Act
        var result = await _sut.DeleteAsync("test/file.png");

        // Assert
        Assert.True(result.IsFailure);
    }

    // ==================== ExistsAsync ====================
    
    /// <summary>
    /// Sjekker at en fil eksisterer
    /// </summary>
    [Fact]
    public async Task ExistsAsync_FileExists_ReturnsTrue()
    {
        // Arrange
        _mockS3Client
            .Setup(s => s.GetObjectMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectMetadataResponse());

        // Act
        var result = await _sut.ExistsAsync("test/file.png");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }
    
    /// <summary>
    /// Sjekker at en fil ikke eksisterer
    /// </summary>
    [Fact]
    public async Task ExistsAsync_FileNotFound_ReturnsFalse()
    {
        // Arrange
        _mockS3Client
            .Setup(s => s.GetObjectMetadataAsync(It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Not Found") { StatusCode = System.Net.HttpStatusCode.NotFound });

        // Act
        var result = await _sut.ExistsAsync("nonexistent/file.png");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }
    
    /// <summary>
    /// Tester om en annen uforventet feil oppstår ved eksistens-sjekk
    /// </summary>
    [Fact]
    public async Task ExistsAsync_S3Exception_ReturnsFailure()
    {
        // Arrange
        _mockS3Client
            .Setup(s => s.GetObjectMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("S3 error"));

        // Act
        var result = await _sut.ExistsAsync("test/file.png");

        // Assert
        Assert.True(result.IsFailure);
    }
}