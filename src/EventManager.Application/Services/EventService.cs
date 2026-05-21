using EventManager.Application.Common;
using EventManager.Domain.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Domain.Exceptions;
using EventManager.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace EventManager.Application.Services;

public class EventService
{
    private readonly IEventRepository _repository;
    private readonly IRegistrationRepository? _registrationRepository;
    private readonly IEventObserver? _observer;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    // Compatibility constructor used by Console app (no logic change, just adapters)
    public EventService(
        IEventRepository repository,
        IRegistrationRepository registrationRepository,
        IEventObserver observer)
    {
        _repository = repository;
        _registrationRepository = registrationRepository;
        _observer = observer;

        // Attach observer to currently loaded events if any
        var events = _repository.GetAll();
        foreach (var ev in events)
        {
            ev.AttachObserver(_observer);
        }
    }

    public async Task<Result> CreateEventAsync(Event eventEntity)
    {
        try
        {
            await _repository.AddAsync(eventEntity);
            await _repository.SaveAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    // Console-friendly synchronous wrapper used by Program.cs
    public Result CreateEvent(string title, string description, DateTime date, int capacity,
        string venueName, string venueAddress,
        string organizerName, string organizerEmail,
        EventCategory category)
    {
        var venue = new Domain.Entities.Venue(venueName, venueAddress, capacity);
        var organizer = new Domain.Entities.Organizer(organizerName, organizerEmail);
        var ev = new Event(title, description, date, capacity, venue, organizer, category);

        // Attach observer if provided
        if (_observer != null) ev.AttachObserver(_observer);

        var res = CreateEventAsync(ev).GetAwaiter().GetResult();
        return res;
    }

    public async Task LoadDataAsync()
    {
        await _repository.LoadAsync();
        // Re-attach observer to events after load
        if (_observer != null)
        {
            foreach (var ev in _repository.GetAll()) ev.AttachObserver(_observer);
        }
    }

    public List<Event> GetAllEvents()
    {
        return _repository.GetAll();
    }

    public Result RegisterParticipant(Guid eventId, string name, string email)
    {
        var participant = new Participant(name, email);
        var res = RegisterParticipantAsync(eventId, participant).GetAwaiter().GetResult();
        if (res.IsSuccess && _registrationRepository != null)
        {
            // persist registration to repository
            var registration = new Registration(eventId, participant.Id);
            _registrationRepository.Add(registration);
        }
        return res;
    }

    public Result CancelEvent(Guid eventId)
    {
        var ev = _repository.GetById(eventId);
        if (ev is null) return Result.Failure("Event not found.");
        try
        {
            ev.CancelEvent();
            _repository.SaveAsync().GetAwaiter().GetResult();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task SaveDataAsync()
    {
        await _repository.SaveAsync();
    }

    public List<Event> GetAvailableEvents()
    {
        return _repository.GetAll().Where(e => e.HasAvailablePlaces() && e.Status == EventStatus.Open).ToList();
    }

    public List<Event> GetTopPopularEvents(int topN)
    {
        return _repository.GetAll().OrderByDescending(e => e.Registrations.Count).Take(topN).ToList();
    }

    public int GetTotalCapacityOfOpenEvents()
    {
        return _repository.GetAll().Where(e => e.Status == EventStatus.Open).Sum(e => e.Capacity);
    }

    public Dictionary<EventCategory, int> GetEventCountByCategory()
    {
        return _repository.GetAll()
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Result> RegisterParticipantAsync(
        Guid eventId,
        Participant participant)
    {
        var eventEntity = await _repository.GetByIdAsync(eventId);

        if (eventEntity is null)
        {
            return Result.Failure("Event not found.");
        }

        try
        {

            var registration = new Registration(eventId, participant.Id);
            eventEntity.AddRegistration(registration);

            await _repository.SaveAsync();

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
}