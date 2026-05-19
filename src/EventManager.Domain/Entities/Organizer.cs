using System.Text.Json.Serialization;

namespace EventManager.Domain.Entities;

public class Organizer : BaseEntity
{
    public string FullName { get; private set; }
    public string Email { get; private set; }

    [JsonConstructor]
    public Organizer(Guid id, string fullName, string email)
    {
        Id = id;
        FullName = fullName;
        Email = email;
    }

    public Organizer(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Organizer name cannot be empty.");
        }

        if (!email.Contains('@'))
        {
            throw new ArgumentException("Invalid email format.");
        }

        FullName = fullName;
        Email = email;
    }
}