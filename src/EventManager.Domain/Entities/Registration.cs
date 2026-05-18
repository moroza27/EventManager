namespace EventManager.Domain.Entities;

public class Registration : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public DateTime RegistrationDate { get; private set; }

    public Registration(Guid eventId, Guid participantId)
    {
        EventId = eventId;
        ParticipantId = participantId;
        RegistrationDate = DateTime.Now;
    }
}