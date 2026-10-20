using Events.Domain;

namespace Events.Application.Interfaces;

public sealed record EventSnapshot(Guid Id, string Title, string? Description, DateTime StartAt,
    DateTime EndAt, int TotalSeats, int AvailableSeats)
{
    public static EventSnapshot From(Event value) => new(value.Id, value.Title, value.Description,
        value.Period.StartAt, value.Period.EndAt, value.TotalSeats, value.AvailableSeats);

    public Event ToEvent()
    {
        var value = Event.Create(Id, Title, Description, EventPeriod.Create(StartAt, EndAt), TotalSeats);
        if (TotalSeats > AvailableSeats) value.TryReserveSeats(TotalSeats - AvailableSeats);
        return value;
    }
}
