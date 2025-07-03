using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using FrameHub.Exceptions;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class AmazonS3Provider(IAmazonS3 amazonS3, ILogger logger) : IUploadProvider
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
            Verb = HttpVerb.PUT
        };

        try
        {
            return await amazonS3.GetPreSignedURLAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError("A Provider Exception occured during presigned url creation. Original message : {}", ex.Message);
            throw new ProviderException("Provider could not generate presigned Url", HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteMedia(string storageKey)
    {
        ValidateBucketName();

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey
        };
        try
        {
            await amazonS3.DeleteObjectAsync(deleteRequest);
        }
        catch (Exception ex)
        {
            logger.LogError("A Provider Exception occured during Deletion request. Original message : {}", ex.Message);
            throw new ProviderException("Provider could not complete deletion request", HttpStatusCode.InternalServerError);
        }
    }

    public string ProviderId => "AmazonS3";

    private void ValidateBucketName()
    {
        if (_bucketName is null)
        {
            throw new GeneralException("Environmental variable not set for S3 bucket",
                HttpStatusCode.InternalServerError);
        }
    }
}