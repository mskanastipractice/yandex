using Domain.Entities;

namespace Application.Contracts.DTOs;

public record EventDto(string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats){
    public static EventDto ToDto(Event entity) => new(
        entity.Title,
        entity.Description,
        entity.Period.StartAt,
        entity.Period.EndAt,
        entity.TotalSeats
    );
}