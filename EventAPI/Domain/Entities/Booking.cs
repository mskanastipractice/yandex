using Domain.Enums;
using static Domain.Enums.BookingStatus;

namespace Domain.Entities;

public class Booking
{
	private Booking(Guid id, Guid eventId, DateTime createdAt)
	{
		Id = id;
		EventId = eventId;
		CreatedAt = createdAt;
		Status = Pending;
	}

	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? ProcessedAt { get; private set; }

	public static Booking Create(Guid eventId)
	{
		return new Booking(Guid.NewGuid(), eventId, DateTime.UtcNow);
	}
	
	public void Confirm(DateTime processedAt)
	{
		Status = Confirmed;
		ProcessedAt = processedAt;
	}

	public void Reject(DateTime processedAt)
	{
		Status = Rejected;
		ProcessedAt = processedAt;
	}
}