namespace Shopwave.Modules.Stores.Application.Abstractions;

public interface ISellerValidationService
{
    Task<bool> UserAlreadyHasStoreAsync(Guid ownerId, CancellationToken ct = default);
    
    Task<bool> IsSlugUniqueAsync(string slug, CancellationToken ct = default);
}