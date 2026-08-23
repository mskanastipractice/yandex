using Domain.Entities;

namespace Application.Contracts;

public interface IBookingRepository
{
	ValueTask<Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken);

	Task<Booking[]> GetPendingAsync(CancellationToken cancellationToken);

	Task AddAsync(Booking booking);
	
	Task SaveChangesAsync(CancellationToken cancellationToken);
}