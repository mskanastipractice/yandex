using Events.Application.Contracts.Queries;
using Events.Application.Interfaces;
using Events.Application.QueryHandlers;
using Events.Application.UseCases;
using Events.Application.Contracts.Commands;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Events.UnitTest.Application.QueryHandlers;

public class CacheTests : BaseUnitTest
{
    [Fact]
    public async Task EventCacheHit_DoesNotReadRepository()
    {
        var snapshot = new EventSnapshot(EventId, EventTitle, EventDescription, EventStartAt, EventEndAt, 7, 4);
        EventCacheMock.Setup(x => x.GetAsync<EventSnapshot>($"event:{EventId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);
        var handler = ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();
        var result = await handler.Handle(new GetEventByIdQuery(EventId), CancellationToken.None);
        result.AvailableSeats.Should().Be(4);
        EventRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EventCacheMiss_ReadsAndCaches()
    {
        var handler = ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();
        await handler.Handle(new GetEventByIdQuery(EventId), CancellationToken.None);
        EventRepositoryMock.Verify(x => x.FindAsync(EventId, It.IsAny<CancellationToken>()), Times.Once);
        EventCacheMock.Verify(x => x.SetAsync($"event:{EventId}", It.IsAny<EventSnapshot>(),
            TimeSpan.FromSeconds(300), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TopCacheHit_DoesNotReadRepository()
    {
        EventCacheMock.Setup(x => x.GetAsync<EventSnapshot[]>("events:top10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new EventSnapshot(EventId, EventTitle, null, EventStartAt, EventEndAt, 7, 3) });
        var result = await ServiceProvider.GetRequiredService<GetTopEventsQueryHandler>()
            .Handle(new GetTopEventsQuery(), CancellationToken.None);
        result.Should().HaveCount(1);
        EventRepositoryMock.Verify(x => x.GetTopAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TopCacheMiss_ReadsAndCaches()
    {
        // Arrange
        EventCacheMock.Setup(x => x.GetAsync<EventSnapshot[]>("events:top10", It.IsAny<CancellationToken>())).ReturnsAsync((EventSnapshot[]?)null);
        EventRepositoryMock.Setup(x => x.GetTopAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Events.Domain.Event>());

        // Act
        await ServiceProvider.GetRequiredService<GetTopEventsQueryHandler>().Handle(new GetTopEventsQuery(), CancellationToken.None);

        // Assert
        EventRepositoryMock.Verify(x => x.GetTopAsync(It.IsAny<CancellationToken>()), Times.Once);
        EventCacheMock.Verify(x => x.SetAsync("events:top10", It.IsAny<EventSnapshot[]>(), TimeSpan.FromSeconds(60), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidatesOnlyAfterSave()
    {
        var saved = false;
        EventRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saved = true).Returns(Task.CompletedTask);
        EventCacheMock.Setup(x => x.RemoveAsync($"event:{EventId}", It.IsAny<CancellationToken>()))
            .Callback(() => saved.Should().BeTrue()).Returns(Task.CompletedTask);
        await ServiceProvider.GetRequiredService<UpdateEventCommandHandler>()
            .Handle(new UpdateEventCommand(EventId, "Updated", null, EventStartAt, EventEndAt, 7), CancellationToken.None);
        EventCacheMock.Verify(x => x.RemoveAsync($"event:{EventId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_InvalidatesEvent()
    {
        await ServiceProvider.GetRequiredService<DeleteEventCommandHandler>()
            .Handle(new DeleteEventCommand(EventId), CancellationToken.None);
        EventCacheMock.Verify(x => x.RemoveAsync($"event:{EventId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
