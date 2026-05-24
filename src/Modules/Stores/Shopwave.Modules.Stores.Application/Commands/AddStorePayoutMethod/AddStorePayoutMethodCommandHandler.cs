using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Domain.Repositories;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.Modules.Stores.Application.Commands.AddStorePayoutMethod;

internal sealed class AddStorePayoutMethodCommandHandler : ICommandHandler<AddStorePayoutMethodCommand, Result<Guid>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly IMediator _mediator;
    private readonly IStoreUnitOfWork _unitOfWork;

    public AddStorePayoutMethodCommandHandler(IStoreRepository storeRepository, IMediator mediator,
        IStoreUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<Guid>> Handle(AddStorePayoutMethodCommand request, CancellationToken ct)
    {
        if (request.StoreId == Guid.Empty)
            return Result.Failure<Guid>("StoreId cannot be empty.");
        
        var store = await _storeRepository.GetByIdAsync(request.StoreId, ct);

        if (store == null)
            return Result.Failure<Guid>("Store not found.");
        
        if (store.OwnerId != request.CurrentUserId)
            return Result.Failure<Guid>("You do not have permission to modify this store.");
        
        var newPayoutMethod = store.AddPayoutMethod(
            request.Type, 
            request.Provider, 
            request.AccountName, 
            request.AccountIdentifier
            );
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        foreach (var domainEvent in store.DomainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }

        store.ClearDomainEvents();
        
        return Result.Success(store.Id);
    }
}