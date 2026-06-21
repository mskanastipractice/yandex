using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Middleware;

/// <summary>
/// Глобальная обработка исключений для API.
/// </summary>
internal class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
	/// <summary>
	/// Выполняет обработку HTTP-запроса с перехватом исключений.
	/// </summary>
	/// <param name="context">Контекст HTTP-запроса.</param>
	/// <returns>Task, представляющий асинхронную операцию.</returns>
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		int statusCode;
		string message;
		
		logger.LogError(
			exception, 
			"Ошибка при обработке запроса {Method} {Path}",
			context.Request.Method, context.Request.Path);
		
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
		var responseMessage = new ProblemDetails
		{
			Status = statusCode,
			Title = message,
			Detail = message
		};
		
		return context.Response.WriteAsJsonAsync(responseMessage);
	}
}