using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Domain.Entities;
using Domain.Entities.ValueObjects;

namespace Application.Services;

public class EventService(IEventRepository repository) : IEventService
{
	public async Task<PaginatedResultDto<EventInfoDto>> GetAllAsync(Filters filters, int page, int pageSize, CancellationToken cancellationToken)
	{
		var allEvents  = await repository.GetAllAsync(cancellationToken);
		IEnumerable<Event> filteredEvents = allEvents
			.WhereIf(!string.IsNullOrWhiteSpace(filters.Title), x => x.Title.Contains(filters.Title!, StringComparison.OrdinalIgnoreCase))
			.WhereIf(filters.From.HasValue, x => x.Period.StartAt >= filters.From)
			.WhereIf(filters.To.HasValue, x => x.Period.EndAt <= filters.To).ToArray();

		var totalItems = filteredEvents.Count();
		var result = filteredEvents.Skip((page - 1) * pageSize).Take(pageSize).Select(EventInfoDto.ToDto).ToArray();

		return new PaginatedResultDto<EventInfoDto>(totalItems, page, result.Length, result);
	}

	public async Task<EventInfoDto> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
	{
		var eventData = await repository.FindAsync(eventId, cancellationToken);
		return eventData != null ? EventInfoDto.ToDto(eventData) : throw new EntityNotFoundException("Событие", eventId);
	}

	public async Task<EventInfoDto> CreateAsync(EventDto dto, CancellationToken cancellationToken)
	{
		var eventData = Event.Create(dto.Id, dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt), dto.TotalSeats);
		await repository.AddAsync(eventData);
		
		await repository.SaveChangesAsync(cancellationToken);
		
		return EventInfoDto.ToDto(eventData);
	}

	public async Task<EventInfoDto> UpdateAsync(Guid eventId, EventDto dto, CancellationToken cancellationToken)
	{
		var eventToUpdate = await repository.FindAsync(eventId, cancellationToken);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		eventToUpdate.Update(dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt));
		await repository.SaveChangesAsync(cancellationToken);
		
		return EventInfoDto.ToDto(eventToUpdate);
	}

	public async Task DeleteAsync(Guid eventId, CancellationToken cancellationToken)
	{
		var eventToDelete = await repository.FindAsync(eventId, cancellationToken);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		await repository.RemoveAsync(eventToDelete, cancellationToken);
		await repository.SaveChangesAsync(cancellationToken);
	}
	
	public async Task<bool> TryReserveSeatsAsync(Guid eventId, CancellationToken cancellationToken, int seats = 1)
	{
		var eventToReserve = await repository.FindAsync(eventId, cancellationToken);

		if (eventToReserve is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		return eventToReserve.TryReserveSeats(seats);
	}
}