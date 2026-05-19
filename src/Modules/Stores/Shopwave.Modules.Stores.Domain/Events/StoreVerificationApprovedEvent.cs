using Shopwave.Shared.Abstractions;

namespace Shopwave.Modules.Stores.Domain.Events;

public record StoreVerificationApprovedEvent(Guid StoreId, Guid OwnerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}