using Domain.Entities;

namespace Application.Contracts.DTOs;

public record EventDto(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats){
    public static EventDto ToDto(Event entity) => new(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.Period.StartAt,
        entity.Period.EndAt,
        entity.TotalSeats
    );
}