namespace Application.Contracts.DTOs;

public record PaginatedResultDto<T>(int TotalItems, int CurrentPage, int ItemsPerPage, IReadOnlyList<T> Items);