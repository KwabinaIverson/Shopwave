using System.Data;
using Dapper;
using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Application.Abstractions;
using Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod.Responses;

namespace Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod;

internal sealed class GetStorePayoutMethodQueryHandler 
    : IQueryHandler<GetStorePayoutMethodQuery, Result<GetStorePayoutMethodResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetStorePayoutMethodQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<GetStorePayoutMethodResponse>> Handle(
        GetStorePayoutMethodQuery request, CancellationToken ct)
    {
        if (request.StoreId == Guid.Empty)
        {
            return Result.Failure<GetStorePayoutMethodResponse>(
                "Store.InvalidId: The provided Store ID is empty or invalid."
            );
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                ""Id"" AS PayoutId, 
                ""Provider"" AS Provider, 
                ""AccountName"" AS AccountName, 
                ""AccountIdentifier"" AS AccountIdentifier, 
                ""IsVerified"" AS IsVerified
            FROM store_payout_methods
            WHERE ""StoreId"" = @StoreId
            LIMIT 1;";
        
        var payoutMethod = await connection.QueryFirstOrDefaultAsync<GetStorePayoutMethodResponse>(
            sql, 
            new { StoreId = request.StoreId }
        );
        
        if (payoutMethod is null)
        {
            return Result.Failure<GetStorePayoutMethodResponse>(
                "PayoutMethod.NotFound: No payout method has been configured for this store."
            );
        }

        return Result.Success(payoutMethod);
    }
}