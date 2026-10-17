using Booking.Application.Interfaces;
using Booking.Domain.Enums;
using Booking.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext context) : IBookingRepository
{
    public async Task<Domain.Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken) => await context.Bookings.FindAsync([bookingId], cancellationToken: cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetPendingAsync(CancellationToken cancellationToken) =>
        await context.Bookings.Where(b => b.Status == BookingStatus.Pending).Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

    public async Task<int> CountPendingByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await context.Bookings.CountAsync(x => x.Status == BookingStatus.Pending && x.UserId == userId,
            cancellationToken);

    public Guid Add(Domain.Booking booking)
    {
        var data = context.Bookings.Add(booking);
        return data.Entity.Id;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}