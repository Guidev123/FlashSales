namespace Modules.Payments.IntegrationTests.Abstractions
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationWebApplicationFactory>
    {
    }
}
