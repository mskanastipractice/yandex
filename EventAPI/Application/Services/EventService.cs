using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Domain.Entities;
using Domain.Entities.ValueObjects;

namespace Application.Services;

public class EventService(IEventRepository repository) : IEventService
{
	public PaginatedResultDto<EventInfoDto> GetAll(Filters filters, int page, int pageSize)
	{
		IEnumerable<Event> filteredEvents = repository.GetAll()
			.WhereIf(!string.IsNullOrWhiteSpace(filters.Title), x => x.Title.Contains(filters.Title!, StringComparison.OrdinalIgnoreCase))
			.WhereIf(filters.From.HasValue, x => x.Period.StartAt >= filters.From)
			.WhereIf(filters.To.HasValue, x => x.Period.EndAt <= filters.To);

		var totalItems = filteredEvents.Count();
		var result = filteredEvents.Skip((page - 1) * pageSize).Take(pageSize).Select(EventInfoDto.ToDto).ToArray();

		return new PaginatedResultDto<EventInfoDto>(totalItems, page, result.Length, result);
	}

	public EventInfoDto GetById(Guid eventId)
	{
		var eventData = repository.Find(eventId);
		return eventData != null ? EventInfoDto.ToDto(eventData) : throw new EntityNotFoundException("Событие", eventId);
	}

	public Task<EventInfoDto> CreateAsync(EventDto dto)
	{
		var eventData = Event.Create(dto.Id, dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt), dto.TotalSeats);
		repository.Add(eventData);
		
		return Task.FromResult(EventInfoDto.ToDto(eventData));
	}

	public EventInfoDto Update(Guid eventId, EventDto dto)
	{
		var eventToUpdate = repository.Find(eventId);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		eventToUpdate.Update(dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt));
		return EventInfoDto.ToDto(eventToUpdate);
	}

	public void Delete(Guid eventId)
	{
		var eventToDelete = repository.Find(eventId);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		repository.Remove(eventToDelete);
	}
	
	public bool TryReserveSeats(Guid eventId, int seats = 1)
	{
		var eventToReserve = repository.Find(eventId);

		if (eventToReserve is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		return eventToReserve.TryReserveSeats(seats);
	}
}