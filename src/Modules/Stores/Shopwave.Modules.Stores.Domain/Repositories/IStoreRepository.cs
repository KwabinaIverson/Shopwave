using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Domain.Repositories;

public interface IStoreRepository
{
    Task AddAsync(Store store, CancellationToken ct = default);
    Task<Store?> GetByIdAsync(Guid storeId, CancellationToken ct = default);
    Task<Store?> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<Store?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);

    // --- State Modification Methods ---
    void Update(Store store);
    void Remove(Store store);
}