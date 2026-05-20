using Shopwave.Shared.Results;
using Shopwave.Shared.Domain;
using Shopwave.Shared.Abstractions;
using Shopwave.Modules.Stores.Domain.Entities;
using Shopwave.Modules.Stores.Application.Abstractions;
using Shopwave.Modules.Stores.Domain.Repositories;

namespace Shopwave.Modules.Stores.Application.Commands.RegisterStore;

public class RegisterStoreCommandHandler : ICommandHandler<RegisterStoreCommand, Result<Guid>>
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ISellerValidationService _sellerValidationService;

    public RegisterStoreCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork, IMediator mediator,
        ISellerValidationService sellerValidationService)
    {
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _sellerValidationService = sellerValidationService ?? throw new ArgumentNullException(nameof(sellerValidationService));
    }

    public async Task<Result<Guid>> Handle(RegisterStoreCommand request, CancellationToken ct)
    {
        if (request.OwnerId == Guid.Empty)
            return Result.Failure<Guid>("Owner Id cannot be empty.");
        
        if (!await _sellerValidationService.IsValidSellerAsync(request.OwnerId, ct))
        {
            return Result.Failure<Guid>("User is not authorized to register a store.");
        }
        
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return Result.Failure<Guid>("Display name cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(request.Slug))
            return Result.Failure<Guid>("Slug cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(request.BusinessName))
            return Result.Failure<Guid>("Business name cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.StoreAddress.StreetAddress1))
            return Result.Failure<Guid>("Street address 1 cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.StoreAddress.City))
            return Result.Failure<Guid>("City cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.StoreAddress.StateProvinceRegion))
            return Result.Failure<Guid>("State/Province/Region cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.StoreAddress.Country))
            return Result.Failure<Guid>("Country cannot be empty.");

        var address = new Address(
            request.StoreAddress.StreetAddress1,
            request.StoreAddress.StreetAddress2,
            request.StoreAddress.City,
            request.StoreAddress.StateProvinceRegion,
            request.StoreAddress.Country,
            request.StoreAddress.PostalZipCode
            );

        var store = Store.Create(
            request.OwnerId,
            request.DisplayName,
            request.Slug,
            request.BusinessName,
            address
            );

        await _storeRepository.AddAsync(store);
        await _unitOfWork.SaveChangesAsync(ct);

        foreach (var domainEvent in store.DomainEvents)
            await _mediator.Publish(domainEvent, ct);

        store.ClearDomainEvents();
        
        return Result.Success(store.Id);
    }
}