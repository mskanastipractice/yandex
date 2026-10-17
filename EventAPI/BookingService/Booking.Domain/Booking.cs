using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using static Booking.Domain.Enums.BookingStatus;

namespace Booking.Domain;

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
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? ProcessedAt { get; private set; }

	public static Booking Create(Guid eventId, Guid userId)
	{
		return new Booking(Guid.NewGuid(), eventId, userId, DateTime.UtcNow);
	}
	
	public void Confirm(DateTime processedAt)
	{
		if (Status != Pending)
		{
			throw new BookingMustBeInPendingStatusException("Нельзя изменить бронирование. Бронирование отменено.");
		}
		
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
		
		if (Status == Rejected)
		{
			throw new BookingMustBeInPendingStatusException("Нельзя отменить отклонённое бронирование.");
		}

		Status = Cancelled;
		ProcessedAt = processedAt;
	}
}