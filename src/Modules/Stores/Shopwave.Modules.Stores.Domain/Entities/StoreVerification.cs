using Shopwave.Shared.Domain;
using Shopwave.Modules.Stores.Domain.Enums;

namespace Shopwave.Modules.Stores.Domain.Entities;

public class StoreVerification : Entity
{
    public Guid StoreId { get; private set; }

    public string? TaxDocumentUrl { get; private set; }

    public string? RegistrationDocumentUrl { get; private set; }

    public DateTime SubmittedAt { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public string? ReviewNote { get; private set; }

    public VerificationStatus Status { get; private set; }

    private StoreVerification() { }

    public StoreVerification(Guid storeId, string taxDocumentUrl, string registrationDocumentUrl)
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

    public void Approve()
    {
        if (Status == VerificationStatus.Approved)
            return;

        if (Status == VerificationStatus.Rejected)
            throw new InvalidOperationException("Rejected verification cannot be approved directly.");

        Status = VerificationStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
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