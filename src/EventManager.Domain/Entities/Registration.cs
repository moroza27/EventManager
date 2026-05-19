using System.Text.Json.Serialization;

namespace EventManager.Domain.Entities;

public class Registration : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public DateTime RegistrationDate { get; private set; }

    [JsonConstructor]
    public Registration(Guid id, Guid eventId, Guid participantId, DateTime registrationDate)
    {
        Id = id;
        EventId = eventId;
        ParticipantId = participantId;
        RegistrationDate = registrationDate;
    }

    public Registration(Guid eventId, Guid participantId)
    {
        EventId = eventId;
        ParticipantId = participantId;
        RegistrationDate = DateTime.Now;
    }
}