using Application.Contracts;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

internal class BookingBackgroundService(
	IServiceScopeFactory scopeFactory, 
	ILogger<BookingBackgroundService> logger)
	: BackgroundService
{
	private readonly TimeSpan _delayTimeSpan = TimeSpan.FromSeconds(2);
	private readonly TimeSpan _processBookingDelayTimeSpan = TimeSpan.FromSeconds(10);
	
	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Фоновая обработка бронирований запущена.");

		while (!cancellationToken.IsCancellationRequested)
		{
			using var scope = scopeFactory.CreateScope();
			var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
			var pendingBookings = await bookingStore.GetPendingAsync(cancellationToken);
			var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, cancellationToken));
			await Task.WhenAll(tasks);

			await Task.Delay(_delayTimeSpan, cancellationToken);
		}

		logger.LogInformation("Фоновая обработка бронирований остановлена.");
	}
	
	private async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
	{
		using var scope = scopeFactory.CreateScope();
		var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
		var eventStore = scope.ServiceProvider.GetRequiredService<IEventRepository>();
		var booking = await bookingStore.FindAsync(bookingId, cancellationToken);
		try
		{
			var @event = await eventStore.FindAsync(booking!.EventId, cancellationToken);

			if (@event is null)
			{
				booking.Reject(DateTime.UtcNow);
				logger.LogWarning(
					"Бронирование с идентификатором {BookingId} отклонено. Не найдено событие с идентификатором {EventId}.",
					booking.Id, booking.EventId);
			}
			else
			{
				await Task.Delay(_processBookingDelayTimeSpan, cancellationToken);
				booking.Confirm(DateTime.UtcNow);
				await bookingStore.SaveChangesAsync(cancellationToken);
				logger.LogInformation("Бронирование с идентификатором {BookingId} обработано успешно.", booking.Id);
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			logger.LogWarning("Обработка бронирования с идентификатором {BookingId} отменена.", booking.Id);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при обработке бронирования.");
			await RejectAsync(booking, eventStore, bookingStore, cancellationToken);
		}
	}

	private async Task RejectAsync(Booking booking, IEventRepository eventStore, IBookingRepository bookingStore, CancellationToken cancellationToken)
	{
		booking.Reject(DateTime.UtcNow);
		var @event = await eventStore.FindAsync(booking.EventId, cancellationToken);
		@event?.ReleaseSeats(); 
		await bookingStore.SaveChangesAsync(cancellationToken);
	}
}