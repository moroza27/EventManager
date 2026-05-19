using EventManager.Domain.Entities;

namespace EventManager.Domain.Interfaces;

public interface IEventRepository
{
    void Add(Event eventItem);

    Event? GetById(Guid id);

    List<Event> GetAll();
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
}