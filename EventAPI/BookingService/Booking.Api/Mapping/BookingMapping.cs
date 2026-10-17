using Bookings.Api.Contracts.Bookings;
using BookingStatus = Bookings.Api.Contracts.Bookings.BookingStatus;

namespace Booking.Api.Mapping;

internal static class BookingMapping
{
    internal static BookingResponse ToResponse(this Domain.Booking dto)
    {
        return new BookingResponse(dto.Id, dto.EventId, MapStatus(dto.Status));
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