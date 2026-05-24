using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.API.Endpoints.Stores;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores/documents")
            .WithTags("Store Documents");

        // [Authorize]
        group.MapPost("upload-private", async (
                IFormFile file, 
                IObjectStorageService storageService, 
                CancellationToken ct) =>
            {
                if (file is null || file.Length == 0)
                {
                    return Results.BadRequest("No file was uploaded.");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return Results.BadRequest("Invalid file type.");
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream, ct);
                stream.Position = 0;

                var fileKey = await storageService.UploadPrivateFileAsync(
                    stream, 
                    file.FileName, 
                    file.ContentType, 
                    ct);

                return Results.Ok(new { FileKey = fileKey });
            })
            .DisableAntiforgery().RequireAuthorization();
    }
}