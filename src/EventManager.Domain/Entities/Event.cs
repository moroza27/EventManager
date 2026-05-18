using EventManager.Domain.Enums;

namespace EventManager.Domain.Entities;

public class Event : BaseEntity
{
    private readonly List<Registration> _registrations = new();

    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public int Capacity { get; private set; }

    public Venue Venue { get; private set; }
    public Organizer Organizer { get; private set; }

    public EventStatus Status { get; private set; }
    public EventCategory Category { get; private set; }

    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();

    public Event(
        string title,
        string description,
        DateTime date,
        int capacity,
        Venue venue,
        Organizer organizer,
        EventCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Event title cannot be empty.");
        }

        if (date < DateTime.Now)
        {
            throw new ArgumentException("Event date cannot be in the past.");
        }

        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.");
        }

        Title = title;
        Description = description;
        Date = date;
        Capacity = capacity;
        Venue = venue;
        Organizer = organizer;
        Category = category;

        Status = EventStatus.Open;
    }

    public Registration? this[Guid participantId]
    {
        get => _registrations.FirstOrDefault(r => r.ParticipantId == participantId);
    }

    public bool HasAvailablePlaces()
    {
        return _registrations.Count < Capacity;
    }

    public void AddRegistration(Registration registration)
    {
        if (Status != EventStatus.Open)
        {
            throw new InvalidOperationException("Registration is closed.");
        }

        if (!HasAvailablePlaces())
        {
            throw new InvalidOperationException("No available places.");
        }

        if (this[registration.ParticipantId] != null)
        {
            throw new InvalidOperationException("Participant already registered.");
        }

        _registrations.Add(registration);
    }
}