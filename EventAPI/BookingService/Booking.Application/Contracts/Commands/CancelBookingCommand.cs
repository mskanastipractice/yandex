using MediatR;

namespace Booking.Application.Contracts.Commands;

public record CancelBookingCommand(Guid BookingId) : IRequest;