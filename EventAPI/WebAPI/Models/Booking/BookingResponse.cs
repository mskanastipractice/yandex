using Application.Contracts.DTOs;

namespace WebAPI.Models.Booking;

/// <summary>
/// Представляет данные бронирования.
/// </summary>
/// <param name="BookingId">Идентификатор брони.</param>
/// <param name="EventId">Идентификатор события.</param>
/// <param name="Status">Статус.</param>
public record BookingResponse(Guid BookingId, Guid EventId, BookingStatus Status)
{
    public static BookingResponse ToResponse(BookingDto dto)
    {
        return new BookingResponse(dto.BookingId, dto.EventId, MapStatus(dto.Status));
    }
    
    private static BookingStatus MapStatus(Domain.Enums.BookingStatus value)
    {
        return value switch
        {
            Domain.Enums.BookingStatus.Pending => BookingStatus.Pending,
            Domain.Enums.BookingStatus.Confirmed => BookingStatus.Confirmed,
            Domain.Enums.BookingStatus.Rejected => BookingStatus.Rejected,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}