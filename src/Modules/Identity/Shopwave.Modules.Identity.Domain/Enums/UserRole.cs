namespace Shopwave.Modules.Identity.Domain.Enums;

/// <summary>
/// Represents the different roles a user can have in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// No role assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// Administrator role with full system access.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Seller role for users who sell products.
    /// </summary>
    Seller = 2,

    /// <summary>
    /// Buyer role for users who purchase products.
    /// </summary>
    Buyer = 3
}