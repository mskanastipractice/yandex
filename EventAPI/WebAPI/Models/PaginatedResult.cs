namespace WebAPI.Models;

/// <summary>
/// Представляет результат пагинации.
/// </summary>
/// <param name="Data">Данные.</param>
/// <param name="PageInfo">Дополнительные данные.</param>
public record PaginatedResult<T>(IReadOnlyCollection<T> Data, PageInfo PageInfo);

/// <summary>
/// Представляет дополнительные данные.
/// </summary>
/// <param name="TotalItems">Общее количество элементов.</param>
/// <param name="CurrentPage">Номер текущей страницы.</param>
/// <param name="ItemsPerPage">Количество элементов на странице.</param>
public record PageInfo(int TotalItems, int CurrentPage, int ItemsPerPage);