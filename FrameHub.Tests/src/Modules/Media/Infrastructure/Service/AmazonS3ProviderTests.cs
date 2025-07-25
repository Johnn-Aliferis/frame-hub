using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using FrameHub.Modules.Media.Infrastructure.Service;
using FrameHub.Modules.Shared.Application.Exception;
using Microsoft.Extensions.Logging;
using Moq;

namespace FrameHub.Tests.Modules.Media.Infrastructure.Service;

public class AmazonS3ProviderTests
{
    private readonly Mock<IAmazonS3> _amazonS3Mock; 
    private readonly Mock<ILogger<AmazonS3Provider>> _loggerMock;
    private AmazonS3Provider _provider;


    public AmazonS3ProviderTests()
    {
        _amazonS3Mock = new Mock<IAmazonS3>();
        _loggerMock = new Mock<ILogger<AmazonS3Provider>>();
    }

    [Fact]
    public async Task GeneratePresignedUrl_NoBucketName()
    {
        _provider = new AmazonS3Provider(_amazonS3Mock.Object, _loggerMock.Object);
        const string userId = "test-user-id";
        const string fileName = "test-file-name";
        Environment.SetEnvironmentVariable("S3_BUCKET_NAME", null);

        var exception = await Assert.ThrowsAsync<GeneralException>(async () => 
            await _provider.GeneratePresignedUrl(userId, fileName)
        );
        
        Assert.Equal("Environmental variable not set for S3 bucket", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }
    
    [Fact]
    public async Task DeleteMedia_NoBucketName()
    {
        _provider = new AmazonS3Provider(_amazonS3Mock.Object, _loggerMock.Object);
        const string storageKey = "test-storage-key";

        var exception = await Assert.ThrowsAsync<GeneralException>(async () => 
            await _provider.DeleteMedia(storageKey)
        );
        
        Assert.Equal("Environmental variable not set for S3 bucket", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.Status);
    }
    
    [Fact]
    public async Task GeneratePresignedUrl_Success()
    {
        Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket-name");
        _provider = new AmazonS3Provider(_amazonS3Mock.Object, _loggerMock.Object);
        
        const string userId = "test-user-id";
        const string fileName = "test-file-name";
        
        _amazonS3Mock.Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync("test-url");

        var result = await _provider.GeneratePresignedUrl(userId, fileName);
        
        Assert.Equal("test-url", result);
    }
    
    
    [Fact]
    public async Task DeleteMedia_Success()
    {
        Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket-name");
        _provider = new AmazonS3Provider(_amazonS3Mock.Object, _loggerMock.Object);

        var objectResponse = new DeleteObjectResponse();
        const string storageKey = "test-storage-key";
        
        _amazonS3Mock.Setup(x =>
                x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(objectResponse);

        await _provider.DeleteMedia(storageKey);
        _amazonS3Mock.Verify(x => 
            x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}