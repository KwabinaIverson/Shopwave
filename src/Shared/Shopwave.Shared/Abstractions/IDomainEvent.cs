namespace Shopwave.Shared.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
   public Guid EventId { get;  }
   public DateTime OccurredAt { get;  }
}
