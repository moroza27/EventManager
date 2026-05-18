namespace EventManager.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; private set; }
    public string Address { get; private set; }
    public int MaxCapacity { get; private set; }

    public Venue(string name, string address, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Venue name cannot be empty.");
        }

        if (maxCapacity <= 0)
        {
            throw new ArgumentException("Venue capacity must be greater than zero.");
        }

        Name = name;
        Address = address;
        MaxCapacity = maxCapacity;
    }
}