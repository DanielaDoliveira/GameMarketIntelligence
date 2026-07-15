namespace GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection
    : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL integration tests";
}