using Events.Domain;

namespace Events.Application.Interfaces;

public interface IEventRepository
{
    Task<FilteredResult<Event>> GetFilteredAsync(int page, int pageSize, Filters? filters, CancellationToken cancellationToken);

    Task<Event?> FindAsync(Guid eventId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Event>> GetTopAsync(CancellationToken cancellationToken);

    void Add(Event @event);

    void Delete(Event @event);

    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}