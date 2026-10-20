using Events.Domain;

namespace Events.IntegrationTests;

internal static class TestData
{
    public static readonly Guid UserId = Guid.NewGuid();

    public static Guid EventId = Guid.NewGuid();
    public const string Event1Title = "Театральное представление на площади";
    public const string Event1Description = "Спектакль осени";
    public static DateTime Event1StartAt = new(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc);
    public static DateTime Event1EndAt = new(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc);
    public const int Event1TotalSeats = 10;

    public const string Event2Title = "Встреча выпускников";
    public const string Event2Description = "Встреча выпускников 1990г.";
    public static DateTime Event2StartAt = new(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc);
    public static DateTime Event2EndAt = new(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc);
    public const int Event2TotalSeats = 30;

    public const string Event3Title = "День города";
    public const string Event3Description = "Большое мероприятие ко дню города.";
    public static DateTime Event3StartAt = new(2026, 02, 02, 03, 20, 00, DateTimeKind.Utc);
    public static DateTime Event3EndAt = new(2026, 02, 02, 22, 30, 00, DateTimeKind.Utc);
    public const int Event3TotalSeats = 50;

    public const string UpdatedTitle = "Детский праздник";
    public const string UpdatedDescription = "Праздник для малышей от 1г до 5ти лет";
    public static DateTime UpdatedStartAt = new(2026, 02, 03, 18, 55, 00, DateTimeKind.Utc);
    public static DateTime UpdatedEndAt = new(2026, 02, 05, 12, 00, 00, DateTimeKind.Utc);
    public const int UpdatedTotalSeats = 15;

    public static Event TestEvent => Event.Create(EventId, Event1Title, Event1Description,
        EventPeriod.Create(Event1StartAt, Event1EndAt), Event1TotalSeats);
}