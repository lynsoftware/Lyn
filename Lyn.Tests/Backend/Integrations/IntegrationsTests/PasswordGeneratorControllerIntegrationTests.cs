using System.Net;
using System.Text;
using System.Text.Json;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;

namespace Lyn.Tests.Backend.Integrations.IntegrationsTests;

public class PasswordGeneratorControllerIntegrationTests(LynBackendApplicationFactory factory)
    : IClassFixture<LynBackendApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static StringContent CreateJsonContent(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    [Fact]
    public async Task GeneratePassword_ValidRequest_ReturnsOk()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "321",
            Seed = "321",
            Length = 16,
            IncludeSpecialChars = true
        };

        var response = await _client.PostAsync("/api/PasswordGenerator", CreateJsonContent(request));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePassword_ValidRequest_ReturnsDeterministicPassword()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "321",
            Seed = "321",
            Length = 16,
            IncludeSpecialChars = true
        };

        var response = await _client.PostAsync("/api/PasswordGenerator", CreateJsonContent(request));
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PasswordGenerationResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal("Cat1txGIY7_jTeoe", result!.Value);
    }

    [Fact]
    public async Task GeneratePassword_TooShortLength_Returns400()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test",
            Seed = "test",
            Length = 3,
            IncludeSpecialChars = true
        };

        var response = await _client.PostAsync("/api/PasswordGenerator", CreateJsonContent(request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePassword_MissingMasterPassword_Returns400()
    {
        var json = JsonSerializer.Serialize(new { Seed = "test", Length = 16, IncludeSpecialChars = true });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/PasswordGenerator", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePassword_MissingSeed_Returns400()
    {
        var json = JsonSerializer.Serialize(new { MasterPassword = "test", Length = 16, IncludeSpecialChars = true });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/PasswordGenerator", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePassword_EmptyBody_Returns400()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/PasswordGenerator", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}