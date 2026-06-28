using Domain.Entities;

namespace Application.Contracts;

public interface IBookingRepository
{
	Booking? Find(Guid bookingId);

	IReadOnlyCollection<Booking> GetPending();

	void Add(Booking booking);
}