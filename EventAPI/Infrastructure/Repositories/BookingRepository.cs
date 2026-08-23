using Application.Contracts;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DataAccess;

namespace Infrastructure.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    public ValueTask<Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken) 
        => context.Bookings.FindAsync([bookingId], cancellationToken);
	
    public Task<Booking[]> GetPendingAsync(CancellationToken cancellationToken) 
    {
        var result = context.Bookings.Where(b => b.Status == BookingStatus.Pending).ToArray();
        return Task.FromResult(result);
    }
    
    public Task AddAsync(Booking booking)
    {
        context.Bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}