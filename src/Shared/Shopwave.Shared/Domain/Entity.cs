using Shopwave.Shared.Abstractions;

namespace Shopwave.Shared.Domain;

/// <summary>
/// Represents a base class for domain entities.
/// Entities are objects that have a unique identity and are defined by their identity rather than their attributes.
/// </summary>
public class Entity
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when this entity was created, in UTC.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the date and time when this entity was last updated, in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets a value indicating whether this entity has been marked as deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Gets the date and time when this entity was deleted, in UTC, or null if not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    private readonly List<IDomainEvent> _events = new();

    /// <summary>
    /// Raises a domain event for this entity.
    /// Domain events represent significant changes in the domain that other parts of the system may be interested in.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
        _events.Add(domainEvent);
    }

    /// <summary>
    /// Gets a read-only list of domain events that have been raised for this entity.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.AsReadOnly();

    /// <summary>
    /// Clears all domain events for this entity.
    /// This method should be called after the events have been dispatched to prevent duplicate processing.
    /// </summary>
    public void ClearDomainEvents() => _events.Clear();
}