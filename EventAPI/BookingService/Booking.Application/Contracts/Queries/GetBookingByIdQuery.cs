using MediatR;

namespace Booking.Application.Contracts.Queries;

public record GetBookingByIdQuery(Guid BookingId) : IRequest<Domain.Booking>;