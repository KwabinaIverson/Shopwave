using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Stores.Domain.Events;

public record StoreArchivedEvent(Guid StoreId, Guid OwnerId) : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get;  } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when this event occurred, in UTC.
    /// </summary>
    public DateTime OccurredAt { get;  } = DateTime.UtcNow;
}