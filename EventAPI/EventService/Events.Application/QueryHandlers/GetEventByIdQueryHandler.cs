using Events.Application.Contracts.Queries;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Settings;
using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers;

public class GetEventByIdQueryHandler(IEventRepository eventRepository, IEventCache cache, EventCacheSettings settings) : IRequestHandler<GetEventByIdQuery, Event>
{
    public async Task<Event> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var key = $"event:{query.EventId}";
        var cached = await cache.GetAsync<EventSnapshot>(key, cancellationToken);
        if (cached is not null) return cached.ToEvent();

        var @event = await eventRepository.FindAsync(query.EventId, cancellationToken)
                     ?? throw new EntityNotFoundException("Событие", query.EventId);
        await cache.SetAsync(key, EventSnapshot.From(@event), TimeSpan.FromSeconds(settings.EventTtlSeconds), cancellationToken);
        return @event;
    }
}