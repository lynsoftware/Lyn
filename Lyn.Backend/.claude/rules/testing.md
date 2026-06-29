# Testing-regler — Lyn.Backend

**Framework:** xUnit + Moq + FluentAssertions. Integrasjonstester mot ekte PostgreSQL via Testcontainers.

## Kjør tester

```bash
# ALLTID target testprosjektet — IKKE "dotnet test" på solution.
# Solution-bygg trekker inn MAUI-appen (net10.0-android) som krever Android SDK 36.
dotnet test Lyn.Tests/Lyn.Tests.csproj

# Spesifikk gruppe
dotnet test Lyn.Tests/Lyn.Tests.csproj --filter "FullyQualifiedName~Integrations"
```

Integrasjonstester krever at **Docker kjører** (Testcontainers starter en ekte Postgres-container).

## Teststruktur

```
Lyn.Tests/Backend/
├── Integrations/
│   ├── LynBackendApplicationFactory.cs   # Testcontainers Postgres + Respawn + mocks
│   ├── TestConfiguration.cs              # Overstyrer appsettings (connection string, Jwt, dummy-verdier)
│   ├── IntegrationTestsCollection.cs     # Delt factory-instans (collection-fixture)
│   └── IntegrationsTests/                # Selve testene
├── Controllers/  Services/  Middleware/  Validators/   # Unit-tester
```

## Integrasjonstest-mønster

Factoryen (`LynBackendApplicationFactory`) starter én Postgres-container per testkjøring, kjører migrasjonene (begge kontekster via Program.cs), og mocker eksterne tjenester (`IAmazonS3`, `IResend`). Respawn nullstiller databasen mellom tester.

```csharp
[Collection(nameof(IntegrationTestsCollection))]
public class SupportControllerIntegrationTests(LynBackendApplicationFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await factory.ResetDatabaseAsync();   // Respawn mellom tester

    [Fact]
    public async Task Method_WhenCondition_ShouldExpectedBehavior() { /* Arrange / Act / Assert */ }
}
```

- **Container-versjon:** `postgres:18-alpine` — speiler prod-motoren.
- **Seed/les data:** `factory.SeedAsync(db => ...)` og `factory.QueryAsync(db => ...)` (mot `AppDbContext`).
- **Respawn ignore-liste:** `__EFMigrationsHistory`, `__EFMigrationsHistory_Calorie`, `AspNetRoles`, `AspNetRoleClaims`, `PasswordGeneratorUsageStatistics` (seed-/migrasjonsdata som ikke skal nullstilles mellom tester).

## Unit-test-mønster

```csharp
[Fact]
public async Task Method_WhenCondition_ShouldExpectedBehavior()
{
    // Arrange
    var mockDep = new Mock<IDependency>();
    mockDep.Setup(d => d.GetAsync(It.IsAny<T>())).ReturnsAsync(Result<T>.Success(value));
    var sut = new Service(mockDep.Object);

    // Act
    var result = await sut.MethodAsync(input);

    // Assert
    result.IsSuccess.Should().BeTrue();
    mockDep.Verify(d => d.GetAsync(It.IsAny<T>()), Times.Once);
}
```

## FluentAssertions + Result-pattern

```csharp
result.IsSuccess.Should().BeTrue();
result.IsFailure.Should().BeFalse();
result.ErrorType.Should().Be(ErrorTypeEnum.NotFound);
result.Value.Should().NotBeNull();
```

## Pakker (Lyn.Tests.csproj)

`Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`, `Respawn`, `Moq`, `FluentAssertions`, `xunit`. Ingen `EntityFrameworkCore.InMemory` — integrasjonstester kjører mot ekte Postgres.
