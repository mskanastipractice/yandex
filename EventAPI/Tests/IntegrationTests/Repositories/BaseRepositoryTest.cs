using Domain.Entities;
using Domain.Entities.ValueObjects;
using Xunit;

namespace IntegrationTests.Repositories;

public abstract class BaseRepositoryTest(DbFixture fixture) : IAsyncLifetime
{
    protected readonly DbFixture Fixture = fixture;

    protected readonly DateTime BaseDate = new(2024, 06, 10, 11, 00, 00, DateTimeKind.Utc);

    protected readonly Guid EventId = Guid.NewGuid();

    protected Event CreateEvent()
    {
        var period = EventPeriod.Create(
            TestData.StartAt,
            TestData.EndAt);

        return Event.Create(
            EventId,
            TestData.Title,
            TestData.Description,
            period,
            TestData.TotalSeats);
    }

    protected async Task AddTestEventsAsync()
    {
        await using var context = Fixture.CreateContext();

        var events = new[]
        {
            Event.Create(
                Guid.NewGuid(),
                "Городской фестиваль",
                "Музыкальная программа и выступления местных артистов",
                EventPeriod.Create(BaseDate, BaseDate.AddHours(5)),
                30),

            Event.Create(
                Guid.NewGuid(),
                "Книжная выставка",
                "Выставка современной и классической литературы",
                EventPeriod.Create(BaseDate.AddDays(2), BaseDate.AddDays(2).AddHours(6)),
                45),

            Event.Create(
                Guid.NewGuid(),
                "Кулинарный мастер-класс",
                "Практическое занятие по приготовлению традиционных блюд",
                EventPeriod.Create(
                    BaseDate.AddDays(5),
                    BaseDate.AddDays(5).AddHours(3)),
                12),

            Event.Create(
                Guid.NewGuid(),
                "День cпорта",
                "Любительские соревнования среди городских команд",
                EventPeriod.Create(
                    BaseDate.AddDays(8),
                    BaseDate.AddDays(8).AddHours(7)),
                60),

            Event.Create(
                Guid.NewGuid(),
                "Вечер настольных игр",
                "Свободная встреча для любителей настольных игр",
                EventPeriod.Create(
                    BaseDate.AddDays(10),
                    BaseDate.AddDays(10).AddHours(4)),
                20)
        };

        context.Events.AddRange(events);
        await context.SaveChangesAsync();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Fixture.ClearTablesAsync();
}