using Application.Contracts.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Queue;
using NUnit.Framework;
using Xunit;

namespace Tests.Application;

public class BookingServiceUnitTests
{
	private readonly Guid _eventId1 = Guid.NewGuid();
	private readonly Guid _eventId2 = Guid.NewGuid();
	private readonly EventService _eventService = new();
	private readonly BookingService _service;

	public BookingServiceUnitTests()
	{
		_service = new(new InMemoryBookingTaskQueue(), _eventService);
		var now = DateTime.UtcNow;
		_eventService.CreateAsync(new EventDto(_eventId1, "Новый год", "Праздник наступления Нового Года", now, now.AddDays(7), 10));
		_eventService.CreateAsync(new EventDto(_eventId2, "Пасха", "Празднование Пасхи", now.AddMonths(-1), now.AddMonths(-1).AddDays(2), 10));
	}

	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var eventId = _eventId1;

		//Act
		var result = await _service.CreateBookingAsync(eventId);

		//Assert
		result.Should().NotBeNull();
		result.EventId.Should().Be(eventId);
		result.Status.Should().Be(BookingStatus.Pending);
	}
	
	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
	/// </summary>
	[Fact]
	public async Task Add_MultipleBookingsForOneEvent_Success()
	{
		//Arrange
		var eventId = _eventId1;
		const int bookingCount = 3;

		//Act
		var bookingList = new BookingDto[bookingCount];
		for (int i = 0; i < bookingList.Length; i++)
		{
			bookingList[i] = await _service.CreateBookingAsync(eventId);
		}
		
		//Assert
		var uniqueIds = bookingList.Select(b => b.BookingId).Distinct().ToList();
		CollectionAssert.AllItemsAreUnique(uniqueIds);
		
		foreach (var bookingId in uniqueIds)
		{
			var result = await _service.GetBookingByIdAsync(bookingId);
			result.Should().NotBeNull();
			result.BookingId.Should().Be(bookingId);
			result.EventId.Should().Be(eventId);
			result.Status.Should().Be(BookingStatus.Pending);
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
		var booking = await _service.CreateBookingAsync(eventId);

		//Act
		_service.Confirm(booking.BookingId);

		//Assert
		var result = await _service.GetBookingByIdAsync(booking.BookingId);
		result.Should().NotBeNull();
		result.BookingId.Should().Be(booking.BookingId);
		result.EventId.Should().Be(eventId);
		result.Status.Should().Be(BookingStatus.Confirmed);
	}
	
	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Reject_ValidData_Success()
	{
		//Arrange
		var eventId = _eventId1;
		var booking = await _service.CreateBookingAsync(eventId);

		//Act
		_service.Reject(booking.BookingId);

		//Assert
		var result = await _service.GetBookingByIdAsync(booking.BookingId);
		result.Should().NotBeNull();
		result.BookingId.Should().Be(booking.BookingId);
		result.EventId.Should().Be(eventId);
		result.Status.Should().Be(BookingStatus.Rejected);
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
		Func<Task> act = () => _service.CreateBookingAsync(eventId);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет создание брони для удаленного события.
	/// </summary>
	[Fact]
	public async Task Add_ForDeletedEvent_Failed()
	{
		//Arrange
		var eventId = _eventId1;

		//Act
		_eventService.Delete(eventId);
		Func<Task> act = () => _service.CreateBookingAsync(eventId);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет получение несуществующей брони.
	/// </summary>
	[Fact]
	public void GetById_NonExistentBooking_Failed()
	{
		//Arrange
		var bookingId = Guid.NewGuid();
		
		//Act
		Action act = () => _service.GetBookingByIdAsync(bookingId);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{bookingId.ToString()}] не найдена.");
	}
}