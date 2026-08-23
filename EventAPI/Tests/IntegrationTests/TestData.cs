namespace IntegrationTests;

internal static class TestData
{
    public static string Title => "Вечер живой музыки";
    public static string Description => "Концерт местных музыкальных коллективов в городском культурном центре.";
    public static DateTime StartAt => new(2026, 03, 15, 18, 00, 00, DateTimeKind.Utc);
    public static DateTime EndAt => new(2026, 03, 15, 21, 00, 00, DateTimeKind.Utc);
    public static int TotalSeats => 50;

    public static string UpdatedTitle => "Фестиваль короткометражного кино";
    public static string UpdatedDescription => "Показ лучших короткометражных фильмов молодых режиссёров.";
    public static DateTime UpdatedStartAt => new(2026, 04, 20, 17, 30, 00, DateTimeKind.Utc);
    public static DateTime UpdatedEndAt => new(2026, 04, 20, 22, 00, 00, DateTimeKind.Utc);
    public static int UpdatedTotalSeats => 80;
}