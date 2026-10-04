using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions.Exceptions;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public class BookingService(
	ICurrentUserContext userContext, 
	TimeProvider timeProvider, 
	IBookingRepository repository, 
	IEventService eventService) : IBookingService
{
	private static readonly SemaphoreSlim AdditionSemaphore = new(1, 1);
	private const int BookingLimitPerUser = 20;
	
	public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.FindAsync(bookingId, cancellationToken);
		if (booking == null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}
		
		if (booking.UserId != userContext.UserId)
		{
			throw new AccessDeniedException("Недостаточно прав.");
		}
		
		return BookingDto.ToDto(booking);
	}
	
	public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
	{
		Booking booking;
		
		await AdditionSemaphore.WaitAsync(cancellationToken);
		try
		{
			var bookingsCount = await repository.CountPendingByUser(userContext.UserId, cancellationToken);

			if (bookingsCount >= BookingLimitPerUser)
			{
				throw new BookingLimitReachingException($"Достигнут лимит [{BookingLimitPerUser}] бронирования у события.");
			}
			
			var @event = await eventService.GetByIdAsync(eventId, cancellationToken);
			
			if (@event.StartAt < timeProvider.GetUtcNow().UtcDateTime)
			{
				throw new PastEventBookingException();
			}
			
			var seatsExist = await eventService.TryReserveSeatsAsync(eventId, cancellationToken);

			if (!seatsExist)
			{
				throw new NoAvailableSeatsException(eventId);
			}
			
			booking = Booking.Create(eventId, userContext.UserId);
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

	public async Task CancelBookingAsync(Guid bookingId, CancellationToken cancellationToken)
	{
		var booking = await repository.FindAsync(bookingId, cancellationToken);

		if (booking is null)
		{
			throw new EntityNotFoundException("Бронь", bookingId);
		}

		var @event = await eventService.GetByIdAsync(booking.EventId, cancellationToken);

		if (@event is null)
		{
			throw new EntityNotFoundException("Событие", booking.EventId);
		}

		if (!userContext.IsAdmin && booking.UserId != userContext.UserId)
		{
			throw new AccessDeniedException("Недостаточно прав.");
		}

		if (@event.StartAt < timeProvider.GetUtcNow().UtcDateTime)
		{
			throw new PastEventCancellationException();
		}

		booking.Cancel(timeProvider.GetUtcNow().UtcDateTime);

		await repository.SaveChangesAsync(cancellationToken);
	}
}