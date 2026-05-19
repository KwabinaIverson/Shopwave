using Shopwave.Shared.Domain;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Domain.Entities;

/// <summary>
/// Represents a verification submission for a <see cref="Store"/>.
/// Stores uploaded documents and maintains review state and timestamps.
/// </summary>
public class StoreVerification : Entity
{
    /// <summary>
    /// Gets the identifier of the store that submitted the verification.
    /// </summary>
    public Guid StoreId { get; private set; }

    /// <summary>
    /// Gets the URL to the tax document uploaded by the seller.
    /// </summary>
    public string? TaxDocumentUrl { get; private set; }

    /// <summary>
    /// Gets the URL to the business registration document uploaded by the seller.
    /// </summary>
    public string? RegistrationDocumentUrl { get; private set; }

    /// <summary>
    /// Timestamp when the verification was submitted (UTC).
    /// </summary>
    public DateTime SubmittedAt { get; private set; }

    /// <summary>
    /// Timestamp when the verification was reviewed, if applicable (UTC).
    /// </summary>
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>
    /// Optional note recorded by the reviewer when approving or rejecting.
    /// </summary>
    public string? ReviewNote { get; private set; }

    /// <summary>
    /// Current verification status for this submission.
    /// </summary>
    public VerificationStatus Status { get; private set; }

    private StoreVerification() { }

    /// <summary>
    /// Initializes a new <see cref="StoreVerification"/> with required document URLs.
    /// </summary>
    /// <param name="storeId">Identifier of the store submitting verification.</param>
    /// <param name="taxDocumentUrl">URL to the tax document.</param>
    /// <param name="registrationDocumentUrl">URL to the registration document.</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are missing or invalid.</exception>
    internal StoreVerification(Guid storeId, string taxDocumentUrl, string registrationDocumentUrl)
    {
        if (storeId == Guid.Empty)
            throw new ArgumentException("StoreId is required.");

        if (string.IsNullOrWhiteSpace(taxDocumentUrl))
            throw new ArgumentException("Tax document is required.");

        if (string.IsNullOrWhiteSpace(registrationDocumentUrl))
            throw new ArgumentException("Registration document is required.");

        StoreId = storeId;
        TaxDocumentUrl = taxDocumentUrl.Trim();
        RegistrationDocumentUrl = registrationDocumentUrl.Trim();

        SubmittedAt = DateTime.UtcNow;
        Status = VerificationStatus.PendingReview;
    }

    /// <summary>
    /// Approves the verification submission. Cannot approve a previously rejected submission.
    /// </summary>
    /// <remarks>
    /// If the submission is already approved the method returns silently.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to approve a rejected submission.</exception>
    internal void Approve()
    {
        if (Status == VerificationStatus.Approved)
            return;

        if (Status == VerificationStatus.Rejected)
            throw new InvalidOperationException("Rejected verification cannot be approved directly.");

        Status = VerificationStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejects the verification submission with a required reason.
    /// </summary>
    /// <param name="reason">Non-empty reason explaining why the submission was rejected.</param>
    /// <remarks>
    /// If the submission is already rejected the method returns silently.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to reject an approved submission.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reason"/> is null or whitespace.</exception>
    internal void Reject(string reason)
    {
        if (Status == VerificationStatus.Rejected)
            return;

        if (Status == VerificationStatus.Approved)
            throw new InvalidOperationException("Approved verification cannot be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.");

        Status = VerificationStatus.Rejected;
        ReviewNote = reason.Trim();
        ReviewedAt = DateTime.UtcNow;
    }
}