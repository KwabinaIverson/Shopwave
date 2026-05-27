namespace Shopwave.Modules.Stores.Domain.Entities;

public class SellerReference
{
    public Guid Id { get; private set; } 
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
	public DateTime? DeletedAt { get; private set; }
    public Store? Store { get; private set; }
    
    private SellerReference() { }
    
    public static SellerReference Create(Guid identityUserId)
    {
        if (identityUserId == Guid.Empty)
        {
            throw new ArgumentException("Seller ID cannot be an empty Guid.", nameof(identityUserId));
        }

        return new SellerReference
        {
            Id = identityUserId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }
    
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

    	IsDeleted = true;
    	DeletedAt = DateTime.UtcNow;
    }

	public void Restore()
	{
    	IsDeleted = false;
    	DeletedAt = null;
	}
}