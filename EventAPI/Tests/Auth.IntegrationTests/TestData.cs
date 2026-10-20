using Auth.Domain.User;

namespace Auth.IntegrationTests;

internal static class TestData
{
    public static readonly Guid UserId = Guid.NewGuid();
    public const string Login = "logintest";
    public const string Password = "loginpass";
    public static User TestUser => User.Create(UserId, Login, Password, UserRole.User);
}