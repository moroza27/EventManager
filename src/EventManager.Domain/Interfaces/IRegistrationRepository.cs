using EventManager.Domain.Entities;

namespace EventManager.Domain.Interfaces;

public interface IRegistrationRepository
{
    void Add(Registration registration);

    List<Registration> GetAll();
}