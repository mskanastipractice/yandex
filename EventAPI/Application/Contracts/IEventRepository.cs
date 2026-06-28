using Domain.Entities;

namespace Application.Contracts;

public interface IEventRepository
{
    IReadOnlyCollection<Event> GetAll();
	
    Event? Find(Guid eventId);

    void Add(Event @event);

    void Remove(Event @event);
}