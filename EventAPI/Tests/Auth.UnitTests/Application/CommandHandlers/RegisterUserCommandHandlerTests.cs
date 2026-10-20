using Auth.Application.Contracts.Commands;
using Auth.Application.Exceptions;
using Auth.Application.UseCases;
using Auth.Domain.User;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Auth.UnitTests.Application.CommandHandlers;

public class RegisterUserCommandHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет успешную обработку команды <see cref="RegisterUserCommand"/>>.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RegisterUserCommandHandler>();
        var command = new RegisterUserCommand(UserLogin, UserPassword, UserRole.User);

        //Act
        await handler.Handle(command, CancellationToken.None);

        //Assert
        PasswordHasherMock.Verify(
            hasher => hasher.Hash(It.Is<string>(x => x == UserPassword)),
            Times.Once);

        UserRepositoryMock.Verify(
            repo => repo.ExistsByLoginAsync(It.Is<string>(x => x == UserLogin), It.IsAny<CancellationToken>()),
            Times.Once);

        UserRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<User>(x =>
                x.Login == UserLogin && x.Role == UserRole.User)),
            Times.Once);

        UserRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Проверяет обработку команды <see cref="RegisterUserCommand"/>>, если пользователь уже зарегистрирован в системе.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldThrowUserAlreadyExistsException()
    {
        //Arrange
        UserRepositoryMock.Setup(repo => repo.ExistsByLoginAsync(UserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RegisterUserCommandHandler>();
        var command = new RegisterUserCommand(UserLogin, UserPassword, UserRole.User);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UserAlreadyExistsException>();

        //Assert
        PasswordHasherMock.Verify(
            hasher => hasher.Hash(It.Is<string>(x => x == UserPassword)),
            Times.Once);

        UserRepositoryMock.Verify(
            repo => repo.ExistsByLoginAsync(It.Is<string>(x => x == UserLogin), It.IsAny<CancellationToken>()),
            Times.Once);

        UserRepositoryMock.Verify(
            repo => repo.AddAsync(It.IsAny<User>()),
            Times.Never);

        UserRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}