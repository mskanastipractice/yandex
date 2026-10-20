using Xunit;

namespace Auth.IntegrationTests.Repositories.Base;

[CollectionDefinition("Database collection")]
public class DbCollection : ICollectionFixture<DbFixture>
{
}