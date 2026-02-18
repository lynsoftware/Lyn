using Lyn.Backend.Repository;
using Lyn.Backend.Services;
using Lyn.Backend.Services.Interface;
using Lyn.Backend.Validators;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyn.Tests.Backend.Services;

public class SupportTicketServiceTests
{
    private readonly SupportTicketService _sut;
    private readonly Mock<ISupportRepository> _mockSupportRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IFileValidator> _mockFileValidator;
    private readonly Mock<IStorageService> _mockStorageService;

    public SupportTicketServiceTests()
    {
        var mockLogger = new Mock<ILogger<SupportTicketService>>();
        _mockSupportRepository = new Mock<ISupportRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockFileValidator = new Mock<IFileValidator>();
        _mockStorageService = new Mock<IStorageService>();

        _sut = new SupportTicketService(
            mockLogger.Object,
            _mockSupportRepository.Object,
            _mockEmailService.Object,
            _mockFileValidator.Object,
            _mockStorageService.Object);
    }
    
    // ==================== Hjelpemetoder ====================
    
    /// <summary>
    /// Hjelpemetode for å opprette en gyldig request
    /// </summary>
    private static SupportTicketRequest CreateValidRequest() => new()
    {
        Email = "user@test.com",
        Title = "Test ticket",
        Category = "Bug",
        Description = "Something is broken"
    };

    /// <summary>
    /// Hjelpemetode for å opprette en mock IFormFile
    /// </summary>
    private static Mock<IFormFile> CreateMockFile(string fileName = "test.png", string contentType = "image/png", 
        long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([1, 2, 3]));
        return mockFile;
    }

    // ==================== CreateSupportTicketAsync - Uten vedlegg ====================

    /// <summary>
    /// Tester opprettelse av support ticket uten vedlegg
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_NoAttachments_ReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.NumberOfAttachments);
    }

    /// <summary>
    /// Tester at repository kalles ved opprettelse
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_NoAttachments_CallsRepository()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        await _sut.CreateSupportTicketAsync(request, null);

        // Assert
        // Sjekker at ISupportRepository er kalt engang
        _mockSupportRepository.Verify(r => r.CreateSupportTicketAsync(
            It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tester at e-poster sendes etter vellykket opprettelse
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_Success_SendsEmails()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        await _sut.CreateSupportTicketAsync(request, null);

        // Assert
        _mockEmailService.Verify(e => e.SendSupportTicketConfirmationAsync(
            It.IsAny<SupportTicket>()), Times.Once);
        _mockEmailService.Verify(e => e.SupportTicketReceivedNotificationEmail(
            It.IsAny<SupportTicket>()), Times.Once);
    }

    /// <summary>
    /// Tester at e-postfeil ikke krasjer opprettelsen
    /// Mocker at EmailService returnerer feil
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_EmailFails_StillReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        _mockEmailService
            .Setup(e => e.SendSupportTicketConfirmationAsync(It.IsAny<SupportTicket>()))
            .ThrowsAsync(new Exception("Email failed"));

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, null);

        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tester at database-feil returnerer Failure
    /// Mocker at ISupportRepository kaster en exception
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_DatabaseFails_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        _mockSupportRepository
            .Setup(r => r.CreateSupportTicketAsync(It.IsAny<SupportTicket>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, null);

        // Assert
        Assert.True(result.IsFailure);
    }

    // ==================== CreateSupportTicketAsync - Med vedlegg ====================

    /// <summary>
    /// Tester opprettelse med gyldige vedlegg
    /// Mocker at FileValidator og StorageService returnerer suksess
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_WithValidAttachments_ReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        var mockFile = CreateMockFile();
        var attachments = new List<IFormFile> { mockFile.Object };

        _mockFileValidator
            .Setup(v => v.ValidateSupportAttachment(It.IsAny<IFormFile>()))
            .Returns(Result.Success());
        _mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, attachments);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.NumberOfAttachments);
    }

    /// <summary>
    /// Tester at for mange vedlegg returnerer Failure
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_TooManyAttachments_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var attachments = new List<IFormFile>();
        for (int i = 0; i < SupportTicketFileConfig.TicketMaxFileCount + 1; i++)
            attachments.Add(CreateMockFile().Object);

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, attachments);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at ugyldig fil returnerer Failure
    /// Mocker at FileValidator returner invalid file type
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_InvalidFile_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var attachments = new List<IFormFile> { CreateMockFile().Object };

        _mockFileValidator
            .Setup(v => v.ValidateSupportAttachment(It.IsAny<IFormFile>()))
            .Returns(Result.Failure("Invalid file type"));

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, attachments);

        // Assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at S3 upload-feil returnerer Failure
    /// Mocker at FileValidator returnerer suksess, men StorageService returnerer feil
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_UploadFails_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var attachments = new List<IFormFile> { CreateMockFile().Object };

        _mockFileValidator
            .Setup(v => v.ValidateSupportAttachment(It.IsAny<IFormFile>()))
            .Returns(Result.Success());
        _mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Upload failed"));

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, attachments);

        // Assert
        Assert.True(result.IsFailure);
    }

    // ==================== Cleanup ====================

    /// <summary>
    /// Tester at filer ryddes opp fra S3 når database-lagring feiler
    /// Mocker at FileValidator returnerer suksess, at storageService returnerer suksess begge ganger,
    /// og at repository returnerer Database-error
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_DatabaseFailsWithAttachments_CleansUpFiles()
    {
        // Arrange
        var request = CreateValidRequest();
        var attachments = new List<IFormFile> { CreateMockFile().Object };

        _mockFileValidator
            .Setup(v => v.ValidateSupportAttachment(It.IsAny<IFormFile>()))
            .Returns(Result.Success());
        _mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _mockStorageService
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _mockSupportRepository
            .Setup(r => r.CreateSupportTicketAsync(It.IsAny<SupportTicket>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _sut.CreateSupportTicketAsync(request, attachments);

        // Assert
        _mockStorageService.Verify(s => s.DeleteAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tester at filer ryddes opp når andre fil i rekken feiler validering.
    /// Mocker at FileValidator kaller feil på fil nr 2, og StorageService returnerer suksess to ganger
    /// </summary>
    [Fact]
    public async Task CreateSupportTicketAsync_SecondFileFailsValidation_CleansUpFirstFile()
    {
        // Arrange
        var request = CreateValidRequest();
        var file1 = CreateMockFile("valid.png");
        var file2 = CreateMockFile("invalid.exe");
        var attachments = new List<IFormFile> { file1.Object, file2.Object };

        var callCount = 0;
        _mockFileValidator
            .Setup(v => v.ValidateSupportAttachment(It.IsAny<IFormFile>()))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1 ? Result.Success() : Result.Failure("Invalid file");
            });
        _mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _mockStorageService
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.CreateSupportTicketAsync(request, attachments);

        // Assert
        Assert.True(result.IsFailure);
        _mockStorageService.Verify(s => s.DeleteAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}