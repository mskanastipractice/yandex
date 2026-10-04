using Application.Contracts;
using Application.Contracts.DTOs;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public class BookingService(IBookingRepository repository, IEventService eventService) : IBookingService
{
	private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);
	
	public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.FindAsync(bookingId, cancellationToken);
		if (booking == null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		return BookingDto.ToDto(booking);
	}
	
	public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
	{
		Booking booking;
		
		await AdditionSemaphore.WaitAsync(cancellationToken);
		try
		{
			var seatsExist = await @eventService.TryReserveSeatsAsync(eventId, cancellationToken);

			if (!seatsExist)
			{
				throw new NoAvailableSeatsException(eventId);
			}
			
			booking = Booking.Create(eventId);
			await repository.AddAsync(booking);
			await repository.SaveChangesAsync(cancellationToken);
		}
		finally
		{
			AdditionSemaphore.Release();
		}
		
		return BookingDto.ToDto(booking);
	}

	public async Task ConfirmAsync(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.FindAsync(bookingId, cancellationToken);
		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Confirm(DateTime.UtcNow);
		await repository.SaveChangesAsync(cancellationToken);
	}

	public async Task RejectAsync(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.FindAsync(bookingId, cancellationToken);
		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		booking.Reject(DateTime.UtcNow);
		await repository.SaveChangesAsync(cancellationToken);
	}
}