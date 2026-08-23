using Domain.Entities;

namespace Application.Contracts;

public interface IEventRepository
{
    Task<List<Event>> GetAllAsync(CancellationToken cancellationToken);

    ValueTask<Event?> FindAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddAsync(Event @event);

    Task RemoveAsync(Event @event, CancellationToken cancellationToken);

    Task<bool> Exists(Guid eventId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}