using Common.Contracts;

namespace Booking.Application.Interfaces;

public interface IKafkaPublisher
{
    Task PublishBookingConfirmedAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken);
}
