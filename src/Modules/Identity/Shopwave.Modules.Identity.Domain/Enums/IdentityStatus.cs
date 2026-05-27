namespace Shopwave.Modules.Identity.Domain.Enums;

/// <summary>
/// Specifies the operational and administrative states of a user account.
/// </summary>
public enum IdentityStatus
{
    /// <summary>
    /// Account is created but email or identity verification is incomplete. Restricts system access.
    /// </summary>
    Unverified = 0,

    /// <summary>
    /// Account is in good standing. Full access to standard platform features.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Temporary restriction applied by administrators or automated fraud detection systems.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Permanent restriction due to severe policy violations. Access strictly denied.
    /// </summary>
    Banned = 4,

    /// <summary>
    /// Account soft-deleted or deactivated by the user. Retained for historical records.
    /// </summary>
    Archived = 8
}