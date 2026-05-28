using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.API.Endpoints.Stores;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores/documents")
            .WithTags("Store Documents");
        
        group.MapPost("upload-documents", async (
            HttpRequest request,
            IObjectStorageService storageService,
            CancellationToken ct) =>
        {
            var form = await request.ReadFormAsync(ct);
            var files = form.Files;

            if (files.Count == 0)
            {
                return Results.BadRequest("No files uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            
            // --- LOOP 1: STRICT PRE-VALIDATION ---
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return Results.BadRequest($"Invalid file type for '{file.FileName}'.");
                }
                
                var typeValues = form["documentTypes"]; 
                
                if (typeValues.Count <= i || string.IsNullOrWhiteSpace(typeValues[i]))
                {
                    return Results.BadRequest($"Missing document type for file '{file.FileName}'.");
                }

                if (!int.TryParse(typeValues[i], out _))
                {
                    return Results.BadRequest($"Invalid document type format for '{file.FileName}'.");
                }
            }

            var uploadedDocuments = new List<object>();
            
            // --- LOOP 2: EXECUTION ---
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                // Safe to parse because it passed validation in Loop 1
                var documentType = int.Parse(form["documentTypes"][i]!);

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream, ct);
                stream.Position = 0;

                var fileKey = await storageService.UploadPrivateFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    ct
                );

                uploadedDocuments.Add(new
                {
                    OriginalName = file.FileName,
                    FileKey = fileKey,
                    Type = documentType
                });
            }

            return Results.Ok(new { UploadedDocuments = uploadedDocuments });
        })
        .DisableAntiforgery()
        .RequireAuthorization(policy => policy.RequireRole("Seller"));
    }
}