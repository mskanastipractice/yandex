using System.Collections.Concurrent;
using Booking.Application.Contracts.Commands;
using Booking.Application.UseCases;
using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bookings.UnitTest.Application.CommandHandlers;
using Bookings = Booking.Domain.Booking;

public class CreateBookingCommandHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет создание брони.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();
        var command = new CreateBookingCommand(EventId);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        BookingRepositoryMock.Verify(repo => repo.Add(It.Is<Bookings>(x => x.EventId == EventId)),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNull();
        result.EventId.Should().Be(EventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Проверяет создание брони при достижении лимита пользователем.
    /// </summary>
    [Fact]
    public async Task Handle_WhenBookingLimitReached_ShouldThrowBookingLimitReachingException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        BookingRepositoryMock.Setup(repo => repo.CountPendingByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();

        //Act
        Func<Task> act = () => handler.Handle(new CreateBookingCommand(EventId), CancellationToken.None);
        await act.Should().ThrowAsync<BookingLimitReachingException>()
            .WithMessage("Достигнут лимит [10] бронирования.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<Bookings>()),
            Times.Never);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
    /// </summary>
    [Fact]
    public async Task Handle_WhenMultipleBookingsForOneEvent_ShouldWorkCorrectly()
    {
        // Arrange
        var successCount = 0;
        var exceptionsCount = 0;
        var bookingIdsList = new ConcurrentBag<Guid>();

        //Act
        var tasks = Enumerable.Range(0, EventTotalSeats)
            .Select(async _ =>
            {
                try
                {
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var bookingId = Guid.NewGuid();
                        bookingIdsList.Add(bookingId);

                        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();
                        await handler.Handle(new CreateBookingCommand(EventId), CancellationToken.None);

                        Interlocked.Increment(ref successCount);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref exceptionsCount);
                }
            }).ToArray();

        await Task.WhenAll(tasks);

        //Assert
        successCount.Should().Be(EventTotalSeats);
        exceptionsCount.Should().Be(0);
        bookingIdsList.Distinct().Should().HaveCount(successCount);
    }
}