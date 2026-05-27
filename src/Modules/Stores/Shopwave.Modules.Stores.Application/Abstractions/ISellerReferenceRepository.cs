using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Application.Abstractions;

public interface ISellerReferenceRepository
{
    Task AddSellerReferenceAsync(SellerReference SellerReference, CancellationToken ct = default);

    Task<SellerReference?> GetSellerReferenceAsync(Guid SellerId, CancellationToken ct = default);

    Task<bool> VerifySellerReferenceAsync(Guid SellerId, CancellationToken ct = default);
}