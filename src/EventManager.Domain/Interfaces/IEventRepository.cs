using EventManager.Domain.Entities;

namespace EventManager.Domain.Interfaces;

public interface IEventRepository
{
    void Add(Event eventItem);

    Event? GetById(Guid id);

    List<Event> GetAll();
}