using Application.Contracts;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = [];
	
    public Booking? Find(Guid bookingId)
    {
        return _bookings.Find(b => b.Id == bookingId);
    }
	
    public IReadOnlyCollection<Booking> GetPending()
    {
        return _bookings.Where(b => b.Status == BookingStatus.Pending).ToArray();
    }

    public void Add(Booking booking)
    {
        _bookings.Add(booking);
    }
}