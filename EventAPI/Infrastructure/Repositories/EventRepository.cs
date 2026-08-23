using Application.Contracts;
using Domain.Entities;
using Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    public Task<List<Event>> GetAllAsync(CancellationToken cancellationToken)
        => context.Events.ToListAsync(cancellationToken);

    public ValueTask<Event?> FindAsync(Guid eventId, CancellationToken cancellationToken) 
        => context.Events.FindAsync([eventId], cancellationToken);

    public Task AddAsync(Event @event)
    {
        context.Events.Add(@event);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Event @event, CancellationToken cancellationToken)
    {
        context.Events.Remove(@event);
        return Task.CompletedTask;
    }

    public Task<bool> Exists(Guid eventId, CancellationToken cancellationToken) 
        => context.Events.AnyAsync(e => e.Id == eventId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) 
        => context.SaveChangesAsync(cancellationToken);
}