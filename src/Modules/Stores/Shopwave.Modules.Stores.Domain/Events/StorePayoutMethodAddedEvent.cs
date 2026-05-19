using Shopwave.Shared.Abstractions;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Domain.Events;

public record StorePayoutMethodAddedEvent(Guid StoreId, Guid PayoutMethodId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}