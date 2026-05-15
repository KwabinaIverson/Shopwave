namespace Shopwave.Modules.Stores.Domain.Enums;

public enum VerificationStatus
{
    /// <summary>The seller has not uploaded onboarding documents yet.</summary>
    Unverified = 0,

    /// <summary>Documents are uploaded and awaiting review by administrators or third-party APIs.</summary>
    PendingReview = 1,

    /// <summary>Documents were rejected (e.g., expired ID, blurry upload). Action required from seller.</summary>
    Rejected = 2,

    /// <summary>Legitimacy confirmed. The seller is authorized to sell and receive payouts.</summary>
    Approved = 3,

    /// <summary>Previously approved, but undergoing mandatory annual re-verification or triggered by a high-risk change.</summary>
    ReverificationRequired = 4
}