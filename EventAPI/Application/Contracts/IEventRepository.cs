using Domain.Entities;

namespace Application.Contracts;

public interface IEventRepository
{
    Task<FilteredResult<Event>> GetFiltered(int page, int pageSize, Filters filters,
        CancellationToken cancellationToken);
    
    ValueTask<Event?> FindAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddAsync(Event @event);

    Task RemoveAsync(Event @event, CancellationToken cancellationToken);

    Task<bool> Exists(Guid eventId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}