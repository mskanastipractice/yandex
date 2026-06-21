using System.ComponentModel.DataAnnotations;
using Application.Contracts.DTOs;
using Application.Exceptions;

namespace Application.Contracts;

/// <summary>
/// Предоставляет интерфейс сервиса для управления бронированиями.
/// </summary>
public interface IBookingService
{
	/// <summary>
	/// Возвращает бронирование по его идентификатору.
	/// </summary>
	/// <param name="bookingId">Идентификатор бронирования.</param>
	/// <returns>DTO бронирования.</returns>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	Task<BookingDto> GetBookingByIdAsync(Guid bookingId);

	/// <summary>
	/// Создаёт новое бронирование.
	/// </summary>
	/// <param name="eventId">Идентификатор события для бронирования.</param>
	/// <returns>DTO созданного бронирования.</returns>
	/// <exception cref="ValidationException">Выбрасывается, если переданные данные не проходят валидацию.</exception>
	Task<BookingDto> CreateBookingAsync(Guid eventId);

	/// <summary>
	/// Подтверждает бронирование по его идентификатору.
	/// </summary>
	/// <param name="bookingId">Идентификатор подтверждаемого бронирования.</param>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	/// <exception cref="InvalidOperationException">Выбрасывается, если бронирование уже подтверждено или отклонено.</exception>
	void Confirm(Guid bookingId);

	/// <summary>
	/// Отклоняет бронирование по его идентификатору.
	/// </summary>
	/// <param name="bookingId">Идентификатор отклоняемого бронирования.</param>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	/// <exception cref="InvalidOperationException">Выбрасывается, если бронирование уже подтверждено или отклонено.</exception>
	void Reject(Guid bookingId);
}