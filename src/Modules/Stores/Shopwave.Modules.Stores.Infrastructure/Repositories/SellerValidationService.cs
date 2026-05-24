using Shopwave.Modules.Stores.Application.Abstractions;
using Shopwave.Modules.Stores.Domain.Repositories;

namespace Shopwave.Modules.Stores.Infrastructure.Repositories;

public sealed class SellerValidationService : ISellerValidationService
{
    private readonly IStoreRepository _storeRepository;

    public SellerValidationService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }
    
    public async Task<bool> UserAlreadyHasStoreAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _storeRepository.ExistsByOwnerIdAsync(ownerId, ct);
    }
    
    public async Task<bool> IsSlugUniqueAsync(string slug, CancellationToken ct = default)
    {
        var exists = await _storeRepository.ExistsBySlugAsync(slug, ct);
        
        return !exists; 
    }
}