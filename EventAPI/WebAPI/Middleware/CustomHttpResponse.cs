namespace WebAPI.Middleware;

/// <summary>
/// Представляет стандартный HTTP-ответ с сообщением и дополнительной информацией.
/// </summary>
/// <param name="Message">Текст сообщения.</param>
public record CustomHttpResponse(string Message, int? StatusCode = null, string? Details = null);