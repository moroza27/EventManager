using System.Text.Json.Serialization;

namespace EventManager.Domain.Entities;

internal class RegistrationsManager
{
    private readonly List<Registration> _registrations = new();

    public IReadOnlyCollection<Registration> AsReadOnly() => _registrations.AsReadOnly();

    [JsonInclude]
    [JsonPropertyName("Registrations")]
    public List<Registration> InternalList
    {
        get => _registrations;
        set
        {
            _registrations.Clear();
            if (value != null)
            {
                _registrations.AddRange(value);
            }
        }
    }

    public bool HasAvailablePlaces(int capacity) => _registrations.Count < capacity;

    public bool IsAlreadyRegistered(Guid participantId) => _registrations.Any(r => r.ParticipantId == participantId);

    public void Add(Registration registration) => _registrations.Add(registration);

    public int Count => _registrations.Count;
}
