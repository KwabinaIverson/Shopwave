using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Stores.Domain.Events;

public record StorePayoutMethodVerifiedEvent(Guid StoreId, Guid PayoutMethodId) : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get;  } = Guid.NewGuid();
    
    public DateTime OccurredAt { get;  } = DateTime.UtcNow;
}