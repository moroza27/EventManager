using EventManager.Domain.Entities;

namespace EventManager.Domain.Interfaces;

public interface IEventRepository
{
    void Add(Event eventItem);

    Event? GetById(Guid id);

    List<Event> GetAll();
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);

    // Async counterparts used by higher-level services
    Task AddAsync(Event eventItem, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default);
}