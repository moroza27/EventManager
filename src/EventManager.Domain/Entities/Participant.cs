using System.Text.Json.Serialization;

namespace EventManager.Domain.Entities;

public class Participant : BaseEntity
{
    public string FullName { get; private set; }
    public string Email { get; private set; }

    [JsonConstructor]
    public Participant(Guid id, string fullName, string email)
    {
        Id = id;
        FullName = fullName;
        Email = email;
    }

    public Participant(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Participant name cannot be empty.");
        }

        if (!email.Contains('@'))
        {
            throw new ArgumentException("Invalid email format.");
        }

        FullName = fullName;
        Email = email;
    }
}