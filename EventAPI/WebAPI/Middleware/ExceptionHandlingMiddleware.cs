using System.Diagnostics;
using Application.Exceptions.Exceptions;
using Domain.Exceptions;
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
		var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
		context.Response.ContentType = "application/json";

		int statusCode;
		var message = exception.Message;
		
		logger.LogError(
			exception, 
			"Ошибка при обработке запроса {Method} {Path}",
			context.Request.Method, context.Request.Path);
		
		switch (exception)
		{
			case AccessDeniedException:
				statusCode = StatusCodes.Status403Forbidden;
				break;
			
			case EntityNotFoundException:
				statusCode = StatusCodes.Status404NotFound;
				message = exception.Message;
				break;
			
			case UserAlreadyExistsException:
			case NoAvailableSeatsException:
			case BookingLimitReachingException:
				statusCode = StatusCodes.Status409Conflict;
				break;

			case ArgumentException:
			case BookingMustBeInPendingStatusException:
			case PastEventBookingException:
			case PastEventCancellationException:
				statusCode = StatusCodes.Status400BadRequest;
				break;

			default:
				statusCode = StatusCodes.Status500InternalServerError;
				message = "Internal server error";
				logger.LogError(exception, "Ошибка при обработке запроса {Method} {Path}. TraceId: {TraceId}",
					context.Request.Method, context.Request.Path, traceId);
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