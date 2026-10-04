using System.ComponentModel.DataAnnotations;
using Application.Contracts.DTOs;
using Application.Exceptions.Exceptions;
using Domain.Exceptions;

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
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>DTO бронирования.</returns>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);

	/// <summary>
	/// Создаёт новое бронирование.
	/// </summary>
	/// <param name="eventId">Идентификатор события для бронирования.</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>DTO созданного бронирования.</returns>
	/// <exception cref="ValidationException">Выбрасывается, если переданные данные не проходят валидацию.</exception>
	Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken);

	/// <summary>
	/// Подтверждает бронирование по его идентификатору.
	/// </summary>
	/// <param name="bookingId">Идентификатор подтверждаемого бронирования.</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	/// <exception cref="InvalidOperationException">Выбрасывается, если бронирование уже подтверждено или отклонено.</exception>
	Task ConfirmAsync(Guid bookingId, CancellationToken cancellationToken);

	/// <summary>
	/// Отклоняет бронирование по его идентификатору.
	/// </summary>
	/// <param name="bookingId">Идентификатор отклоняемого бронирования.</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	/// <exception cref="InvalidOperationException">Выбрасывается, если бронирование уже подтверждено или отклонено.</exception>
	Task RejectAsync(Guid bookingId, CancellationToken cancellationToken);
	
	/// <summary>
	/// Отменяет бронирование по его идентификатору.
	/// </summary>
	/// <param name="bookingId">Идентификатор отменяемого бронирования.</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <exception cref="EntityNotFoundException">Выбрасывается, если бронирование с указанным ID не найдено.</exception>
	/// <exception cref="AccessDeniedException">Выбрасывается, если недостаточно прав на действие.</exception>
	/// <exception cref="PastEventCancellationException">Выбрасывается при попытке отменить прошедшее событие.</exception>
	Task CancelBookingAsync(Guid bookingId, CancellationToken cancellationToken);
}