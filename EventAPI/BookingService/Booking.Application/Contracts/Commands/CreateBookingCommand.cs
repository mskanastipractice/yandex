using MediatR;

namespace Booking.Application.Contracts.Commands;

public record CreateBookingCommand(Guid EventId) : IRequest<Booking.Domain.Booking>;