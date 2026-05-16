using Application.Exceptions;

namespace WebAPI.Middleware;

/// <summary>
/// Глобальная обработка исключений для API.
/// </summary>
internal class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	
	/// <summary>
	/// Инициализирует новый экземпляр middleware.
	/// </summary>
	/// <param name="next">Следующий делегат в конвейере обработки запросов.</param>
	public ExceptionHandlingMiddleware(RequestDelegate next)
	{
		_next = next;
	}
	
	/// <summary>
	/// Выполняет обработку HTTP-запроса с перехватом исключений.
	/// </summary>
	/// <param name="context">Контекст HTTP-запроса.</param>
	/// <returns>Task, представляющий асинхронную операцию.</returns>
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		int statusCode;
		string message;

		switch (exception)
		{
			case EntityNotFoundException:
				statusCode = StatusCodes.Status404NotFound;
				message = exception.Message;
				break;

			case ArgumentException:
				statusCode = StatusCodes.Status400BadRequest;
				message = exception.Message;
				break;

			default:
				statusCode = StatusCodes.Status500InternalServerError;
				message = "Internal server error";
				break;
		}

		context.Response.StatusCode = statusCode;

		return context.Response.WriteAsJsonAsync(new CustomHttpResponse(message));
	}
}