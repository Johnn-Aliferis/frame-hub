using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Media;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class AmazonS3Provider(IAmazonS3 amazonS3) : IUploadProvider
{
    private readonly string? _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
    public async Task<string> GeneratePresignedUrl(string userId, string fileName)
    {
        ValidateBucketName();
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = $"uploads/{userId}/{Guid.NewGuid()}_{fileName}",
            Expires = DateTime.UtcNow.AddMinutes(5),
            Verb =  HttpVerb.PUT
        };
        
        return await amazonS3.GetPreSignedURLAsync(request);
    }

    public async Task DeleteMedia(string storageKey)
    {
        ValidateBucketName();
        
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey
        };
        
        await amazonS3.DeleteObjectAsync(deleteRequest);
    }

    public string ProviderId => "AmazonS3";

    private void ValidateBucketName()
    {
        if (_bucketName is null)
        {
            throw new GeneralException("Environmental variable not set for S3 bucket", HttpStatusCode.InternalServerError);
        }
    }
}