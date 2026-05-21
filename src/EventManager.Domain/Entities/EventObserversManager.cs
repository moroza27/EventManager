using EventManager.Domain.Interfaces;

namespace EventManager.Domain.Entities;

internal class EventObserversManager
{
    private readonly List<IEventObserver> _observers = new();

    public void Attach(IEventObserver observer)
    {
        if (observer == null) return;
        _observers.Add(observer);
    }

    public void NotifyCancelled(Event ev)
    {
        foreach (var observer in _observers)
        {
            observer.OnEventCancelled(ev);
        }
    }
}
