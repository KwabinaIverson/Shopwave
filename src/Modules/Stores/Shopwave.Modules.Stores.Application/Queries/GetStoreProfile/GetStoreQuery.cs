using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Application.Queries.GetStoreProfile.Responses;

namespace Shopwave.Modules.Stores.Application.Queries.GetStoreProfile;

public record GetStoreQuery(Guid StoreId) : IQuery<Result<GetStoreResponse>>;