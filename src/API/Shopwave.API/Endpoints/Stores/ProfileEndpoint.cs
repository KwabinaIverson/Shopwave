using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopwave.Shared.Abstractions;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Application.Queries.GetStoreProfile;
using Shopwave.Modules.Stores.Application.Queries.GetStoreProfile.Responses;
using Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod;
using Shopwave.Modules.Stores.Application.Queries.GetStorePayoutMethod.Responses;

namespace Shopwave.API.Endpoints.Stores;

public static class StoreProfileEndpoints
{
    public static void MapStoreProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores")
            .WithTags("Store Profile");
        
        group.MapGet("{storeId:guid}", async (
                Guid storeId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetStoreQuery(storeId);
                
                var result =  await mediator.Query<GetStoreQuery, Result<GetStoreResponse>>(query, ct);
                
                if (result.IsFailure)
                {
                    if (result.Error == "Store.NotFound")
                    {
                        return Results.NotFound(new { error = result.Error });
                    }
                
                    if (result.Error == "Store.InvalidId")
                    {
                        return Results.BadRequest(new { error = result.Error });
                    }
                
                    return Results.BadRequest(new { error = result.Error });
                }
                
                return Results.Ok(result.Value);
            }).RequireAuthorization(policy => policy.RequireRole("Seller"));
        
        group.MapGet("{storeId:guid}/payout-method", async (
                Guid storeId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetStorePayoutMethodQuery(storeId);
                
                var result = await mediator.Query<GetStorePayoutMethodQuery, Result<GetStorePayoutMethodResponse>>(query, ct);
                
                if (result.IsFailure)
                {
                    if (result.Error == "PayoutMethod.NotFound")
                    {
                        return Results.NotFound(new { error = result.Error });
                    }

                    return Results.BadRequest(new { error = result.Error });
                }
                
                return Results.Ok(result.Value);
            }).RequireAuthorization(policy => policy.RequireRole("Seller"));
    }
}