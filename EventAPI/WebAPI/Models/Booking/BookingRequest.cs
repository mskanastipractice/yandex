using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Booking;

/// <summary>
/// Представляет данные для создания брони.
/// </summary>
/// <param name="EventId">Идентификатор события.</param>
public record BookingRequest(
    [Required(ErrorMessage = "Идентификатор события для брони.")]
    Guid EventId);