using EventManager.Domain.Entities;
using EventManager.Domain.Interfaces;

namespace EventManager.Infrastructure.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = new();

    public void Add(Event eventItem)
    {
        _events.Add(eventItem);
    }

    public Event? GetById(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public List<Event> GetAll()
    {
        return _events;
    }
}