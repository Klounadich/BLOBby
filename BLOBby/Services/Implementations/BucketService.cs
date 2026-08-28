using BLOBby.Services.Interfaces;

namespace BLOBby.Services.Implementations;

public class BucketService : IBucketService
{
    private readonly string _blobPath;
    private readonly ILogger<BucketService> _logger;

    public BucketService(IConfiguration configuration, ILogger<BucketService> logger)
    {
        _logger = logger;
        _blobPath = configuration.GetSection("BLOBby:BlobPath").Value
            ?? throw new InvalidOperationException("BLOBby:BlobPath is not configured");
    }

    
    private static bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.Contains("..") || name.Contains('/') || name.Contains('\\')) return false;
        return name.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.');
    }

    public Task<bool> CreateBucketAsync(string bucketName)
    {
        if (!IsValidName(bucketName)) return Task.FromResult(false);

        try
        {
            var bucketPath = Path.Combine(_blobPath, bucketName);
            Directory.CreateDirectory(bucketPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create bucket {BucketName}", bucketName);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeleteBucketAsync(string bucketName)
    {
        if (!IsValidName(bucketName)) return Task.FromResult(false);

        try
        {
            var bucketPath = Path.Combine(_blobPath, bucketName);
            if (!Directory.Exists(bucketPath)) return Task.FromResult(false);

            Directory.Delete(bucketPath, true);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete bucket {BucketName}", bucketName);
            return Task.FromResult(false);
        }
    }

    public async Task<string> SaveObjectAsync(string bucketName, string objectKey, Stream stream, string contentType)
    {
        if (!IsValidName(bucketName) || !IsValidName(objectKey))
            throw new ArgumentException("Invalid bucket name or object key");

        string pathToBucket = Path.Combine(_blobPath, bucketName);
        if (!Directory.Exists(pathToBucket))
            throw new InvalidOperationException("Bucket doesn't exist");

        string pathToFile = Path.Combine(pathToBucket, objectKey);

        await using (var fileStream = new FileStream(
            pathToFile, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true))
        {
            await stream.CopyToAsync(fileStream);
        }

        
        await File.WriteAllTextAsync(pathToFile + ".meta", contentType);

        return $"https://178.236.243.241:7845/api/blob/bucket/{bucketName}/objects/{objectKey}";
    }

    public async Task<(Stream stream, string contentType)?> GetObjectAsync(string bucketName, string objectKey)
    {
        if (!IsValidName(bucketName) || !IsValidName(objectKey))
            return null;

        string pathToBucket = Path.Combine(_blobPath, bucketName);
        string pathToFile = Path.Combine(pathToBucket, objectKey);

        if (!File.Exists(pathToFile)) return null;

        string contentType = "application/octet-stream";
        string metaPath = pathToFile + ".meta";
        if (File.Exists(metaPath))
            contentType = await File.ReadAllTextAsync(metaPath);

        var stream = File.Open(pathToFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, contentType);
    }
}