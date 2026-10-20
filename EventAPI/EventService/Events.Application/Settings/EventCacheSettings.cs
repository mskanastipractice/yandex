namespace Events.Application.Settings;

public sealed class EventCacheSettings
{
    public int EventTtlSeconds { get; set; } = 300;
    public int TopTtlSeconds { get; set; } = 60;
}
