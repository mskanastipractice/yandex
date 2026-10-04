using Domain.Enums;
using Domain.Exceptions;
using static Domain.Enums.BookingStatus;

namespace Domain.Entities;

public class Booking
{
	private Booking()
	{
	}
	
	private Booking(Guid id, Guid eventId, Guid userId, DateTime createdAt)
	{
		Id = id;
		EventId = eventId;
		UserId = userId;
		CreatedAt = createdAt;
		Status = Pending;
	}

	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public Guid UserId { get; private set; }
	
	public User User { get; private set; } = null!;
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? ProcessedAt { get; private set; }
	
	public Event Event { get; private set; } = null!;

	public static Booking Create(Guid eventId, Guid userId)
	{
		return new Booking(Guid.NewGuid(), eventId, userId, DateTime.UtcNow);
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
	
	public void Cancel(DateTime processedAt)
	{
		if (Status != Pending)
		{
			throw new BookingMustBeInPendingStatusException("Нельзя изменить бронирование. Бронирование отменено.");
		}

		Status = Cancelled;
		ProcessedAt = processedAt;
	}
}