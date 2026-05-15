using System.Text.RegularExpressions;
using Shopwave.Shared.Domain;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Domain.Entities;

public class Store : AggregateRoot
{
    private readonly List<PayoutMethod> _payoutMethods = new();
    private readonly List<StoreVerification> _verifications = new();

    private static readonly Regex StoreNameRegex = new(
        @"^[\p{L}\p{N}\s&'.,()\-]{2,100}$",
        RegexOptions.Compiled
    );

    private static readonly Regex BusinessSlugRegex = new(
        @"^(?=.{3,50}$)[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled
    );

    public Guid OwnerId { get; private set; }

    public string DisplayName { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string BusinessName { get; private set; } = null!;

    public StoreStatus Status { get; private set; }

    public VerificationStatus VerificationStatus { get; private set; }

    public Address BusinessAddress { get; private set; } = null!;

    public IReadOnlyCollection<PayoutMethod> PayoutMethods => _payoutMethods.AsReadOnly();
    public IReadOnlyCollection<StoreVerification> Verifications => _verifications.AsReadOnly();

    private Store() { }

    public static Store Create(Guid ownerId, string displayName, string slug, string businessName, Address address)
    {
        return new Store
        {
            OwnerId = ownerId,
            DisplayName = ValidateStoreName(displayName),
            Slug = ValidateSlug(slug),
            BusinessName = ValidateStoreName(businessName),
            BusinessAddress = address,
            Status = StoreStatus.Pending,
            VerificationStatus = VerificationStatus.Unverified
        };
    }

    public void UpdateBusinessAddress(string street1, string? street2, string city, string region, string country,
        string? postalCode)
    {
        BusinessAddress = new Address(
            street1,
            street2,
            city,
            region,
            country,
            postalCode
        );
    }

    public void AddPayoutMethod(PayoutMethodType type, string provider, string accountName, string accountIdentifier)
    {
        if (Status == StoreStatus.Suspended)
            throw new InvalidOperationException("Suspended stores cannot add payout methods.");

        var method = new PayoutMethod(
            type,
            provider,
            accountName,
            accountIdentifier
        );

        _payoutMethods.Add(method);
    }

    public void SubmitForVerification(string taxDocumentUrl, string registrationDocumentUrl)
    {
        if (VerificationStatus == VerificationStatus.Approved)
            throw new InvalidOperationException("Store is already verified.");

        var verification = new StoreVerification(
            Id,
            taxDocumentUrl,
            registrationDocumentUrl);

        _verifications.Add(verification);

        VerificationStatus = VerificationStatus.PendingReview;
    }

    public void ApproveVerification()
    {
        VerificationStatus = VerificationStatus.Approved;

        Status = StoreStatus.Active;
    }

    private static string ValidateStoreName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Store name is required.", nameof(value));

        value = value.Trim();

        if (!StoreNameRegex.IsMatch(value))
            throw new ArgumentException("Store name contains invalid characters.", nameof(value));

        return value;
    }

    private static string ValidateSlug(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug is required.", nameof(value));

        value = value.Trim();

        if (!BusinessSlugRegex.IsMatch(value))
            throw new ArgumentException(
                "Slug must be 3–50 characters and contain only lowercase letters, numbers, and hyphens.",
                nameof(value));

        return value;
    }

    public void VerifyPayoutMethod(Guid payoutMethodId, string verifiedAccountName, string verificationReference)
    {
        var method = _payoutMethods.FirstOrDefault(m => m.Id == payoutMethodId) 
                     ?? throw new InvalidOperationException("Payout method not found.");

        method.Verify(verifiedAccountName, verificationReference);
        
        if (_payoutMethods.Count(m => m.IsVerified) == 1)
        {
            method.SetAsDefault();
        }
    }

    public void SetDefaultPayoutMethod(Guid payoutMethodId)
    {
        var methodToDefault = _payoutMethods.FirstOrDefault(m => m.Id == payoutMethodId)
                              ?? throw new InvalidOperationException("Payout method not found.");

        if (!methodToDefault.IsVerified)
            throw new InvalidOperationException("Cannot set an unverified method as default.");
        
        foreach (var method in _payoutMethods)
        {
            method.RemoveDefault();
        }

        methodToDefault.SetAsDefault();
    }

    public void RejectVerification(string reason)
    {
        if (VerificationStatus != VerificationStatus.PendingReview)
            throw new InvalidOperationException("Can only reject a pending verification.");
        
        var pendingVerification = _verifications.OrderByDescending(v => v.SubmittedAt).FirstOrDefault()
                                  ?? throw new InvalidOperationException("No pending verification found.");

        pendingVerification.Reject(reason);

        VerificationStatus = VerificationStatus.Rejected;
        Status = StoreStatus.Draft;
    }
}