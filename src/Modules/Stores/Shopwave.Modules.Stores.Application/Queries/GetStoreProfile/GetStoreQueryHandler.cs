using System.Data;
using Dapper;
using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Application.Queries.GetStoreProfile.Responses;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.Modules.Stores.Application.Queries.GetStoreProfile;

internal sealed class GetStoreQueryHandler : IQueryHandler<GetStoreQuery, Result<GetStoreResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetStoreQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<GetStoreResponse>> Handle(GetStoreQuery request, CancellationToken ct)
    {
        if (request.StoreId == Guid.Empty)
        {
            return Result.Failure<GetStoreResponse>(
                "Store.InvalidId: The provided Store ID is empty or invalid."
            );
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
               ""Id"" AS StoreId, 
                ""DisplayName"" AS DisplayName, 
                ""Slug"" AS Slug, 
                ""BusinessName"" AS BusinessName,
                ""Id"" AS AddressId, 
                street_address_1 AS StreetAddress1,
                street_address_2 AS StreetAddress2,
                city AS City,
                state_province_region AS StateProvinceRegion,
                country AS Country,
                postal_zip_code AS PostalZipCode
            FROM stores 
            WHERE ""Id"" = @StoreId
            LIMIT 1;";

        // Query the flat raw result using Dapper
        var rawResult = await connection.QueryFirstOrDefaultAsync<StoreRawResult>(
            sql, 
            new { StoreId = request.StoreId }
        );

        if (rawResult is null)
        {
            return Result.Failure<GetStoreResponse>(
                $"Store.NotFound: No store found with ID {request.StoreId}."
            );
        }

        // Map the flat result to your strictly-typed, nested CQRS Response
        var response = new GetStoreResponse(
            rawResult.StoreId,
            rawResult.DisplayName,
            rawResult.Slug,
            rawResult.BusinessName,
            new GetStoreAddressResponse(
                rawResult.AddressId,
                rawResult.StreetAddress1,
                rawResult.StreetAddress2,
                rawResult.City,
                rawResult.StateProvinceRegion,
                rawResult.Country,
                rawResult.PostalZipCode
            )
        );

        return Result.Success(response);
    }

	private record StoreRawResult(
        Guid StoreId, 
        string DisplayName, 
        string Slug, 
        string BusinessName, 
        Guid AddressId, 
        string StreetAddress1, 
        string? StreetAddress2, 
        string City, 
        string StateProvinceRegion, 
        string Country, 
        string? PostalZipCode
    );
}