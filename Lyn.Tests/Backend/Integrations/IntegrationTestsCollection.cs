namespace Lyn.Tests.Backend.Integrations;

/// <summary>
/// Sikrer at alle integrasjonstester i samlingen deler en enkelt
/// LynBackendApplicationFactory-instans. En container-instans per testkjoring —
/// ikke per test.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestsCollection))]
public class IntegrationTestsCollection : ICollectionFixture<LynBackendApplicationFactory>;
