using Application.Contracts;
using Domain.Entities;
using Domain.Entities.ValueObjects;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

[Collection("Database collection")]
public class EventRepositoryTests(DbFixture fixture) : BaseRepositoryTest(fixture)
{
    private static readonly CancellationToken CancellationToken = default;

    /// <summary>
    /// Добавляет событие в базу данных.
    /// </summary>
    private async Task SeedEventAsync(Event? @event = null)
    {
        await using var context = Fixture.CreateContext();

        context.Events.Add(@event ?? CreateEvent());

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Находит событие по идентификатору.
    /// </summary>
    private async Task<Event?> FindEventAsync(Guid id)
    {
        await using var context = Fixture.CreateContext();

        var repository = new EventRepository(context);

        return await repository.FindAsync(id, CancellationToken);
    }

    /// <summary>
    /// Получает отфильтрованный список событий с учетом пагинации.
    /// </summary>
    /// <param name="filters">Фильтры выборки.</param>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Количество элементов на странице.</param>
    /// <returns>Отфильтрованный результат.</returns>
    private async Task<FilteredResult<Event>> GetFilteredAsync(
        Filters? filters = null,
        int page = 1,
        int pageSize = 10)
    {
        await using var context = Fixture.CreateContext();

        var repository = new EventRepository(context);

        return await repository.GetFiltered(
            page,
            pageSize,
            filters ?? new Filters(),
            CancellationToken);
    }

    /// <summary>
    /// Проверяет поиск существующего события по идентификатору.
    /// </summary>
    [Fact]
    public async Task Find_WhenEventExists_ShouldReturnEvent()
    {
        // Arrange
        await SeedEventAsync();

        // Act
        var result = await FindEventAsync(EventId);

        // Assert
        result.Should().NotBeNull();

        result.Should().Match<Event>(x =>
            x.Id == EventId &&
            x.Title == TestData.Title &&
            x.Description == TestData.Description &&
            x.Period.StartAt == TestData.StartAt &&
            x.Period.EndAt == TestData.EndAt &&
            x.TotalSeats == TestData.TotalSeats &&
            x.AvailableSeats == TestData.TotalSeats);
    }

    /// <summary>
    /// Проверяет, что поиск несуществующего события возвращает <c>null</c>.
    /// </summary>
    [Fact]
    public async Task Find_WhenEventDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await FindEventAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет сохранение нового события в базе данных.
    /// </summary>
    [Fact]
    public async Task Add_WhenValidData_ShouldSaveEvent()
    {
        // Arrange
        await SeedEventAsync();

        // Act
        var result = await FindEventAsync(EventId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(EventId);
    }

    /// <summary>
    /// Проверяет изменение существующего события и сохранение обновленных данных.
    /// </summary>
    [Fact]
    public async Task Update_WhenValidData_ShouldSaveChanges()
    {
        // Arrange
        await SeedEventAsync();

        await using (var context = Fixture.CreateContext())
        {
            var repository = new EventRepository(context);

            var @event = await repository.FindAsync(
                EventId,
                CancellationToken);

            @event.Should().NotBeNull();

            @event!.Update(
                TestData.UpdatedTitle,
                TestData.UpdatedDescription,
                EventPeriod.Create(
                    TestData.UpdatedStartAt,
                    TestData.UpdatedEndAt));

            await repository.SaveChangesAsync(CancellationToken);
        }

        // Act
        var result = await FindEventAsync(EventId);

        // Assert
        result.Should().NotBeNull();

        result.Should().Match<Event>(x =>
            x.Title == TestData.UpdatedTitle &&
            x.Description == TestData.UpdatedDescription &&
            x.Period.StartAt == TestData.UpdatedStartAt &&
            x.Period.EndAt == TestData.UpdatedEndAt &&
            x.TotalSeats == TestData.TotalSeats &&
            x.AvailableSeats == TestData.TotalSeats);
    }

    /// <summary>
    /// Проверяет удаление существующего события из базы данных.
    /// </summary>
    [Fact]
    public async Task Delete_WhenValidData_ShouldRemoveEvent()
    {
        // Arrange
        await SeedEventAsync();

        // Act
        await using (var context = Fixture.CreateContext())
        {
            var repository = new EventRepository(context);

            var @event = await repository.FindAsync(
                EventId,
                CancellationToken);

            @event.Should().NotBeNull();

            await repository.RemoveAsync(
                @event!,
                CancellationToken);

            await repository.SaveChangesAsync(CancellationToken);
        }

        // Assert
        var result = await FindEventAsync(EventId);

        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет, что удаление события, для которого существует бронирование,
    /// завершается исключением <see cref="DbUpdateException"/>.
    /// </summary>
    [Fact]
    public async Task Delete_WhenBookingExists_ShouldThrowDbUpdateException()
    {
        // Arrange
        await using (var context = Fixture.CreateContext())
        {
            context.Events.Add(CreateEvent());
            context.Bookings.Add(Booking.Create(EventId));

            await context.SaveChangesAsync();
        }

        // Act
        await using var deleteContext = Fixture.CreateContext();

        var repository = new EventRepository(deleteContext);

        var @event = await repository.FindAsync(
            EventId,
            CancellationToken);

        @event.Should().NotBeNull();

        await repository.RemoveAsync(
            @event!,
            CancellationToken);

        // Assert
        Func<Task> act = () =>
            repository.SaveChangesAsync(CancellationToken);

        await act.Should()
            .ThrowAsync<DbUpdateException>();
    }

    /// <summary>
    /// Проверяет, что для существующего события метод возвращает <c>true</c>.
    /// </summary>
    [Fact]
    public async Task Exists_WhenEventExists_ShouldReturnTrue()
    {
        // Arrange
        await SeedEventAsync();

        // Act
        await using var context = Fixture.CreateContext();

        var repository = new EventRepository(context);

        var result = await repository.Exists(
            EventId,
            CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Проверяет, что для несуществующего события метод возвращает <c>false</c>.
    /// </summary>
    [Fact]
    public async Task Exists_WhenEventDoesNotExist_ShouldReturnFalse()
    {
        // Act
        await using var context = Fixture.CreateContext();

        var repository = new EventRepository(context);

        var result = await repository.Exists(
            Guid.NewGuid(),
            CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет фильтрацию событий по названию без учета регистра.
    /// </summary>
    [Theory]
    [InlineData("Городской")]
    [InlineData("городской")]
    [InlineData("ФЕСТИВАЛЬ")]
    [InlineData("фестив")]
    public async Task GetFiltered_ByTitle_ShouldReturnMatchingEvents(
        string title)
    {
        // Arrange
        await AddTestEventsAsync();

        // Act
        var result = await GetFilteredAsync(
            new Filters(Title: title));

        // Assert
        result.TotalItems.Should().Be(1);

        result.Data
            .Should()
            .ContainSingle()
            .Which.Title
            .Should()
            .Be("Городской фестиваль");
    }

    /// <summary>
    /// Проверяет фильтрацию событий по дате начала.
    /// </summary>
    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 4)]
    [InlineData(5, 3)]
    [InlineData(8, 2)]
    [InlineData(10, 1)]
    public async Task GetFiltered_ByStartDate_ShouldReturnMatchingEvents(
        int daysToAdd,
        int expectedCount)
    {
        // Arrange
        await AddTestEventsAsync();

        var from = BaseDate.AddDays(daysToAdd);

        // Act
        var result = await GetFilteredAsync(
            new Filters(From: from));

        // Assert
        result.TotalItems.Should().Be(expectedCount);

        result.Data.Should()
            .OnlyContain(x => x.Period.StartAt >= from);
    }

    /// <summary>
    /// Проверяет фильтрацию событий по дате окончания.
    /// </summary>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 1)]
    [InlineData(5, 2)]
    [InlineData(8, 3)]
    [InlineData(10, 4)]
    [InlineData(11, 5)]
    public async Task GetFiltered_ByEndDate_ShouldReturnMatchingEvents(
        int daysToAdd,
        int expectedCount)
    {
        // Arrange
        await AddTestEventsAsync();

        var to = BaseDate.AddDays(daysToAdd);

        // Act
        var result = await GetFilteredAsync(
            new Filters(To: to));

        // Assert
        result.TotalItems.Should().Be(expectedCount);

        result.Data.Should()
            .OnlyContain(x => x.Period.EndAt <= to);
    }

    /// <summary>
    /// Проверяет одновременное применение фильтров по названию,
    /// дате начала и дате окончания.
    /// </summary>
    [Theory]
    [InlineData("День", -1, 9, 1)]
    [InlineData("День", 8, 9, 1)]
    [InlineData("Книж", -1, 3, 1)]
    [InlineData("Вечер", -1, 10, 0)]
    [InlineData("Вечер", -1, 11, 1)]
    public async Task GetFiltered_ByCombinedFilters_ShouldReturnMatchingEvents(
        string title,
        int fromDays,
        int toDays,
        int expectedCount)
    {
        // Arrange
        await AddTestEventsAsync();

        var filters = new Filters(
            Title: title,
            From: BaseDate.AddDays(fromDays),
            To: BaseDate.AddDays(toDays));

        // Act
        var result = await GetFilteredAsync(filters);

        // Assert
        result.TotalItems.Should().Be(expectedCount);
        result.Data.Should().HaveCount(expectedCount);
    }

    /// <summary>
    /// Проверяет корректность постраничного получения событий.
    /// </summary>
    [Theory]
    [InlineData(1, 3, 3)]
    [InlineData(2, 3, 2)]
    [InlineData(3, 3, 0)]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 2)]
    [InlineData(3, 2, 1)]
    public async Task GetFiltered_WithPagination_ShouldReturnCorrectPage(
        int page,
        int pageSize,
        int expectedCount)
    {
        // Arrange
        await AddTestEventsAsync();

        // Act
        var result = await GetFilteredAsync(
            page: page,
            pageSize: pageSize);

        // Assert
        result.TotalItems.Should().Be(5);
        result.Data.Should().HaveCount(expectedCount);
    }

    /// <summary>
    /// Проверяет, что при отсутствии подходящих событий возвращается пустой результат.
    /// </summary>
    [Fact]
    public async Task GetFiltered_WhenNothingMatches_ShouldReturnEmptyResult()
    {
        // Arrange
        await AddTestEventsAsync();

        // Act
        var result = await GetFilteredAsync(
            new Filters(Title: "март"));

        // Assert
        result.TotalItems.Should().Be(0);
        result.Data.Should().BeEmpty();
    }
}