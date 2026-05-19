using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Stores.Domain.Events;

public record StoreVerificationRejectedEvent(Guid StoreId, Guid OwnerId, string Reason) : IDomainEvent
{
    public Guid EventId { get;  } = Guid.NewGuid();
    public DateTime OccurredAt { get;  } = DateTime.UtcNow;
}