using System.Text.Json.Serialization;

namespace WebAPI.Models.Booking;

/// <summary>
/// Представляет статусы бронирования.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
	/// <summary>
	/// Создана, ожидает обработки.
	/// </summary>
	Pending,

	/// <summary>
	/// Подтверждена.
	/// </summary>
	Confirmed,

	/// <summary>
	/// Отклонена.
	/// </summary>
	Rejected
}