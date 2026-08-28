namespace BLOBby.Services.Interfaces;

public interface IBucketService
{
    Task<bool> CreateBucketAsync(string bucketName);
    Task<bool> DeleteBucketAsync(string bucketName);
    Task<string> SaveObjectAsync(string bucketName, string objectKey, Stream stream, string contentType);
    Task<(Stream stream, string contentType)?> GetObjectAsync(string bucketName, string objectKey);
}