using Shopwave.Shared.Events.Identity;
using Shopwave.Shared.Abstractions;
using Shopwave.Modules.Stores.Application.Abstractions;
using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Application.EventHandlers;

public class OnSellerCreatedHandler : IDomainEventHandler<SellerCreatedEvent>
{
    private readonly ISellerReferenceRepository _sellerReferenceRepository;
    private readonly IStoreUnitOfWork _storeUnitOfWork;

    public OnSellerCreatedHandler(ISellerReferenceRepository sellerReferenceRepository, IStoreUnitOfWork storeUnitOfWork)
    {
        _sellerReferenceRepository = sellerReferenceRepository ?? throw new ArgumentNullException(nameof(sellerReferenceRepository));
        _storeUnitOfWork = storeUnitOfWork ?? throw new ArgumentNullException(nameof(storeUnitOfWork));
    }

    public async Task Handle(SellerCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var existingSeller = await _sellerReferenceRepository.VerifySellerReferenceAsync(domainEvent.SellerId, 
            cancellationToken);

        if (existingSeller)
        {
            return;
        }
        
        try
        {
            var sellerRef = SellerReference.Create(domainEvent.SellerId);
            
            await _sellerReferenceRepository.AddSellerReferenceAsync(sellerRef, cancellationToken);
            
            await _storeUnitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[FATAL ERROR] The handler crashed!");
            Console.WriteLine($"[MESSAGE] {ex.Message}");
            Console.WriteLine($"[INNER EXCEPTION] {ex.InnerException?.Message}");
            throw;
        }
    }
}