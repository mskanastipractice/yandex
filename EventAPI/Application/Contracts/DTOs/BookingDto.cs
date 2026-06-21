using Domain.Entities;
using Domain.Enums;

namespace Application.Contracts.DTOs;

public record BookingDto(Guid BookingId, Guid EventId, BookingStatus Status){
    public static BookingDto ToDto(Booking entity) => new(
        entity.Id,
        entity.EventId,
        entity.Status
    );
}