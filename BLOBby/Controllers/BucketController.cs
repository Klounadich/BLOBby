using BLOBby.Commands;
using BLOBby.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLOBby.Controllers;

[ApiController]
[Route("api/blob/bucket")]

public class BucketController : ControllerBase
{
    private readonly IBucketService _bucketService;

    public BucketController(IBucketService bucketService)
    {
        _bucketService = bucketService;
    }

    
    [HttpPost]
    public async Task<IActionResult> CreateBucket([FromBody] CreateBucketRequest request)
    {
        await _bucketService.CreateBucketAsync(request.BucketName);
        return Ok();
    }

    [HttpDelete("{bucketName}")]
    public async Task<IActionResult> DeleteBucket(string bucketName)
    {
        await _bucketService.DeleteBucketAsync(bucketName);
        return Ok();
    }

    
    [HttpPut("{bucketName}/objects/{objectKey}")]
    public async Task<IActionResult> UploadIntoBucket(
        string bucketName,
        string objectKey,
        IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        await _bucketService.SaveObjectAsync(bucketName, objectKey, stream, file.ContentType);
        return Ok();
    }

    [HttpGet("{bucketName}/objects/{objectKey}")]
    public async Task<IActionResult> GetFromBucket(string bucketName, string objectKey)
    {
        var result = await _bucketService.GetObjectAsync(bucketName, objectKey);
        if (result is null) return NotFound();

        var (stream, contentType) = result.Value;
        return File(stream, contentType);
    }
}