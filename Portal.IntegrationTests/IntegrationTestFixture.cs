namespace Portal.IntegrationTests;

/// <summary>
/// Shared fixture that starts the PostgreSQL container and ensures the database schema exists
/// before any integration tests run. Use with IClassFixture or ICollectionFixture.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public PortalWebApplicationFactory Factory { get; } = new PortalWebApplicationFactory();

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
        _ = Factory.Server; // ensure host is built so Services are available
        await Factory.EnsureDatabaseSchemaAsync();
    }

    public async Task DisposeAsync()
    {

    }
}

/// <summary>
/// Marker for the integration test collection so the fixture is shared and container starts once.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
