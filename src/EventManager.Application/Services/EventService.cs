// Шлях: src/EventManager.Application/Services/EventService.cs
using EventManager.Application.Common;
using EventManager.Domain.Entities;
using EventManager.Domain.Enums;
using EventManager.Domain.Interfaces;

namespace EventManager.Application.Services;

public class EventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IRegistrationRepository _registrationRepository;

    public EventService(IEventRepository eventRepository, IRegistrationRepository registrationRepository)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
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
}