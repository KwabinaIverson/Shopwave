using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Stores.Application.Abstractions;
using Shopwave.Modules.Stores.Domain.Entities;
using Shopwave.Modules.Stores.Infrastructure.Persistence;

namespace Shopwave.Modules.Stores.Infrastructure.Repositories;

public class SellerReferenceRepository : ISellerReferenceRepository
{
    private readonly StoreDbContext _context;

    public SellerReferenceRepository(StoreDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddSellerReferenceAsync(SellerReference sellerReference, CancellationToken ct = default)
    {
        await _context.Set<SellerReference>().AddAsync(sellerReference, ct);
    }

    public async Task<SellerReference?> GetSellerReferenceAsync(Guid sellerId, CancellationToken ct = default)
    {
        return await _context.Set<SellerReference>()
            .FirstOrDefaultAsync(s => s.Id == sellerId, ct);
    }
    
    public async Task<bool> VerifySellerReferenceAsync(Guid sellerId, CancellationToken ct = default)
    {
        return await _context.Set<SellerReference>()
            .AnyAsync(s => s.Id == sellerId, ct);
    }
}