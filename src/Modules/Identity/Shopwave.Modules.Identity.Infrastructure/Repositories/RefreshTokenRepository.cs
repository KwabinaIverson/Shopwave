using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Domain.Repositories;
using Shopwave.Modules.Identity.Infrastructure.Persistence;

using RefreshTokenEntity = Shopwave.Modules.Identity.Domain.Entities.RefreshToken;

namespace Shopwave.Modules.Identity.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context;

    public RefreshTokenRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task SaveAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _context.RefreshToken.AddAsync(token, ct);
    }
    
    public async Task<RefreshToken?> FindByTokenStringAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshToken
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }
    
    public async Task<IEnumerable<RefreshToken>> FindUserTokens(Guid userId, CancellationToken ct = default)
    {
        return await _context.RefreshToken
            .Where(rt => rt.UserId == userId)
            .ToListAsync(ct);
    }
    
    public Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
       _context.RefreshToken.Update(token);
       return Task.CompletedTask;
    }

}