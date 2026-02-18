using System.Net;

namespace Lyn.Tests.Backend.Integrations.IntegrationsTests;

public class HealthCheckTests(LynBackendApplicationFactory factory) : IClassFixture<LynBackendApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}