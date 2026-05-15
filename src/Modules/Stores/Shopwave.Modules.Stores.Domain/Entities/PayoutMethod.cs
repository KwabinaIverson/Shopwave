using System.Text.RegularExpressions;
using Shopwave.Shared.Domain;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Domain.Entities;

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

    public PayoutMethodType Type { get; private set; }

    public string? Provider { get; private set; }

    public string? AccountName { get; private set; }

    public string? AccountIdentifier { get; private set; }

    public bool IsVerified { get; private set; }

    public DateTime? VerifiedAt { get; private set; }

    public string? VerificationReference { get; private set; }

    public bool IsDefault { get; private set; }

    private PayoutMethod() { }

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

    internal void SetAsDefault()
    {
        if (!IsVerified)
            throw new InvalidOperationException("Only verified payout methods can be set as default.");

        IsDefault = true;
    }

    internal void RemoveDefault()
    {
        IsDefault = false;
    }

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