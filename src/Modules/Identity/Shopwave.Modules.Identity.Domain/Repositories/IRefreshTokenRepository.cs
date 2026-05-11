using Shopwave.Modules.Identity.Domain.Entities;

namespace Shopwave.Modules.Identity.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken token, CancellationToken cancellationToken = default);
    
    Task<RefreshToken?> FindByTokenStringAsync(string token, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<RefreshToken>> FindUserTokens(Guid userId, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
}