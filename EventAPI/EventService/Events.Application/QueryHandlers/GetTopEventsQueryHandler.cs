using Events.Application.Interfaces;
using Events.Application.Settings;
using Events.Domain;
using MediatR;

namespace Events.Application.QueryHandlers;

public sealed class GetTopEventsQueryHandler(IEventRepository repository, IEventCache cache, EventCacheSettings settings)
    : IRequestHandler<GetTopEventsQuery, IReadOnlyList<Event>>
{
    public async Task<IReadOnlyList<Event>> Handle(GetTopEventsQuery request, CancellationToken cancellationToken)
    {
        const string key = "events:top10";
        var cached = await cache.GetAsync<EventSnapshot[]>(key, cancellationToken);
        if (cached is not null) return cached.Select(x => x.ToEvent()).ToArray();

        var events = await repository.GetTopAsync(cancellationToken);
        await cache.SetAsync(key, events.Select(EventSnapshot.From).ToArray(),
            TimeSpan.FromSeconds(settings.TopTtlSeconds), cancellationToken);
        return events;
    }
}
