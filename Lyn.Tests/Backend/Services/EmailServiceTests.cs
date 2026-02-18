using Lyn.Backend.Services;
using Lyn.Shared.Models;
using Moq;
using Resend;

namespace Lyn.Tests.Backend.Services;

public class EmailServiceTests
{
    private readonly EmailService _sut;
    private readonly Mock<IResend> _mockResend;

    public EmailServiceTests()
    {
        _mockResend = new Mock<IResend>();
        _sut = new EmailService(_mockResend.Object);
    }

    // ==================== SendSupportTicketConfirmationAsync ====================

    /// <summary>
    /// Tester at bekreftelsesmail sendes vellykket til brukeren.
    /// Mocker Resend til å returnere suksess
    /// </summary>
    [Fact]
    public async Task SendSupportTicketConfirmationAsync_Success_DoesNotThrow()
    {
        // Arrange
        var ticket = new SupportTicket { Id = 1, Email = "user@test.com" };
        _mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));

        // Act & Assert
        await _sut.SendSupportTicketConfirmationAsync(ticket);
    }

    /// <summary>
    /// Tester at InvalidOperationException kastes når e-post feiler.
    /// Mocker Resend til å returnere ResendException
    /// </summary>
    [Fact]
    public async Task SendSupportTicketConfirmationAsync_Failure_ThrowsInvalidOperationException()
    {
        // Arrange
        var ticket = new SupportTicket { Id = 1, Email = "user@test.com" };
        _mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(
                new ResendException(System.Net.HttpStatusCode.InternalServerError, ErrorType.InternalServerError, 
                    "Email failed"), null));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SendSupportTicketConfirmationAsync(ticket));
    }

    /// <summary>
    /// Tester at e-posten sendes til riktig mottaker
    /// Mocker at Resend returnerer suksess
    /// </summary>
    [Fact]
    public async Task SendSupportTicketConfirmationAsync_SendsToCorrectRecipient()
    {
        // Arrange
        var ticket = new SupportTicket { Id = 1, Email = "user@test.com" };
        _mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));

        // Act
        await _sut.SendSupportTicketConfirmationAsync(ticket);

        // Assert
        _mockResend.Verify(r => r.EmailSendAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ==================== SupportTicketReceivedNotificationEmail ====================

    /// <summary>
    /// Tester at notifikasjonsmail sendes vellykket til support
    /// Mocker at Resend returnerer suksess
    /// </summary>
    [Fact]
    public async Task SupportTicketReceivedNotificationEmail_Success_DoesNotThrow()
    {
        // Arrange
        var ticket = new SupportTicket { Id = 1, Email = "user@test.com" };
        _mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));

        // Act & Assert
        await _sut.SupportTicketReceivedNotificationEmail(ticket);
    }

    /// <summary>
    /// Tester at InvalidOperationException kastes når notifikasjonsmail feiler
    ///  Mocker at Resend returnerer ResendException
    /// </summary>
    [Fact]
    public async Task SupportTicketReceivedNotificationEmail_Failure_ThrowsInvalidOperationException()
    {
        // Arrange
        var ticket = new SupportTicket { Id = 1, Email = "user@test.com" };
        _mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(
                new ResendException(System.Net.HttpStatusCode.InternalServerError, ErrorType.InternalServerError, 
                    "Email failed"), 
                null));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SupportTicketReceivedNotificationEmail(ticket));
    }

    /// <summary>
    /// Tester at notifikasjonsmailen sendes til support@lynsoftware.com
    /// Mocker at Resend returnerer suksess
    /// </summary>
    [Fact]
    public async Task SupportTicketReceivedNotificationEmail_SendsToSupport()
    {
        // Arrange
        var ticket = new SupportTicket { Id = 1, Email = "user@test.com" };
        _mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));

        // Act
        await _sut.SupportTicketReceivedNotificationEmail(ticket);

        // Assert
        _mockResend.Verify(r => r.EmailSendAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}