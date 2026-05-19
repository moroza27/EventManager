using System.Text.Json.Serialization;
using EventManager.Domain.Enums;
using EventManager.Domain.Interfaces;

namespace EventManager.Domain.Entities;

public class Event : BaseEntity
{
    private readonly List<Registration> _registrations = new();
    private readonly List<IEventObserver> _observers = new();

    [JsonInclude]
    public string Title { get; private set; }

    [JsonInclude]
    public string Description { get; private set; }

    [JsonInclude]
    public DateTime Date { get; private set; }

    [JsonInclude]
    public int Capacity { get; private set; }

    [JsonInclude]
    public Venue Venue { get; private set; }

    [JsonInclude]
    public Organizer Organizer { get; private set; }

    [JsonInclude]
    public EventStatus Status { get; private set; }

    [JsonInclude]
    public EventCategory Category { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();

    [JsonInclude]
    [JsonPropertyName("Registrations")]
    public List<Registration> RegistrationsInternal
    {
        get => _registrations;
        private set
        {
            _registrations.Clear();
            if (value != null)
            {
                _registrations.AddRange(value);
            }
        }
    }

    public Event()
    {
        Status = EventStatus.Open;
    }

    public Event(string title, string description, DateTime date, int capacity, Venue venue, Organizer organizer, EventCategory category)
    {
        if (string.IsNullOrWhiteSpace(title)) 
            throw new ArgumentException("Event title cannot be empty.");

        if (date < DateTime.Now) 
            throw new ArgumentException("Event date cannot be in the past.");

        if (capacity <= 0) 
            throw new ArgumentException("Capacity must be greater than zero.");

        Title = title; Description = description; Date = date; Capacity = capacity;
        Venue = venue; Organizer = organizer; Category = category;
        Status = EventStatus.Open;
    }

    public bool HasAvailablePlaces() => _registrations.Count < Capacity;

    public void AddRegistration(Registration registration)
    {
        if (Status != EventStatus.Open) 
            throw new InvalidOperationException($"Cannot register. Event status is {Status}.");

        if (!HasAvailablePlaces()) 
            throw new InvalidOperationException("No available places.");

        _registrations.Add(registration);
    }

    public void AttachObserver(IEventObserver observer) => _observers.Add(observer);

    public void CancelEvent()
    {
        if (Status == EventStatus.Cancelled || Status == EventStatus.Closed)
        {
            throw new InvalidOperationException("Cannot cancel an event that is already Cancelled or Closed.");
        }

        Status = EventStatus.Cancelled;

        foreach (var observer in _observers)
        {
            observer.OnEventCancelled(this);
        }
    }
}