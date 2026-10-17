using Booking.Application.Contracts.Commands;
using Booking.Application.Interfaces;
using Booking.Domain.Exceptions;
using MediatR;

namespace Booking.Application.UseCases;

public class CreateBookingCommandHandler(ICurrentUserContext userContext, IBookingRepository bookingRepository) : IRequestHandler<CreateBookingCommand, Booking.Domain.Booking>
{
    private const int BookingLimitPerUser = 10;
    private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);

    public async Task<Booking.Domain.Booking> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        Booking.Domain.Booking booking;

        await AdditionSemaphore.WaitAsync(cancellationToken);
        try
        {
            var bookingsCount = await bookingRepository.CountPendingByUserAsync(userContext.UserId, cancellationToken);

            if (bookingsCount >= BookingLimitPerUser)
            {
                throw new BookingLimitReachingException(
                    $"Достигнут лимит [{BookingLimitPerUser}] бронирования.");
            }

            booking = Booking.Domain.Booking.Create(command.EventId, userContext.UserId);
            bookingRepository.Add(booking);
            await bookingRepository.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            AdditionSemaphore.Release();
        }

        return booking;
    }
}