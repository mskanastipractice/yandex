using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Contracts.Infrastructure;
using Application.Exceptions;
using Domain.Entities;

namespace Application.Services;

public class BookingService(IBookingTaskQueue taskQueue, IEventService eventService) : IBookingService
{
	private readonly List<Booking> _bookings = [];
	
	public Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
	{
		var booking = _bookings.Find(b => b.Id == bookingId);
		if (booking == null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		return Task.FromResult(BookingDto.ToDto(booking));
	}
	
	public Task<BookingDto> CreateBookingAsync(Guid eventId)
	{
		var @event = eventService.GetById(eventId);
		var booking = Booking.Create(@event.Id);
		_bookings.Add(booking);

		taskQueue.Enqueue(new BookingTask(booking.Id));

		return Task.FromResult(BookingDto.ToDto(booking));
	}

	public void Confirm(Guid bookingId)
	{
		var booking = _bookings.Find(b => b.Id == bookingId);
		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Confirm(DateTime.UtcNow);
	}

	public void Reject(Guid bookingId)
	{
		var booking = _bookings.Find(b => b.Id == bookingId);
		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Reject(DateTime.UtcNow);
	}
}