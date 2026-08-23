using Xunit;

namespace IntegrationTests.Repositories;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DbFixture>
{
}