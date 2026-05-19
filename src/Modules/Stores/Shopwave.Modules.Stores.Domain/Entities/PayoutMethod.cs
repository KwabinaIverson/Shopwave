using System.Text.RegularExpressions;
using Shopwave.Shared.Domain;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Domain.Entities;

/// <summary>
/// Represents a payout method for a store (e.g., bank account or mobile money).
/// Instances are created by the store aggregate and contain verification metadata.
/// </summary>
public class PayoutMethod : Entity
{
    private static readonly Regex ProviderRegex = new(
        @"^[\p{L}\p{N}\s&.\-]{2,50}$",
        RegexOptions.Compiled
    );

    private static readonly Regex AccountNameRegex = new(
        @"^[\p{L}\p{N}\s&'.,()\-]{2,100}$",
        RegexOptions.Compiled
    );

    private static readonly Regex GhanaMomoRegex = new(
        @"^0(20|24|26|27|50|54|55|59)\d{7}$",
        RegexOptions.Compiled
    );

    private static readonly Regex BankAccountRegex = new(
        @"^\d{8,20}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Gets the type of payout method (BankAccount, MobileMoney, etc.).
    /// </summary>
    public PayoutMethodType Type { get; private set; }

    /// <summary>
    /// Provider name (e.g., bank or mobile money operator).
    /// </summary>
    public string? Provider { get; private set; }

    /// <summary>
    /// The account name associated with this payout method.
    /// </summary>
    public string? AccountName { get; private set; }

    /// <summary>
    /// Identifier for the account (e.g., account number or mobile number).
    /// </summary>
    public string? AccountIdentifier { get; private set; }

    /// <summary>
    /// Whether the payout method has been verified by the platform or a third party.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// When the payout method was verified, if applicable.
    /// </summary>
    public DateTime? VerifiedAt { get; private set; }

    /// <summary>
    /// Reference returned by the verification provider.
    /// </summary>
    public string? VerificationReference { get; private set; }

    /// <summary>
    /// Whether this payout method is marked as the default for the store.
    /// </summary>
    public bool IsDefault { get; private set; }

    private PayoutMethod() { }

    /// <summary>
    /// Initializes a new instance of <see cref="PayoutMethod"/>.
    /// Intended for internal use by the <see cref="Store"/> aggregate.
    /// </summary>
    /// <param name="type">Payout method type.</param>
    /// <param name="provider">Provider name.</param>
    /// <param name="accountName">Account holder name.</param>
    /// <param name="accountIdentifier">Account identifier.</param>
    internal PayoutMethod(PayoutMethodType type, string provider, string accountName, string accountIdentifier)
    {
        Type = type;
        Provider = ValidateProvider(provider);
        
        AccountName = ValidateAccountName(accountName);
        AccountIdentifier = ValidateAccountIdentifier(
            type,
            accountIdentifier
        );
    }

    /// <summary>
    /// Marks the payout method as verified and records verification metadata.
    /// </summary>
    /// <param name="verifiedAccountName">Account name confirmed during verification.</param>
    /// <param name="verificationReference">Reference identifier from the verification provider.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="verificationReference"/> is null or empty.</exception>
    internal void Verify(
        string verifiedAccountName,
        string verificationReference)
    {
        if (IsVerified)
            return;

        if (string.IsNullOrWhiteSpace(verificationReference))
            throw new ArgumentException("Verification reference is required.", nameof(verificationReference));

        AccountName = ValidateAccountName(
            verifiedAccountName
        );

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerificationReference = verificationReference;
    }

    /// <summary>
    /// Marks this payout method as the default. Only verified methods may be set as default.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the method is not verified.</exception>
    internal void SetAsDefault()
    {
        if (!IsVerified)
            throw new InvalidOperationException("Only verified payout methods can be set as default.");

        IsDefault = true;
    }

    /// <summary>
    /// Removes the default designation from this payout method.
    /// </summary>
    internal void RemoveDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Validates the provider string.
    /// </summary>
    /// <param name="value">Provider value to validate.</param>
    /// <returns>Trimmed provider value.</returns>
    /// <exception cref="ArgumentException">Thrown when provider is null/empty or contains invalid characters.</exception>
    private static string ValidateProvider(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Provider is required.", nameof(value));

        value = value.Trim();

        if (!ProviderRegex.IsMatch(value))
            throw new ArgumentException("Provider contains invalid characters.", nameof(value));

        return value;
    }

    /// <summary>
    /// Validates the account name.
    /// </summary>
    /// <param name="value">Account name to validate.</param>
    /// <returns>Trimmed account name.</returns>
    /// <exception cref="ArgumentException">Thrown when the account name is null/empty or contains invalid characters.</exception>
    private static string ValidateAccountName(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account name is required.", nameof(value));

        value = value.Trim();

        if (!AccountNameRegex.IsMatch(value))
            throw new ArgumentException("Account name contains invalid characters.", nameof(value));

        return value;
    }

    /// <summary>
    /// Validates the account identifier based on payout method type.
    /// </summary>
    /// <param name="type">Payout method type.</param>
    /// <param name="value">Identifier to validate.</param>
    /// <returns>Trimmed identifier.</returns>
    /// <exception cref="ArgumentException">Thrown when identifier is null/empty or fails pattern checks for the specified type.</exception>
    private static string ValidateAccountIdentifier(
        PayoutMethodType type,
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account identifier is required.", nameof(value));

        value = value.Trim();

        var isValid = type switch
        {
            PayoutMethodType.MobileMoney =>
                GhanaMomoRegex.IsMatch(value),

            PayoutMethodType.BankAccount =>
                BankAccountRegex.IsMatch(value),

            _ => false
        };

        if (!isValid)
            throw new ArgumentException("Invalid account identifier.", nameof(value));

        return value;
    }
}