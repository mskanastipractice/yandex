using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Domain.Entities;

namespace Application.Services;

public class BookingService(IBookingRepository repository, IEventService eventService) : IBookingService
{
	private readonly Lock _bookingLock = new();
	
	public Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
	{
		var booking = repository.Find(bookingId);
		if (booking == null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		return Task.FromResult(BookingDto.ToDto(booking));
	}
	
	public Task<BookingDto> CreateBookingAsync(Guid eventId)
	{
		Booking booking;
		lock (_bookingLock)
		{
			var seatsExist = eventService.TryReserveSeats(eventId);
			
			if (!seatsExist)
			{
				throw new NoAvailableSeatsException(eventId);
			}

			booking = Booking.Create(eventId);
			repository.Add(booking);
		}
		
		return Task.FromResult(BookingDto.ToDto(booking));
	}

	public void Confirm(Guid bookingId)
	{
		var booking = repository.Find(bookingId);
		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Confirm(DateTime.UtcNow);
	}

	public void Reject(Guid bookingId)
	{
		var booking = repository.Find(bookingId);
		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Reject(DateTime.UtcNow);
	}
}