using Booking.Application.Contracts.Queries;
using Booking.Application.Exceptions;
using Booking.Application.Interfaces;
using MediatR;

namespace Booking.Application.QueryHandlers;

public class GetBookingByIdQueryHandler(ICurrentUserContext userContext, IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingByIdQuery, Domain.Booking>
{
    public async Task<Domain.Booking> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.FindAsync(query.BookingId, cancellationToken);

        if (booking is null)
        {
            throw new EntityNotFoundException("Бронь", query.BookingId);
        }

        if (booking.UserId != userContext.UserId)
        {
            throw new AccessDeniedException("Недостаточно прав.");
        }

        return booking;
    }
}