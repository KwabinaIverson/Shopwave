namespace Shopwave.Modules.Stores.Domain.Enums;

/// <summary>
/// Specifies supported payout method categories used by stores.
/// </summary>
public enum PayoutMethodType
{
    /// <summary>Default fallback state to prevent accidental processing errors.</summary>
    Unknown = 0,

    /// <summary>Traditional corporate ACH, Wire, or SEPA bank transfers.</summary>
    BankAccount = 1,

    /// <summary>Telco-based digital wallets popular in emerging markets (e.g., MTN MoMo, M-Pesa, Orange Money).</summary>
    MobileMoney = 2,
}