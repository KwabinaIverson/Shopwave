namespace Shopwave.Modules.Stores.Application.Abstractions;

public interface IStoreUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}