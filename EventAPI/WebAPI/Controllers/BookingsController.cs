using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Booking;

namespace WebAPI.Controllers;

/// <summary>
/// Представляет контроллер для бронирования.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
	/// <summary>
	/// Возвращает бронь по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор брони.</param>
	/// <param name="cancellationToken">Токен отмены</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<ActionResult<BookingResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		var booking = await bookingService.GetBookingByIdAsync(id, cancellationToken);
		return Ok(BookingResponse.ToResponse(booking));
	}
	
	/// <summary>
	/// Удаляет бронь.
	/// </summary>
	/// <param name="id">Идентификатор брони.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<BookingResponse>> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
	{
		await bookingService.CancelBookingAsync(id, cancellationToken);
		return Ok();
	}
}