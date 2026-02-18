using Lyn.Backend.Validators;
using Lyn.Shared.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyn.Tests.Backend.Validators;

public class FileValidatorTests
{
    private readonly FileValidator _sut;
    
    // Magic bytes for vanlige filtyper
    private static readonly byte[] JpegMagicBytes = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagicBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] PdfMagicBytes = [0x25, 0x50, 0x44, 0x46];
    private static readonly byte[] GifMagicBytes = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61];

    public FileValidatorTests()
    {
        var mockLogger = new Mock<ILogger<FileValidator>>();
        _sut = new FileValidator(mockLogger.Object);
    }

    /// <summary>
    /// Hjelpemetode for å opprette en mock IFormFile med riktige magic bytes
    /// </summary>
    private static Mock<IFormFile> CreateMockFile(
        string fileName = "test.jpg",
        string contentType = "image/jpeg",
        long length = 1024,
        byte[]? magicBytes = null)
    {
        // Bruk JPEG magic bytes som default, paddet til 16 bytes for buffer-lesing
        var content = new byte[16];
        var bytes = magicBytes ?? JpegMagicBytes;
        Array.Copy(bytes, content, bytes.Length);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(content));
        return mockFile;
    }

    // ==================== ValidateNotEmpty ====================

    [Fact]
    public void ValidateNotEmpty_FileWithContent_ReturnsSuccess()
    {
        var file = CreateMockFile(length: 1024);
        var result = _sut.ValidateNotEmpty(file.Object);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateNotEmpty_EmptyFile_ReturnsFailure()
    {
        var file = CreateMockFile(length: 0);
        var result = _sut.ValidateNotEmpty(file.Object);
        Assert.True(result.IsFailure);
    }

    // ==================== ValidateSize ====================

    [Fact]
    public void ValidateSize_UnderLimit_ReturnsSuccess()
    {
        var file = CreateMockFile(length: 1024);
        var result = _sut.ValidateSize(file.Object, 2048);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateSize_OverLimit_ReturnsFailure()
    {
        var file = CreateMockFile(length: 5000);
        var result = _sut.ValidateSize(file.Object, 2048);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateSize_ExactLimit_ReturnsSuccess()
    {
        var file = CreateMockFile(length: 2048);
        var result = _sut.ValidateSize(file.Object, 2048);
        Assert.True(result.IsSuccess);
    }

    // ==================== ValidateExtension ====================

    [Fact]
    public void ValidateExtension_ValidExtension_ReturnsSuccessWithExtension()
    {
        var file = CreateMockFile(fileName: "test.png");
        var result = _sut.ValidateExtension(file.Object, [".png", ".jpg"]);
        Assert.True(result.IsSuccess);
        Assert.Equal(".png", result.Value);
    }

    [Fact]
    public void ValidateExtension_InvalidExtension_ReturnsFailure()
    {
        var file = CreateMockFile(fileName: "test.exe");
        var result = _sut.ValidateExtension(file.Object, [".png", ".jpg"]);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateExtension_NoExtension_ReturnsFailure()
    {
        var file = CreateMockFile(fileName: "testfile");
        var result = _sut.ValidateExtension(file.Object, [".png", ".jpg"]);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateExtension_UpperCase_NormalizesAndReturnsSuccess()
    {
        var file = CreateMockFile(fileName: "test.PNG");
        var result = _sut.ValidateExtension(file.Object, [".png"]);
        Assert.True(result.IsSuccess);
        Assert.Equal(".png", result.Value);
    }

    // ==================== ValidateContentType ====================

    [Fact]
    public void ValidateContentType_ValidType_ReturnsSuccess()
    {
        var file = CreateMockFile(contentType: "image/png");
        var result = _sut.ValidateContentType(file.Object, ["image/png", "image/jpeg"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateContentType_InvalidType_ReturnsFailure()
    {
        var file = CreateMockFile(contentType: "application/exe");
        var result = _sut.ValidateContentType(file.Object, ["image/png", "image/jpeg"]);
        Assert.True(result.IsFailure);
    }

    // ==================== ValidateSupportAttachment ====================

    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("animation.gif", "image/gif")]
    [InlineData("photo.webp", "image/webp")]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("notes.txt", "text/plain")]
    [InlineData("debug.log", "text/plain")]
    public void ValidateSupportAttachment_AllSupportedTypes_ReturnsSuccess(string fileName, string contentType)
    {
        // Velg riktige magic bytes basert på extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var magicBytes = extension switch
        {
            ".jpg" or ".jpeg" => JpegMagicBytes,
            ".png" => PngMagicBytes,
            ".gif" => GifMagicBytes,
            ".webp" => [0x52, 0x49, 0x46, 0x46],
            ".pdf" => PdfMagicBytes,
            ".txt" or ".log" => [0x48, 0x65, 0x6C, 0x6C], // "Hell" (vilkårlig tekst)
            _ => JpegMagicBytes
        };

        var file = CreateMockFile(fileName: fileName, contentType: contentType, magicBytes: magicBytes);
        var result = _sut.ValidateSupportAttachment(file.Object);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateSupportAttachment_EmptyFile_ReturnsFailure()
    {
        var file = CreateMockFile(length: 0);
        var result = _sut.ValidateSupportAttachment(file.Object);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateSupportAttachment_TooLargeFile_ReturnsFailure()
    {
        var file = CreateMockFile(length: SupportTicketFileConfig.TicketMaxFileSizeBytes + 1);
        var result = _sut.ValidateSupportAttachment(file.Object);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateSupportAttachment_InvalidExtension_ReturnsFailure()
    {
        var file = CreateMockFile(fileName: "virus.exe", contentType: "application/x-msdownload");
        var result = _sut.ValidateSupportAttachment(file.Object);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateSupportAttachment_InvalidContentType_ReturnsFailure()
    {
        var file = CreateMockFile(fileName: "test.jpg", contentType: "application/octet-stream");
        var result = _sut.ValidateSupportAttachment(file.Object);
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Tester at en fil med riktig extension men feil magic bytes feiler.
    /// Simulerer at noen har omdøpt en .exe til .jpg
    /// </summary>
    [Fact]
    public void ValidateSupportAttachment_WrongMagicBytes_ReturnsFailure()
    {
        var exeMagicBytes = new byte[] { 0x4D, 0x5A }; // EXE magic bytes
        var file = CreateMockFile(
            fileName: "fake.jpg",
            contentType: "image/jpeg",
            magicBytes: exeMagicBytes);

        var result = _sut.ValidateSupportAttachment(file.Object);
        Assert.True(result.IsFailure);
    }
}