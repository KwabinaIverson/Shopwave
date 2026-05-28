using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shopwave.Shared.Abstractions;
using Shopwave.API.Endpoints.Requests.Store.Requests;
using Shopwave.Shared.Results;
using Shopwave.Modules.Stores.Application.Commands.DocumentVerification;

namespace Shopwave.API.Endpoints.Stores;

public static class VerificationEndpoints
{
    public static void MapVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores")
            .WithTags("Store Verification");

        // [Authorize]
        group.MapPost("{storeId:guid}/verify", async (
            Guid storeId,
            [FromBody] SubmitVerificationApiRequest request,
            IMediator mediator,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            _ = Guid.TryParse(userIdString, out Guid currentUserId);
            
            var command = new SubmitVerificationBundleCommand(
                StoreId: storeId,
                CurrentUserId: currentUserId,
                Documents: request.Documents
            );
            
            var result = await mediator.Send<SubmitVerificationBundleCommand, Result<Guid>>(command, ct);
            
            if (result.IsFailure)
            {
                if (result.Error != null && result.Error.Contains("NotFound"))
                {
                    return Results.NotFound(new { error = result.Error });
                }
                
                return Results.BadRequest(new { error = result.Error });
            }
            
            return Results.Ok(new { message = "Verification submitted successfully", storeId = result.Value });
        }).RequireAuthorization(policy => policy.RequireRole("Seller"));
    }
}