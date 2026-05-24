namespace Shopwave.Modules.Identity.Application.Abstractions;

public interface IIdentityUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}