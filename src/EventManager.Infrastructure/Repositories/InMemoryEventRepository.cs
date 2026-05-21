using EventManager.Domain.Entities;
using EventManager.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace EventManager.Infrastructure.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = new();

    public void Add(Event eventItem)
    {
        _events.Add(eventItem);
    }

    public Task AddAsync(Event eventItem, CancellationToken cancellationToken = default)
    {
        _events.Add(eventItem);
        return Task.CompletedTask;
    }

    public Event? GetById(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events.FirstOrDefault(e => e.Id == id));
    }

    public List<Event> GetAll()
    {
        return _events;
    }

    public Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events);
    }

    public Task SaveAsync(CancellationToken c = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken c = default) => Task.CompletedTask;
}