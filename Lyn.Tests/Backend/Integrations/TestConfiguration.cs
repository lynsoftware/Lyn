namespace Lyn.Tests.Backend.Integrations;

/// <summary>
/// Testverdier som overstyrer appsettings under integrasjonstester.
/// Eksterne tjenester far dummy-verdier — selve implementasjonene erstattes med
/// mocker i LynBackendApplicationFactory.
/// </summary>
public static class TestConfiguration
{
    public static Dictionary<string, string?> Build(string pgConnectionString) => new()
    {
        // Database — peker mot Testcontainers-Postgres
        ["ConnectionStrings:DefaultConnection"] = pgConnectionString,

        // JWT — Key ma vaere minst 32 tegn (DataAnnotation i JwtSettings)
        ["JwtSettings:Key"] = "test_key_minimum_32_characters_long_for_testing",
        ["JwtSettings:Issuer"] = "TestIssuer",
        ["JwtSettings:Audience"] = "TestAudience",
        ["JwtSettings:TokenValidityMinutes"] = "5",

        // Eksterne tjenester — dummy-verdier slik at tjenestene registreres uten feil.
        // Implementasjonene (Resend, S3) erstattes med mocker i factoryen.
        ["Resend:ApiKey"] = "test-api-key",
        ["AWS:BucketName"] = "test-bucket",
        ["AWS:Region"] = "eu-north-1",
        ["ReleaseApiKey"] = "test-release-key",

        // Admin-bruker — seedes ved oppstart av DatabaseSeeder
        ["AdminUser:Email"] = "dev@lynsoftware.com",
        ["AdminUser:Password"] = "DevAdmin123!",
    };
}
