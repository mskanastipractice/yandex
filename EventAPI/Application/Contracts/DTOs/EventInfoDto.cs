using Domain.Entities;

namespace Application.Contracts.DTOs;

public record EventInfoDto(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats, int AvailableSeats){
    public static EventInfoDto ToDto(Event entity) => new(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.Period.StartAt,
        entity.Period.EndAt,
        entity.TotalSeats,
        entity.AvailableSeats
    );
}