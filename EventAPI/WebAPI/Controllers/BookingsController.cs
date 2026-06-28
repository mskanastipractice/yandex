using Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Booking;

namespace WebAPI.Controllers;

/// <summary>
/// Представляет контроллер для бронирования.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
	/// <summary>
	/// Возвращает бронь по идентификатору.
	/// </summary>
	/// <param name="id">Идентификатор брони.</param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<ActionResult<BookingResponse>> GetById([FromRoute] Guid id)
	{
		var booking = await bookingService.GetBookingByIdAsync(id);
		return Ok(BookingResponse.ToResponse(booking));
	}
}