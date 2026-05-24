namespace Shopwave.Modules.Stores.Application.Abstractions;

public interface IObjectStorageService
{
    // Returns the permanent public URL (e.g., https://pub-xxx.r2.dev/logo.png)
    Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    
    // Returns just the file key (e.g., passport-1234.jpg). No public URL exists!
    Task<string> UploadPrivateFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    
    // Generates a temporary, self-destructing URL for admins to view private files
    Task<string> GetPreSignedUrlAsync(string fileKey, TimeSpan expiration);
}