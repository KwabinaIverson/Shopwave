using System.Text.RegularExpressions;
using Shopwave.Shared.Domain;
using Shopwave.Modules.Stores.Domain.Enums;
using Shopwave.Modules.Stores.Domain.Events;

namespace Shopwave.Modules.Stores.Domain.Entities;

/// <summary>
/// Represents a seller's store (aggregate root) in the Stores module.
/// The <see cref="Store"/> holds business-facing information such as display name,
/// slug, business name, address, payout methods and verification state.
/// </summary>
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

    /// <summary>
    /// Gets the identifier of the user who owns the store.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Gets the public display name for the store.
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Gets the URL-friendly business slug (lowercase, hyphen-separated).
    /// </summary>
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Gets the legal or registered business name for the store.
    /// </summary>
    public string BusinessName { get; private set; } = null!;

    /// <summary>
    /// Gets the current operational status of the store.
    /// </summary>
    public StoreStatus Status { get; private set; }

    /// <summary>
    /// Gets the verification lifecycle status for the store's onboarding documents.
    /// </summary>
    public VerificationStatus VerificationStatus { get; private set; }

    /// <summary>
    /// Gets the business address associated with the store.
    /// </summary>
    public Address BusinessAddress { get; private set; } = null!;

    public IReadOnlyCollection<PayoutMethod> PayoutMethods => _payoutMethods.AsReadOnly();
    public IReadOnlyCollection<StoreVerification> Verifications => _verifications.AsReadOnly();

    private Store() { }

    /// <summary>
    /// Creates a new <see cref="Store"/> instance with the provided details.
    /// </summary>
    /// <param name="ownerId">Identifier of the store owner.</param>
    /// <param name="displayName">Public display name for the store.</param>
    /// <param name="slug">URL-friendly slug for the store.</param>
    /// <param name="businessName">Legal or registered business name.</param>
    /// <param name="address">Business <see cref="Address"/> for the store.</param>
    /// <returns>A newly created <see cref="Store"/> with initial pending state.</returns>
    /// <exception cref="ArgumentException">Thrown when any of the string arguments are invalid.</exception>
    public static Store Create(Guid ownerId, string displayName, string slug, string businessName, Address address)
    {
        var store = new Store
        {
            OwnerId = ownerId,
            DisplayName = ValidateStoreName(displayName),
            Slug = ValidateSlug(slug),
            BusinessName = ValidateStoreName(businessName),
            BusinessAddress = address,
            Status = StoreStatus.Pending,
            VerificationStatus = VerificationStatus.Unverified
        };
        
        store.RaiseDomainEvent(new StoreCreatedEvent(store.Id, store.OwnerId));
        
        return store;
    }
    
    
    /// <summary>
    /// Updates the store's business address.
    /// </summary>
    /// <param name="street1">Primary street address line.</param>
    /// <param name="street2">Optional secondary street address line.</param>
    /// <param name="city">City name.</param>
    /// <param name="region">State, province or region.</param>
    /// <param name="country">Country name.</param>
    /// <param name="postalCode">Optional postal or ZIP code.</param>
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

    /// <summary>
    /// Adds a payout method to the store.
    /// </summary>
    /// <param name="type">Type of payout method.</param>
    /// <param name="provider">Provider name (e.g., bank name or MM provider).</param>
    /// <param name="accountName">Name on the payout account.</param>
    /// <param name="accountIdentifier">Account identifier (e.g., account number or mobile number).</param>
    /// <exception cref="InvalidOperationException">Thrown when the store is suspended.</exception>
    public Guid AddPayoutMethod(PayoutMethodType type, string provider, string accountName, string accountIdentifier)
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
        this.RaiseDomainEvent(new StorePayoutMethodAddedEvent(Id, method.Id));
        return method.Id;
    }

    /// <summary>
    /// Submits onboarding documents for verification.
    /// </summary>
    /// <param name="taxDocumentUrl">URL to the tax document.</param>
    /// <param name="registrationDocumentUrl">URL to the business registration document.</param>
    /// <exception cref="InvalidOperationException">Thrown when the store is already verified.</exception>
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
        this.RaiseDomainEvent(new StoreVerificationSubmittedEvent(Id, OwnerId));
    }

    /// <summary>
    /// Marks the store verification as approved and activates the store.
    /// </summary>
    public void ApproveVerification()
    {
        VerificationStatus = VerificationStatus.Approved;

        Status = StoreStatus.Active;
        this.RaiseDomainEvent(new StoreVerificationApprovedEvent(Id, OwnerId));
    }

    /// <summary>
    /// Validates and normalizes a store display or business name.
    /// </summary>
    /// <param name="value">The store name to validate.</param>
    /// <returns>The trimmed and validated store name.</returns>
    /// <exception cref="ArgumentException">Thrown when the name is null/empty or contains invalid characters.</exception>
    private static string ValidateStoreName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Store name is required.", nameof(value));

        value = value.Trim();

        if (!StoreNameRegex.IsMatch(value))
            throw new ArgumentException("Store name contains invalid characters.", nameof(value));

        return value;
    }

    /// <summary>
    /// Validates a business slug used in URLs.
    /// </summary>
    /// <param name="value">The slug to validate.</param>
    /// <returns>The trimmed slug.</returns>
    /// <exception cref="ArgumentException">Thrown when the slug is null/empty or does not match required format.</exception>
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

    /// <summary>
    /// Marks a payout method as verified and sets it as default if it is the first verified method.
    /// </summary>
    /// <param name="payoutMethodId">Identifier of the payout method to verify.</param>
    /// <param name="verifiedAccountName">The verified account name provided by verification process.</param>
    /// <param name="verificationReference">Reference returned by the verification provider.</param>
    /// <exception cref="InvalidOperationException">Thrown when the payout method is not found.</exception>
    public void VerifyPayoutMethod(Guid payoutMethodId, string verifiedAccountName, string verificationReference)
    {
        var method = _payoutMethods.FirstOrDefault(m => m.Id == payoutMethodId) 
                     ?? throw new InvalidOperationException("Payout method not found.");

        method.Verify(verifiedAccountName, verificationReference);
        
        if (_payoutMethods.Count(m => m.IsVerified) == 1)
        {
            method.SetAsDefault();
        }
        this.RaiseDomainEvent(new StorePayoutMethodVerifiedEvent(Id, method.Id));
    }

    /// <summary>
    /// Sets the specified payout method as the default for the store.
    /// </summary>
    /// <param name="payoutMethodId">Identifier of the payout method to set as default.</param>
    /// <exception cref="InvalidOperationException">Thrown when the payout method is not found or is unverified.</exception>
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

    /// <summary>
    /// Rejects the most recent pending verification with the provided reason and resets store status to draft.
    /// </summary>
    /// <param name="reason">Explanation for rejection shown to the seller.</param>
    /// <exception cref="InvalidOperationException">Thrown when there is no pending verification or wrong verification status.</exception>
    public void RejectVerification(string reason)
    {
        if (VerificationStatus != VerificationStatus.PendingReview)
            throw new InvalidOperationException("Can only reject a pending verification.");
        
        var pendingVerification = _verifications.OrderByDescending(v => v.SubmittedAt).FirstOrDefault()
                                  ?? throw new InvalidOperationException("No pending verification found.");
 
        pendingVerification.Reject(reason);
 
        VerificationStatus = VerificationStatus.Rejected;
        Status = StoreStatus.Draft;
        this.RaiseDomainEvent(new StoreVerificationRejectedEvent(Id, OwnerId, reason));
    }
    
    /// <summary>
    /// Soft-deletes the store by marking it as deleted and recording the deletion time.
    /// Raises a <see cref="StoreDeletedEvent"/> domain event.
    /// </summary>
    public void Archive()
    {
        if (IsDeleted) return;

        Status = StoreStatus.Archived;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new StoreArchivedEvent(Id, OwnerId));
    }
}