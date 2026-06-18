using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod.Responses;

namespace Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod;

public record GetStorePayoutMethodQuery(Guid  StoreId) : IQuery<Result<GetStorePayoutMethodResponse>>;