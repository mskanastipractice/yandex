using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Extensions;
using Domain.Entities;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    public Task<FilteredResult<Event>> GetFiltered(int page, int pageSize, Filters filters, CancellationToken cancellationToken)
    {
        var query = context.Events.AsQueryable();

        var filteredEvents = query
            .AsEnumerable()
            .WhereIf(!string.IsNullOrWhiteSpace(filters.Title), x => x.Title.Contains(filters.Title!, StringComparison.OrdinalIgnoreCase))
            .WhereIf(filters.From.HasValue, x => x.Period.StartAt >= filters.From)
            .WhereIf(filters.To.HasValue, x => x.Period.EndAt <= filters.To)
            .ToArray();

        var totalItems = filteredEvents.Length;

        var result = filteredEvents
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return Task.FromResult(new FilteredResult<Event>(totalItems, result));
    }

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