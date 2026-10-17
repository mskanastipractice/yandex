using Xunit;

namespace Auth.IntegrationTests.Repositories.Base;

public abstract class BaseRepositoryTest(DbFixture dbFixture) : IAsyncLifetime
{
    protected readonly DbFixture DbFixture = dbFixture;
    protected readonly DateTime Date = new(2026, 01, 02, 12, 00, 00, DateTimeKind.Utc);

    protected async Task SeedData()
    {
        await using var context = DbFixture.CreateContext();
        context.Users.Add(TestData.TestUser);
        await context.SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        await DbFixture.ClearTablesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}