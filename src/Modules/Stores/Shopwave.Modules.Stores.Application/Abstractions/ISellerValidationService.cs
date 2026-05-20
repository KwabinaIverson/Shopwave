namespace Shopwave.Modules.Stores.Application.Abstractions;

public interface ISellerValidationService
{
    Task<bool> IsValidSellerAsync(Guid OwnerId, CancellationToken ct = default);
}