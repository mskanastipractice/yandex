using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Entities.ValueObjects;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xunit;

namespace Tests.Application;

public class BookingServiceUnitTests: IDisposable
{
	private readonly Guid _eventId1 = Guid.NewGuid();
	private readonly Guid _eventId2 = Guid.NewGuid();
	private const int TotalSeats = 10;
	
	protected readonly IServiceProvider ServiceProvider;

	public BookingServiceUnitTests()
	{
		var services = new ServiceCollection();
		services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));
		services.AddScoped<IEventRepository, EventRepository>();
		services.AddScoped<IBookingRepository, BookingRepository>();
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		ServiceProvider = services.BuildServiceProvider();

		SeedDatabase();
	}
	
	private void SeedDatabase()
	{
		using var scope = ServiceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var now = DateTime.UtcNow;

		context.Events.AddRange(
			Event.Create(_eventId1, "Новый год", "Праздник наступления Нового Года",
				EventPeriod.Create(now, now.AddDays(7)), 10),
			Event.Create(_eventId2, "Пасха", "Празднование Пасхи",
				EventPeriod.Create(now.AddMonths(-1), now.AddMonths(-1).AddDays(2)), 10));

		context.SaveChanges();
	}

	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var eventId = _eventId1;
		BookingDto returnedResult;
		
		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			returnedResult = await service.CreateBookingAsync(eventId, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			returnedResult.Should().NotBeNull();
			returnedResult.EventId.Should().Be(eventId);
			returnedResult.Status.Should().Be(BookingStatus.Pending);
			
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			var booking = await service.GetBookingByIdAsync(returnedResult.BookingId, CancellationToken.None);
			booking.Should().BeEquivalentTo(returnedResult);
			
			var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await eventService.GetByIdAsync(_eventId1, CancellationToken.None))
				.AvailableSeats.Should().Be(TotalSeats - 1);
		}
	}
	
	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
	/// </summary>
	[Fact]
	public async Task Add_MultipleBookingsForOneEvent_Success()
	{
		//Arrange
		var eventId = _eventId1;
		var bookingList = new BookingDto[TotalSeats];

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			for (int i = 0; i < TotalSeats; i++)
			{
				bookingList[i] = await service.CreateBookingAsync(eventId, CancellationToken.None);
			}
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			var uniqueIds = bookingList.Select(b => b.BookingId).Distinct().ToList();
			CollectionAssert.AllItemsAreUnique(uniqueIds);

			foreach (var bookingId in uniqueIds)
			{
				var result = await service.GetBookingByIdAsync(bookingId, CancellationToken.None);
				result.Should().NotBeNull();
				result.BookingId.Should().Be(bookingId);
				result.EventId.Should().Be(eventId);
				result.Status.Should().Be(BookingStatus.Pending);
			}
		}
	}
	
	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Confirm_ValidData_Success()
	{
		//Arrange
		var eventId = _eventId1;
		BookingDto booking;
		
		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			booking = await service.CreateBookingAsync(eventId, CancellationToken.None);
			await service.ConfirmAsync(booking.BookingId, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			var result = await service.GetBookingByIdAsync(booking.BookingId, CancellationToken.None);
			result.Should().NotBeNull();
			result.BookingId.Should().Be(booking.BookingId);
			result.EventId.Should().Be(eventId);
			result.Status.Should().Be(BookingStatus.Confirmed);
		}
	}
	
	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Reject_ValidData_Success()
	{
		//Arrange
		var eventId = _eventId1;
		BookingDto booking;

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			booking = await service.CreateBookingAsync(eventId, CancellationToken.None);
			await service.RejectAsync(booking.BookingId, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			var result = await service.GetBookingByIdAsync(booking.BookingId, CancellationToken.None);
			result.Should().NotBeNull();
			result.BookingId.Should().Be(booking.BookingId);
			result.EventId.Should().Be(eventId);
			result.Status.Should().Be(BookingStatus.Rejected);
		}
	}
	
	/// <summary>
	/// Проверяет создание брони на недоступное количество мест.
	/// </summary>
	[Fact]
	public async Task Add_NoAvailableSeats_ExceptionThrown()
	{
		//Arrange
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			for (var i = 0; i < TotalSeats; i++)
			{
				await service.CreateBookingAsync(_eventId1, CancellationToken.None);
			}
		}

		//Act
		Func<Task> act;
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			act = () => service.CreateBookingAsync(_eventId1, CancellationToken.None);
		

		//Assert
		await act.Should().ThrowAsync<NoAvailableSeatsException>()
			.WithMessage($"Свободные места на событие с идентификатором [{_eventId1}] не найдены.");
		}
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public async Task Add_ForNonExistentEvent_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();

		//Act
		Func<Task> act;
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			act = () => service.CreateBookingAsync(eventId, CancellationToken.None);
		
		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		}
	}
	
	/// <summary>
	/// Проверяет создание брони для удаленного события.
	/// </summary>
	[Fact]
	public async Task Add_ForDeletedEvent_Failed()
	{
		//Arrange
		var eventId = _eventId1;
		using (var deleteScope = ServiceProvider.CreateScope())
		{
			var eventService = deleteScope.ServiceProvider
				.GetRequiredService<IEventService>();

			var eventToDelete = await eventService.GetByIdAsync(
				eventId,
				CancellationToken.None);

			await eventService.DeleteAsync(
				eventToDelete!.Id,
				CancellationToken.None);
		}

		//Act
		using var scope = ServiceProvider.CreateScope();

		var service = scope.ServiceProvider
			.GetRequiredService<IBookingService>();

		Func<Task> act = () =>
			service.CreateBookingAsync(
				eventId,
				CancellationToken.None);
		

		//Assert
		await act.Should()
			.ThrowAsync<EntityNotFoundException>()
			.WithMessage(
				$"Сущность [Событие] с идентификатором [{eventId}] не найдена.");
	}

	/// <summary>
	/// Проверяет получение несуществующей брони.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentBooking_Failed()
	{
		// Arrange
		var bookingId = Guid.NewGuid();

		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

		// Act
		Func<Task> act = () =>
			service.GetBookingByIdAsync(bookingId, CancellationToken.None);

		// Assert
		await act.Should()
			.ThrowAsync<EntityNotFoundException>()
			.WithMessage(
				$"Сущность [Бронь] с идентификатором [{bookingId}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события при овербукинге.
	/// </summary>
	[Fact]
	public async Task Add_MultipleBookingsForOneEvent_Overbooking_Success()
	{
		// Arrange
		var totalRequests = 25;
		var successCount = 0;
		var exceptionsCount = 0;

		//Act
		var tasks = Enumerable.Range(0, totalRequests)
			.Select(_ => Task.Run(async () =>
			{
				try
				{
					using (var scope = ServiceProvider.CreateScope())
					{
						var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
						await service.CreateBookingAsync(_eventId1,CancellationToken.None);
						Interlocked.Increment(ref successCount);
					}
				}
				catch (NoAvailableSeatsException)
				{
					Interlocked.Increment(ref exceptionsCount);
				}
			})).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
			successCount.Should().Be(TotalSeats);
			exceptionsCount.Should().Be(totalRequests - TotalSeats);
			var @event = await eventRepository.FindAsync(_eventId1,CancellationToken.None);
			@event!.AvailableSeats.Should().Be(0);
		}
	}
	
	public void Dispose()
	{
		if (ServiceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}