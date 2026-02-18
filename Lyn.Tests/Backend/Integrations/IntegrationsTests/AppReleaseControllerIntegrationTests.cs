using System.Net;
using System.Net.Http.Headers;

namespace Lyn.Tests.Backend.Integrations.IntegrationsTests;

public class AppReleaseControllerIntegrationTests(LynBackendApplicationFactory factory)
    : IClassFixture<LynBackendApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string ApiKey = "test-release-key"; // Matcher factory-konfigurasjonen

    private static MultipartFormDataContent CreateValidUploadContent()
    {
        var fileContent = new ByteArrayContent([1, 2, 3]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");

        var form = new MultipartFormDataContent
        {
            { new StringContent("1.0.0"), "Version" },
            { new StringContent("0"), "Type" }, // ReleaseType enum verdi
            { fileContent, "File", "app.apk" },
            { new StringContent("Test release"), "ReleaseNotes" }
        };
        return form;
    }

    // ==================== Upload - API Key ====================

    [Fact]
    public async Task Upload_WithoutApiKey_Returns401()
    {
        var content = CreateValidUploadContent();

        var response = await _client.PostAsync("/api/AppRelease/upload", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithWrongApiKey_Returns401()
    {
        var content = CreateValidUploadContent();
        _client.DefaultRequestHeaders.Remove("X-Api-Key");
        _client.DefaultRequestHeaders.Add("X-Api-Key", "wrong-key");

        var response = await _client.PostAsync("/api/AppRelease/upload", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithValidApiKey_DoesNotReturn401()
    {
        var content = CreateValidUploadContent();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/AppRelease/upload")
        {
            Content = content
        };
        request.Headers.Add("X-Api-Key", ApiKey);

        var response = await _client.SendAsync(request);

        // Kan returnere 400 pga filvalidering, men IKKE 401
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ==================== Upload - Validering ====================

    [Fact]
    public async Task Upload_MissingVersion_Returns400()
    {
        var fileContent = new ByteArrayContent([1, 2, 3]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");

        var form = new MultipartFormDataContent
        {
            { new StringContent("0"), "Type" },
            { fileContent, "File", "app.apk" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/AppRelease/upload")
        {
            Content = form
        };
        request.Headers.Add("X-Api-Key", ApiKey);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_MissingFile_Returns400()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("1.0.0"), "Version" },
            { new StringContent("0"), "Type" }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/AppRelease/upload")
        {
            Content = form
        };
        request.Headers.Add("X-Api-Key", ApiKey);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==================== GetLatest ====================

    [Fact]
    public async Task GetLatest_EmptyDatabase_ReturnsError()
    {
        var response = await _client.GetAsync("/api/AppRelease/latest");

        Assert.False(response.IsSuccessStatusCode);
    }

    // ==================== Download ====================

    [Fact]
    public async Task Download_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync("/api/AppRelease/download/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Download_InvalidId_Returns404Or400()
    {
        var response = await _client.GetAsync("/api/AppRelease/download/abc");

        // ASP.NET returnerer typisk 404 for route-mismatch på int-parameter
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest);
    }
}