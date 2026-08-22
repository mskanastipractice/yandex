using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Entities.ValueObjects;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using NUnit.Framework;
using Xunit;

namespace Tests.Application;

public class BookingServiceUnitTests
{
	private readonly Guid _eventId1 = Guid.NewGuid();
	private readonly Guid _eventId2 = Guid.NewGuid();
	private const int TotalSeats = 10;
	private readonly IEventRepository _eventRepository = new EventRepository();
	private readonly IBookingRepository _bookingRepository = new BookingRepository();
	private readonly BookingService _service;

	public BookingServiceUnitTests()
	{
		var now = DateTime.UtcNow;
		_eventRepository.Add(Event.Create(_eventId1, "Новый год", "Праздник наступления Нового Года",  EventPeriod.Create(now, now.AddDays(7)), 10));
		_eventRepository.Add(Event.Create(_eventId2, "Пасха", "Празднование Пасхи", EventPeriod.Create(now.AddMonths(-1), now.AddMonths(-1).AddDays(2)), 10));
		
		
		var eventService = new EventService(_eventRepository);
		_service = new BookingService(_bookingRepository, eventService);
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
		var booking = await _service.GetBookingByIdAsync(result.BookingId);
		booking.Should().BeEquivalentTo(result);
		_eventRepository.Find(_eventId1)!.AvailableSeats.Should().Be(TotalSeats - 1);
	}
	
	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
	/// </summary>
	[Fact]
	public async Task Add_MultipleBookingsForOneEvent_Success()
	{
		//Arrange
		var eventId = _eventId1;

		//Act
		var bookingList = new BookingDto[TotalSeats];
		for (int i = 0; i < TotalSeats; i++)
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
	/// Проверяет создание брони на недоступное количество мест.
	/// </summary>
	[Fact]
	public async Task Add_NoAvailableSeats_ExceptionThrown()
	{
		//Arrange
		for (var i = 0; i < TotalSeats; i++)
		{
			await _service.CreateBookingAsync(_eventId1);
		}

		//Act
		//Act
		Func<Task> act = () => _service.CreateBookingAsync(_eventId1);

		//Assert
		await act.Should().ThrowAsync<NoAvailableSeatsException>()
			.WithMessage($"Свободные места на событие с идентификатором [{_eventId1}] не найдены.");
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
		var eventToDelete = _eventRepository.Find(eventId);
		_eventRepository.Remove(eventToDelete!);
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
			.Select(_ => Task.Run(() =>
			{
				try
				{
					_service.CreateBookingAsync(_eventId1);
					Interlocked.Increment(ref successCount);
				}
				catch (NoAvailableSeatsException)
				{
					Interlocked.Increment(ref exceptionsCount);
				}
			})).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		successCount.Should().Be(TotalSeats);
		exceptionsCount.Should().Be(totalRequests - TotalSeats);
		_eventRepository.Find(_eventId1)!.AvailableSeats.Should().Be(0);
	}
}