namespace EventManager.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}