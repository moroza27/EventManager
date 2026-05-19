using EventManager.Application.Common;
using EventManager.Domain.Entities;
using EventManager.Domain.Enums;
using EventManager.Domain.Interfaces;

namespace EventManager.Application.Services;

public class EventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEventObserver _eventObserver;

    public EventService(
        IEventRepository eventRepository, 
        IRegistrationRepository registrationRepository,
        IEventObserver eventObserver)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _eventObserver = eventObserver;
    }

    public Result<Event> CreateEvent(
        string title, string description, DateTime date, int capacity,
        string venueName, string venueAddress, string organizerName, string organizerEmail,
        EventCategory category)
    {
        try
        {
            var venue = new Venue(venueName, venueAddress, capacity);
            var organizer = new Organizer(organizerName, organizerEmail);
            var eventItem = new Event(title, description, date, capacity, venue, organizer, category);

            _eventRepository.Add(eventItem);
            return Result<Event>.Success(eventItem);
        }
        catch (ArgumentException ex)
        {
            return Result<Event>.Failure(ex.Message);
        }
    }

    public Result RegisterParticipant(Guid eventId, string participantName, string participantEmail)
    {
        try
        {
            var eventItem = _eventRepository.GetById(eventId);
            if (eventItem is null)
            {
                return Result.Failure("Event not found.");
            }

            var participant = new Participant(participantName, participantEmail);
            var registration = new Registration(eventItem.Id, participant.Id);

            eventItem.AddRegistration(registration);
            _registrationRepository.Add(registration);

            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return Result.Failure(ex.Message);
        }
    }

    public List<Event> GetAllEvents() => _eventRepository.GetAll();
    public Result CancelEvent(Guid eventId)
    {
        try
        {
            var eventItem = _eventRepository.GetById(eventId);
            if (eventItem is null)
            {
                return Result.Failure("Event not found.");
            }

            eventItem.AttachObserver(_eventObserver);
            
            eventItem.CancelEvent();

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task SaveDataAsync()
    {
        await _eventRepository.SaveAsync();
    }

    public async Task LoadDataAsync()
    {
        await _eventRepository.LoadAsync();
    }

    public List<Event> GetAvailableEvents()
    {
        return _eventRepository.GetAll()
            .Where(e => e.Status == EventStatus.Open && e.HasAvailablePlaces())
            .ToList();
    }

    public List<Event> GetTopPopularEvents(int count)
    {
        return _eventRepository.GetAll()
            .OrderByDescending(e => e.Registrations.Count)
            .Take(count)
            .ToList();
    }

    public Dictionary<EventCategory, int> GetEventCountByCategory()
    {
        return _eventRepository.GetAll()
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public int GetTotalCapacityOfOpenEvents()
    {
        return _eventRepository.GetAll()
            .Where(e => e.Status == EventStatus.Open)
            .Sum(e => e.Capacity);
    }
}