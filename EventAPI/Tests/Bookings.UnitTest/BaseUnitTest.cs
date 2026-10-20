using Booking.Application.Interfaces;
using Booking.Application.QueryHandlers;
using Booking.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Bookings.UnitTest;

using Bookings = Booking.Domain.Booking;

public abstract class BaseUnitTest : IDisposable
{
    protected readonly Guid EventId = Guid.NewGuid();
    protected readonly Guid BookingId = Guid.NewGuid();
    protected readonly Guid UserId = Guid.NewGuid();
    protected const int EventTotalSeats = 7;

    protected readonly Mock<ICurrentUserContext> UserContextMock = new();
    protected readonly Mock<IBookingRepository> BookingRepositoryMock = new();

    protected readonly IServiceProvider ServiceProvider;

    protected BaseUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(new FakeTimeProvider());

        ConfigureMockServices(services);

        services.AddScoped<GetBookingByIdQueryHandler>();

        services.AddScoped<CreateBookingCommandHandler>();
        services.AddScoped<CancelBookingCommandHandler>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        var booking = Bookings.Create(EventId, UserId);
        UserContextMock.Setup(x => x.UserId).Returns(UserId);
        UserContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        BookingRepositoryMock.Setup(repo => repo.FindAsync(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        BookingRepositoryMock.Setup(repo => repo.GetPendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([Guid.NewGuid(), Guid.NewGuid()]);
        BookingRepositoryMock.Setup(repo => repo.CountPendingByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        BookingRepositoryMock.Setup(repo => repo.Add(booking));
        BookingRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        services.AddSingleton(UserContextMock.Object);
        services.AddSingleton(BookingRepositoryMock.Object);
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}