using System.Net;
using System.Net.Http.Headers;
using Lyn.Shared.Configuration;

namespace Lyn.Tests.Backend.Integrations.IntegrationsTests;

public class SupportControllerIntegrationTests : IClassFixture<LynBackendApplicationFactory>
{
    private readonly HttpClient _client;

    public SupportControllerIntegrationTests(LynBackendApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static MultipartFormDataContent CreateValidTicketContent()
    {
        return new MultipartFormDataContent
        {
            { new StringContent("user@test.com"), "Email" },
            { new StringContent("Bug report"), "Title" },
            { new StringContent("Bug"), "Category" },
            { new StringContent("Something is broken in the app"), "Description" }
        };
    }

    private static ByteArrayContent CreateFakeFileContent(string contentType = "image/png", int size = 1024)
    {
        var data = new byte[size];
        // PNG magic bytes
        byte[] pngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Array.Copy(pngHeader, data, pngHeader.Length);
    
        var content = new ByteArrayContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return content;
    }

    // ==================== Gyldig request ====================

    [Fact]
    public async Task CreateSupportTicket_ValidRequest_ReturnsOk()
    {
        var content = CreateValidTicketContent();

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupportTicket_ValidRequestWithAttachment_ReturnsOk()
    {
        var content = CreateValidTicketContent();
        content.Add(CreateFakeFileContent(), "attachments", "screenshot.png");

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ==================== Validering - Required fields ====================

    [Fact]
    public async Task CreateSupportTicket_MissingEmail_Returns400()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("Bug report"), "Title" },
            { new StringContent("Bug"), "Category" },
            { new StringContent("Something is broken in the app"), "Description" }
        };

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupportTicket_InvalidEmail_Returns400()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("not-an-email"), "Email" },
            { new StringContent("Bug report"), "Title" },
            { new StringContent("Bug"), "Category" },
            { new StringContent("Something is broken in the app"), "Description" }
        };

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupportTicket_MissingTitle_Returns400()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("user@test.com"), "Email" },
            { new StringContent("Bug"), "Category" },
            { new StringContent("Something is broken in the app"), "Description" }
        };

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupportTicket_DescriptionTooShort_Returns400()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("user@test.com"), "Email" },
            { new StringContent("Bug report"), "Title" },
            { new StringContent("Bug"), "Category" },
            { new StringContent("Short"), "Description" } // Under 10 tegn
        };

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupportTicket_MissingCategory_Returns400()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("user@test.com"), "Email" },
            { new StringContent("Bug report"), "Title" },
            { new StringContent("Something is broken in the app"), "Description" }
        };

        var response = await _client.PostAsync("/api/Support", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==================== Vedlegg-validering ====================

    [Fact]
    public async Task CreateSupportTicket_TooManyAttachments_ReturnsBadRequest()
    {
        var content = CreateValidTicketContent();
        for (var i = 0; i < SupportTicketFileConfig.TicketMaxFileCount + 1; i++)
        {
            content.Add(CreateFakeFileContent(), "attachments", $"file{i}.png");
        }

        var response = await _client.PostAsync("/api/Support", content);

        Assert.False(response.IsSuccessStatusCode);
    }
}