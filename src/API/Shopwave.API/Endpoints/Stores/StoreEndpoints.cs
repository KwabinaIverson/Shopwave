using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shopwave.Shared.Abstractions;
using Shopwave.API.Endpoints.Requests.Store.Requests;
using Shopwave.Modules.Stores.Application.Commands.RegisterStore;
using Shopwave.Shared.Results;

namespace Shopwave.API.Endpoints.Stores;

public static class StoreEndpoints
{
    public static void MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores")
            .WithTags("Stores");

        // [Authorize]
        group.MapPost("", async (
            [FromBody] RegisterStoreApiRequest request,
            IMediator mediator,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            // 1. Check for the modern "sub" claim, then the legacy NameIdentifier, then "id"
            var userIdString = user.FindFirstValue("sub") 
                               ?? user.FindFirstValue(ClaimTypes.NameIdentifier) 
                               ?? user.FindFirstValue("id");

            // 2. If it's still null, or it's not a Guid, log it and fail
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
            {
                Console.WriteLine("\n=== JWT EXTRACTION FAILED ===");
                Console.WriteLine($"Found ID String: {userIdString ?? "NULL"}");
                Console.WriteLine("Dumping all claims in this token:");
    
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type} | Value: {claim.Value}");
                }
                Console.WriteLine("===============================\n");

                return Results.Unauthorized();
            }

            var address = new StoreAddressCommand(
                StreetAddress1: request.StoreAddress.StreetAddress1,
                StreetAddress2: request.StoreAddress.StreetAddress2,
                City: request.StoreAddress.City,
                StateProvinceRegion: request.StoreAddress.StateProvinceRegion,
                Country: request.StoreAddress.Country,
                PostalZipCode: request.StoreAddress.PostalZipCode 
                );
            
            var command = new RegisterStoreCommand(
                OwnerId: currentUserId,
                DisplayName: request.DisplayName,
                Slug: request.Slug,
                BusinessName: request.BusinessName,
                address
            );
            
            var result = await mediator.Send<RegisterStoreCommand, Result<Guid>>(command, ct);

            if (result.IsFailure)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/api/stores/{result.Value}", new { storeId = result.Value });
        }).RequireAuthorization(policy => policy.RequireRole("Seller"));
    }
}