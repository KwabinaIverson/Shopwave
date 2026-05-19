namespace Shopwave.Modules.Stores.Domain.Enums;

/// <summary>
/// Represents the operational lifecycle status of a store.
/// </summary>
public enum StoreStatus
{
    /// <summary>The store is hidden; setup is incomplete.</summary>
    Draft = 0,

    /// <summary>The store is live and customers can browse and buy products.</summary>
    Active = 1,

    /// <summary>The seller temporarily paused operations (e.g., holiday or out of stock).</summary>
    Paused = 2,

    /// <summary>The marketplace platform forced the store offline due to policy violations.</summary>
    Suspended = 3,

    /// <summary>The seller closed the account; data is retained for legal/tax history.</summary>
    Archived = 4,

    /// <summary>The is pending for approval</summary>
    Pending = 5
}