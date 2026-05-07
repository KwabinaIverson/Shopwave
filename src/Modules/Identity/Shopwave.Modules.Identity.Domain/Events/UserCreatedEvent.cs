using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Identity.Domain.Events;

/// <summary>
/// Represents a domain event that is raised when a new user is created.
/// </summary>
/// <param name="UserId">The unique identifier of the created user.</param>
/// <param name="Email">The email address of the created user.</param>
public record UserCreatedEvent(Guid UserId, string Email) : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get;  } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when this event occurred, in UTC.
    /// </summary>
    public DateTime OccurredAt { get;  }= DateTime.UtcNow;
}