using Shopwave.Shared.Results;
using Shopwave.Shared.Domain;
using Shopwave.Shared.Abstractions;
using Shopwave.Modules.Stores.Domain.Repositories;
using Shopwave.Modules.Stores.Domain.Enums;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.Modules.Stores.Application.Commands.DocumentVerification;

 internal class SubmitVerificationBundleCommandHandler : ICommandHandler<SubmitVerificationBundleCommand, Result<Guid>>
 {
     private readonly IStoreRepository _storeRepository;
     private readonly IStoreUnitOfWork _unitOfWork;
     private readonly IMediator _mediator;
     
     public SubmitVerificationBundleCommandHandler(IStoreRepository storeRepository, IStoreUnitOfWork unitOfWork, IMediator mediator)
     {
         _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
         _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
         _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
     }

     public async Task<Result<Guid>> Handle(SubmitVerificationBundleCommand request, CancellationToken ct)
     {
         if (request.StoreId == Guid.Empty)
         {
             return Result.Failure<Guid>("Store ID cannot be empty.");
         }
         
         if (request.CurrentUserId == Guid.Empty)
         {
             return Result.Failure<Guid>("Current user ID cannot be empty.");
         }
         
         if (request.Documents == null || request.Documents.Count == 0)
         {
             return Result.Failure<Guid>("At least one document must be submitted for verification.");
         } 
         
         var store = await _storeRepository.GetByIdAsync(request.StoreId, ct);
         if (store is null)
         {
             return Result.Failure<Guid>("The specified store was not found.");
         }
         
         if (store.OwnerId != request.CurrentUserId)
         {
             return Result.Failure<Guid>("You do not own this store.");
         }
         
         var nationalIdDoc = request.Documents.FirstOrDefault(d => d.Type == DocumentType.NationalIdentityCard);
         var businessRegDoc = request.Documents.FirstOrDefault(d => d.Type == DocumentType.BusinessRegistration);
         
         if (nationalIdDoc is null || businessRegDoc is null)
         {
             return Result.Failure<Guid>("Both a National Identity Card and Business Registration are required.");
         }
         
         try
         {
             store.SubmitForVerification(
                 taxDocumentUrl: nationalIdDoc.FileUrl,
                 registrationDocumentUrl: businessRegDoc.FileUrl);
         }
         catch (InvalidOperationException ex)
         {
             return Result.Failure<Guid>(ex.Message);
         }
         
         await _unitOfWork.SaveChangesAsync(ct);
         return Result.Success(store.Id);
     }
 }