using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Stores.Domain.Entities;
using Shopwave.Modules.Stores.Domain.Repositories;
using Shopwave.Modules.Stores.Infrastructure.Persistence; 

namespace Shopwave.Modules.Stores.Infrastructure.Repositories;

public sealed class StoreRepository : IStoreRepository
{
    private readonly StoreDbContext _dbContext;

    public StoreRepository(StoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Store store, CancellationToken ct = default)
    {
        await _dbContext.Stores.AddAsync(store, ct);
    }

    public async Task<Store?> GetByIdAsync(Guid storeId, CancellationToken ct = default)
    {
        return await _dbContext.Stores
            .Include(s => s.PayoutMethods)
            .Include(s => s.Verifications)
            .FirstOrDefaultAsync(s => s.Id == storeId, ct);
    }

    public async Task<Store?> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Stores
            .Include(s => s.PayoutMethods)
            .Include(s => s.Verifications)
            .FirstOrDefaultAsync(s => s.OwnerId == ownerId, ct);
    }

    public async Task<Store?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.Stores
            .FirstOrDefaultAsync(s => s.Slug == slug, ct);
    }

    public async Task<bool> ExistsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _dbContext.Stores.AnyAsync(s => s.OwnerId == ownerId, ct);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbContext.Stores.AnyAsync(s => s.Slug == slug, ct);
    }

    public void Update(Store store)
    {
        _dbContext.Stores.Update(store);
    }

    public void Remove(Store store)
    {
        _dbContext.Stores.Remove(store);
    }
}