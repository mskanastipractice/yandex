using Domain.Entities;

namespace Application.Contracts.DTOs;

public record EventDto(int Id, string Title, string? Description, DateTime StartAt, DateTime EndAt){
    public static EventDto ToDto(Event entity) => new(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.Period.StartAt,
        entity.Period.EndAt
    );
}