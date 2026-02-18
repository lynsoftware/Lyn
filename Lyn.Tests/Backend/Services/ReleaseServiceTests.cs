using Lyn.Backend.DTOs.Request;
using Lyn.Backend.Models;
using Lyn.Backend.Repository;
using Lyn.Backend.Services;
using Lyn.Backend.Services.Interface;
using Lyn.Backend.Validators;
using Lyn.Shared.Enum;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyn.Tests.Backend.Services;

public class ReleaseServiceTests
{
    private readonly ReleaseService _sut;
    private readonly Mock<IFileValidator> _mockFileValidator;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<IReleaseRepository> _mockReleaseRepository;

    public ReleaseServiceTests()
    {
        var mockLogger = new Mock<ILogger<ReleaseService>>();
        _mockFileValidator = new Mock<IFileValidator>();
        _mockStorageService = new Mock<IStorageService>();
        _mockReleaseRepository = new Mock<IReleaseRepository>();

        _sut = new ReleaseService(
            mockLogger.Object,
            _mockFileValidator.Object,
            _mockStorageService.Object,
            _mockReleaseRepository.Object);
    }

    // ==================== Hjelpemetoder ====================

    private static Mock<IFormFile> CreateMockFile(string fileName = "app.apk",
        string contentType = "application/vnd.android.package-archive", long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([1, 2, 3]));
        return mockFile;
    }

    private static UploadReleaseRequest CreateValidUploadRequest(Mock<IFormFile>? file = null) => new()
    {
        Version = "1.0.0",
        Type = ReleaseType.AndroidApk,
        File = (file ?? CreateMockFile()).Object,
        ReleaseNotes = "Initial release"
    };

    /// <summary>
    /// Setter opp mocks for en vellykket upload-flow
    /// </summary>
    private void SetupSuccessfulUpload()
    {
        _mockReleaseRepository
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ReleaseType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockFileValidator
            .Setup(v => v.ValidateReleaseFile(It.IsAny<IFormFile>(), It.IsAny<ReleaseType>()))
            .Returns(Result.Success());
        _mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
    }

    // ==================== UploadReleaseAsync ====================

    /// <summary>
    /// Tester vellykket opplasting av en release
    /// </summary>
    [Fact]
    public async Task UploadReleaseAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        SetupSuccessfulUpload();
        var request = CreateValidUploadRequest();

        // Act
        var result = await _sut.UploadReleaseAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tester at duplikat release returnerer Failure
    /// </summary>
    [Fact]
    public async Task UploadReleaseAsync_ReleaseAlreadyExists_ReturnsFailure()
    {
        // Arrange
        _mockReleaseRepository
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ReleaseType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var request = CreateValidUploadRequest();

        // Act
        var result = await _sut.UploadReleaseAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at ugyldig fil returnerer Failure
    /// </summary>
    [Fact]
    public async Task UploadReleaseAsync_InvalidFile_ReturnsFailure()
    {
        // Arrange
        _mockReleaseRepository
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ReleaseType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockFileValidator
            .Setup(v => v.ValidateReleaseFile(It.IsAny<IFormFile>(), It.IsAny<ReleaseType>()))
            .Returns(Result.Failure("Invalid file"));
        var request = CreateValidUploadRequest();

        // Act
        var result = await _sut.UploadReleaseAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at S3 upload-feil returnerer Failure
    /// </summary>
    [Fact]
    public async Task UploadReleaseAsync_S3UploadFails_ReturnsFailure()
    {
        // Arrange
        _mockReleaseRepository
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<ReleaseType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockFileValidator
            .Setup(v => v.ValidateReleaseFile(It.IsAny<IFormFile>(), It.IsAny<ReleaseType>()))
            .Returns(Result.Success());
        _mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Upload failed"));
        var request = CreateValidUploadRequest();

        // Act
        var result = await _sut.UploadReleaseAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at database-feil returnerer Failure og rydder opp S3-fil
    /// </summary>
    [Fact]
    public async Task UploadReleaseAsync_DatabaseFails_ReturnsFailureAndCleansUp()
    {
        // Arrange
        SetupSuccessfulUpload();
        _mockReleaseRepository
            .Setup(r => r.CreateAsync(It.IsAny<AppRelease>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        _mockStorageService
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var request = CreateValidUploadRequest();

        // Act
        var result = await _sut.UploadReleaseAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockStorageService.Verify(s => s.DeleteAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tester at repository lagrer release ved suksess
    /// </summary>
    [Fact]
    public async Task UploadReleaseAsync_Success_CallsRepositoryCreate()
    {
        // Arrange
        SetupSuccessfulUpload();
        var request = CreateValidUploadRequest();

        // Act
        await _sut.UploadReleaseAsync(request, CancellationToken.None);

        // Assert
        _mockReleaseRepository.Verify(r => r.CreateAsync(
            It.IsAny<AppRelease>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==================== GetLatestAsync ====================

    /// <summary>
    /// Tester at GetLatest returnerer releases
    /// </summary>
    [Fact]
    public async Task GetLatestAsync_ReleasesExist_ReturnsSuccess()
    {
        // Arrange
        var releases = new List<AppRelease>
        {
            new()
            {
                Id = 1, FileName = "app.apk", Type = ReleaseType.AndroidApk,
                Version = "1.0.0", FileSizeBytes = 1024, UploadedAt = DateTime.UtcNow
            }
        };
        _mockReleaseRepository
            .Setup(r => r.GetLatestAsync())
            .ReturnsAsync(releases);

        // Act
        var result = await _sut.GetLatestAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    /// <summary>
    /// Tester at GetLatest returnerer Failure når ingen releases finnes
    /// </summary>
    [Fact]
    public async Task GetLatestAsync_NoReleases_ReturnsFailure()
    {
        // Arrange
        _mockReleaseRepository
            .Setup(r => r.GetLatestAsync())
            .ReturnsAsync(new List<AppRelease>());

        // Act
        var result = await _sut.GetLatestAsync();

        // Assert
        Assert.True(result.IsFailure);
    }

    // ==================== DownloadAsync ====================

    /// <summary>
    /// Tester vellykket nedlasting av en release
    /// </summary>
    [Fact]
    public async Task DownloadAsync_ValidId_ReturnsFileDownload()
    {
        // Arrange
        var release = new AppRelease
        {
            Id = 1, StorageKey = "releases/test/1.0.0/file.apk",
            ContentType = "application/vnd.android.package-archive",
            Type = ReleaseType.AndroidApk, Version = "1.0.0", FileExtension = ".apk"
        };
        _mockReleaseRepository
            .Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        _mockStorageService
            .Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Stream>.Success(new MemoryStream([1, 2, 3])));

        // Act
        var result = await _sut.DownloadAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Stream);
    }

    /// <summary>
    /// Tester at nedlasting feiler når release ikke finnes i databasen
    /// </summary>
    [Fact]
    public async Task DownloadAsync_ReleaseNotFound_ReturnsFailure()
    {
        // Arrange
        _mockReleaseRepository
            .Setup(r => r.GetAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppRelease?)null);

        // Act
        var result = await _sut.DownloadAsync(999);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at nedlasting feiler når S3 download feiler
    /// </summary>
    [Fact]
    public async Task DownloadAsync_S3DownloadFails_ReturnsFailure()
    {
        // Arrange
        var release = new AppRelease
        {
            Id = 1, StorageKey = "releases/test/1.0.0/file.apk"
        };
        _mockReleaseRepository
            .Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        _mockStorageService
            .Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Stream>.Failure("Download failed"));

        // Act
        var result = await _sut.DownloadAsync(1);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at download count inkrementeres ved vellykket nedlasting
    /// </summary>
    [Fact]
    public async Task DownloadAsync_Success_IncrementsDownloadCount()
    {
        // Arrange
        var release = new AppRelease
        {
            Id = 1, StorageKey = "releases/test/1.0.0/file.apk",
            ContentType = "application/vnd.android.package-archive",
            Type = ReleaseType.AndroidApk, Version = "1.0.0", FileExtension = ".apk"
        };
        _mockReleaseRepository
            .Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        _mockStorageService
            .Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Stream>.Success(new MemoryStream([1, 2, 3])));

        // Act
        await _sut.DownloadAsync(1);

        // Assert
        _mockReleaseRepository.Verify(r => r.IncrementDownloadCountAsync(
            1, It.IsAny<CancellationToken>()), Times.Once);
    }
}