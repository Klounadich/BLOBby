namespace BLOBby.Services.Interfaces;

public interface IBucketService
{
    public Task<bool> CreateBucketAsync(string bucketName);
    public Task<bool> DeleteBucketAsync(string bucketName);
    
    public Task<bool> SaveObjectAsync(string bucketName, string objectKey , Stream stream, string contentType);
    public Task<bool> GetObjectAsync(string bucketName, string objectKey);
}