using Booking.Application.Contracts.Commands;
using Booking.Application.Exceptions;
using Booking.Application.Interfaces;
using MediatR;

namespace Booking.Application.UseCases;

public class CancelBookingCommandHandler(TimeProvider timeProvider, ICurrentUserContext userContext, IBookingRepository bookingRepository) : IRequestHandler<CancelBookingCommand>
{
    public async Task Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindAsync(command.BookingId, cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException("Бронь", command.BookingId);
        }

        if (!userContext.IsAdmin && booking.UserId != userContext.UserId)
        {
            throw new AccessDeniedException("Недостаточно прав.");
        }

        booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);

        await bookingRepository.SaveChangesAsync(cancellationToken);
    }
}