using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.Modules.Stores.Infrastructure.Repositories;

public class CloudflareR2StorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _publicBucket;
    private readonly string _privateBucket;
    private readonly string _publicBaseUrl;
    
    public CloudflareR2StorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _publicBucket = configuration["CloudflareR2:PublicBucketName"]!;
        _privateBucket = configuration["CloudflareR2:PrivateBucketName"]!;
        _publicBaseUrl = configuration["CloudflareR2:PublicDomain"]!;
    }
    
    public async Task<string> UploadPublicFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _publicBucket,
            Key = uniqueFileName,
            InputStream = fileStream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(request, ct);

        // Public files return the fully qualified domain name
        return $"{_publicBaseUrl}/{uniqueFileName}";
    }
    
    public async Task<string> UploadPrivateFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var uniqueFileName = $"documents/{Guid.NewGuid()}-{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _privateBucket,
            Key = uniqueFileName,
            InputStream = fileStream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(request, ct);

        // Private files ONLY return the key. They cannot be accessed directly via URL.
        return uniqueFileName; 
    }
    
    public Task<string> GetPreSignedUrlAsync(string fileKey, TimeSpan expiration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _privateBucket,
            Key = fileKey,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        // This doesn't make a network call; it just cryptographically signs a URL using your secret key
        string url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
}