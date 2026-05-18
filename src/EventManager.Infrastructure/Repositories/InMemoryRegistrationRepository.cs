using EventManager.Domain.Entities;
using EventManager.Domain.Interfaces;

namespace EventManager.Infrastructure.Repositories;

public class InMemoryRegistrationRepository : IRegistrationRepository
{
    private readonly List<Registration> _registrations = new();

    public void Add(Registration registration)
    {
        _registrations.Add(registration);
    }

    public List<Registration> GetAll()
    {
        return _registrations;
    }
}