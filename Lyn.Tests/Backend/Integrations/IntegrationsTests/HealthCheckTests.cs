using System.Net;

namespace Lyn.Tests.Backend.Integrations.IntegrationsTests;

[Collection(nameof(IntegrationTestsCollection))]
public class HealthCheckTests(LynBackendApplicationFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await factory.ResetDatabaseAsync();

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}