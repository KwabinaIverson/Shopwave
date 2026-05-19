using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Domain;

namespace Shopwave.Modules.Stores.Domain.Events;

public record StoreCreatedEvent(Guid StoreId, Guid OwnerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}