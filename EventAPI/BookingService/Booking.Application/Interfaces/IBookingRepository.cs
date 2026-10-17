namespace Booking.Application.Interfaces;

public interface IBookingRepository
{
    Task<Domain.Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetPendingAsync(CancellationToken cancellationToken);

    Task<int> CountPendingByUserAsync(Guid userId, CancellationToken cancellationToken);

    Guid Add(Domain.Booking booking);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}