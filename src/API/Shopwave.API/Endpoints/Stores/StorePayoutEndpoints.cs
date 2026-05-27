using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shopwave.Shared.Abstractions;
using Shopwave.API.Endpoints.Requests.Store.Requests;
using Shopwave.Modules.Stores.Application.Commands.AddStorePayoutMethod;
using Shopwave.Shared.Results;

namespace Shopwave.API.Endpoints.Stores;

public static class StorePayoutEndpoints
{
    public static void MapStorePayoutEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores")
            .WithTags("Store Payouts");

        // [Authorize]
        group.MapPost("{storeId}/payout-methods", async (
            Guid storeId,
            [FromBody] AddStorePayoutMethodApiRequest request,
            IMediator mediator,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Results.Unauthorized();
            }
            
            var command = new AddStorePayoutMethodCommand(
                StoreId: storeId,
                CurrentUserId: currentUserId,
                Type: request.Type,
                Provider: request.Provider,
                AccountName: request.AccountName,
                AccountIdentifier: request.AccountIdentifier
            );
            
            var result = await mediator.Send<AddStorePayoutMethodCommand, Result<Guid>>(command, ct);

            if (result.IsFailure)
            {
                if (result.Error != null && result.Error.Contains("NotFound"))
                {
                    return Results.NotFound(new { error = result.Error });
                }

                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/api/stores/{storeId}/payout-methods/{result.Value}", new { payoutMethodId = result.Value });
        }).RequireAuthorization();
    }
}