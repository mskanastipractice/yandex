using Auth.Application.Interfaces;
using Auth.Application.UseCases;
using Auth.Domain.User;
using Auth.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Auth.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
    private readonly Guid _userId = Guid.NewGuid();
    protected const string UserLogin = "testlogin";
    protected const string UserPassword = "testpass";

    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<ICurrentUserContext> _userContextMock = new();
    protected readonly Mock<IPasswordHasher> PasswordHasherMock = new();
    protected readonly Mock<IUserRepository> UserRepositoryMock = new();

    protected readonly IServiceProvider ServiceProvider;

    protected BaseUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        ConfigureMockServices(services);

        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        var user = User.Create(_userId, UserLogin, "random_string", UserRole.User);

        _userContextMock.Setup(x => x.UserId).Returns(_userId);
        _userContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        _jwtProviderMock.Setup(provider => provider.GenerateToken(user)).Returns("jwt_token");

        PasswordHasherMock.Setup(hasher => hasher.Hash(UserPassword)).Returns("random_password_hash");
        PasswordHasherMock.Setup(hasher => hasher.Verify(UserPassword, It.IsAny<string>()))
            .Returns(true);

        UserRepositoryMock.Setup(repo => repo
                .FindByLoginAsync(UserLogin, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        UserRepositoryMock.Setup(repo => repo.ExistsByLoginAsync(UserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        UserRepositoryMock.Setup(repo => repo.AddAsync(user));
        UserRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        services.AddSingleton(_userContextMock.Object);
        services.AddSingleton(_jwtProviderMock.Object);
        services.AddSingleton(PasswordHasherMock.Object);
        services.AddSingleton(UserRepositoryMock.Object);
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}