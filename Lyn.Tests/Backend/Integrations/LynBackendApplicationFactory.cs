using Amazon.S3;
using Amazon.S3.Model;
using Lyn.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using Resend;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace Lyn.Tests.Backend.Integrations;

/// <summary>
/// Starter backend-applikasjonen med ekte PostgreSQL i en Docker-container (Testcontainers).
/// Eksterne tjenester (e-post via Resend, lagring via S3) erstattes med no-op-mocker.
/// Speiler oppsettet i AFBack sin BackendApplicationFactory (uten Redis — Lyn bruker ikke Redis).
/// </summary>
public class LynBackendApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    private Respawner _respawner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Overstyr konfigurasjon med testverdier — kjorer nar hosten bygges (ved forste tilgang til Services)
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(TestConfiguration.Build(_postgres.GetConnectionString()));
        });

        // Erstatt eksterne tjenester med no-op-mocker slik at ingen ekte HTTP-kall gjores.
        builder.ConfigureServices(services =>
        {
            // ===== Mock S3 =====
            var mockS3 = new Mock<IAmazonS3>();
            mockS3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse());
            mockS3.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream([1, 2, 3]),
                    ContentLength = 3
                });
            mockS3.Setup(s => s.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteObjectResponse());

            services.RemoveAll<IAmazonS3>();
            services.AddSingleton(mockS3.Object);

            // ===== Mock Resend =====
            var mockResend = new Mock<IResend>();
            mockResend
                .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));

            services.RemoveAll<IResend>();
            services.AddSingleton(mockResend.Object);
        });
    }

    public async Task InitializeAsync()
    {
        // Start containeren — ma skje for Services aksesseres
        await _postgres.StartAsync();

        // Forste tilgang til Services bygger hosten og kjorer Program.cs (inkl. migrasjoner).
        await using var scope = Services.CreateAsyncScope();

        // Sett opp Respawn for a nullstille databasen mellom tester
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            // Bevar migrasjonshistorikk (begge kontekstene) og seed-data som settes en gang
            // per fabrikk-instans (roller, rolleclaims, statistikk-seed via HasData).
            TablesToIgnore =
            [
                new Table("__EFMigrationsHistory"),
                new Table("__EFMigrationsHistory_Calorie"),
                new Table("AspNetRoles"),
                new Table("AspNetRoleClaims"),
                new Table("PasswordGeneratorUsageStatistics"),
            ],
        });
    }

    /// <summary>
    /// Legg inn testdata via EF Core. Bruk i stedet for raa SQL.
    /// </summary>
    public async Task SeedAsync(Func<AppDbContext, Task> seed)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await seed(db);
    }

    /// <summary>
    /// Kjorer en sporing mot databasen og returnerer resultatet.
    /// Bruk for a lese testdata etter API-kall (verifisere sideeffekter).
    /// </summary>
    public async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    /// <summary>
    /// Nullstiller alle tabeller unntatt seed-/migrasjonstabeller. Kall i hver tests DisposeAsync.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
