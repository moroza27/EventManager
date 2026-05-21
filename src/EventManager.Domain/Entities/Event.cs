using System.Text.Json.Serialization;
using EventManager.Domain.Enums;
using EventManager.Domain.Exceptions;
using EventManager.Domain.Interfaces;

namespace EventManager.Domain.Entities;

public class Event : BaseEntity
{
    private readonly RegistrationsManager _registrations = new();
    private readonly EventObserversManager _observers = new();

    [JsonInclude]
    public string Title { get; private set; } = string.Empty;

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public DateTime Date { get; private set; }

    [JsonInclude]
    public int Capacity { get; private set; }

    [JsonInclude]
    public Venue Venue { get; private set; } = default!;

    [JsonInclude]
    public Organizer Organizer { get; private set; } = default!;

    [JsonInclude]
    public EventStatus Status { get; private set; }

    [JsonInclude]
    public EventCategory Category { get; private set; }

    [JsonIgnore]
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();

    // Keep JSON shape compatible with previous implementation
    public List<Registration> RegistrationsInternal
    {
        get => _registrations.InternalList;
        private set => _registrations.InternalList = value;
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

    public bool HasAvailablePlaces() => _registrations.HasAvailablePlaces(Capacity);

    public void AddRegistration(Registration registration)
    {
        if (Status != EventStatus.Open)
            throw new DomainException($"Cannot register. Event status is {Status}.");

        if (_registrations.IsAlreadyRegistered(registration.ParticipantId))
            throw new DomainException("Participant is already registered for this event.");

        if (!HasAvailablePlaces())
            throw new DomainException("No available places.");

        _registrations.Add(registration);
    }

    public void AttachObserver(IEventObserver observer) => _observers.Attach(observer);

    public void CancelEvent()
    {
        if (Status == EventStatus.Cancelled || Status == EventStatus.Closed)
        {
            throw new DomainException("Cannot cancel an event that is already Cancelled or Closed.");
        }

        Status = EventStatus.Cancelled;

        _observers.NotifyCancelled(this);
    }
}