using EventManager.Domain.Entities;

namespace EventManager.Domain.Interfaces;

public interface IEventObserver
{
    void OnEventCancelled(Event cancelledEvent);
}