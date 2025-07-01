using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using FrameHub.Exceptions;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class AmazonS3Provider(IAmazonS3 amazonS3) : IUploadProvider
{
    private readonly string? _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
    public async Task<string> GeneratePresignedUrl(string userId)
    {
        if (_bucketName is null)
        {
            throw new GeneralException("Environmental variable not set for S3 bucket", HttpStatusCode.InternalServerError);
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = $"uploads/{userId}/{Guid.NewGuid()}",
            Expires = DateTime.UtcNow.AddMinutes(5),
            Verb =  HttpVerb.PUT 
        };
        
        return await amazonS3.GetPreSignedURLAsync(request);
    }

    public Task<string> DeleteMedia(string url)
    {
        throw new NotImplementedException();
    }
}