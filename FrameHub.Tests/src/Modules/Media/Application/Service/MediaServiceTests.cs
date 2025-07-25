using System.Net;
using AutoMapper;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Media.API.DTO;
using FrameHub.Modules.Media.Application.Exception;
using FrameHub.Modules.Media.Application.Service;
using FrameHub.Modules.Media.Domain.Entities;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Domain.Entities;
using Moq;

namespace FrameHub.Tests.Modules.Media.Application.Service;

public class MediaServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUploadProvider> _uploadProviderMock ;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly MediaService _service;


    public MediaServiceTests()
    {
        _mapperMock = new Mock<IMapper>();
        _uploadProviderMock = new Mock<IUploadProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();

        _service = new MediaService(_mapperMock.Object, _uploadProviderMock.Object, _userRepositoryMock.Object
            ,_subscriptionPlanRepositoryMock.Object,_photoRepositoryMock.Object);
    }

    
    // Generate Pre-signed url Tests
    
    [Fact]
    public async Task GeneratePresignedUrlTest_WrongPlan()
    {
        const string userId = "test-user-id";
        var presignedUrlRequestDto = new PresignedUrlRequestDto
        {   
            FileName = "test-file-name"
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 1L,
        };
        
        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserIdAsync(userId))
            .ReturnsAsync(userSubscription);
        
        var exception = await Assert.ThrowsAsync<MediaException>(async () => 
            await _service.GeneratePresignedUrl(userId, presignedUrlRequestDto)
        );

        // Assertions
        Assert.Equal("Current plan does not support upload", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    
    [Fact]
    public async Task GeneratePresignedUrlTest_MaxUploadSizeExceeded()
    {
        const string userId = "test-user-id";
        var presignedUrlRequestDto = new PresignedUrlRequestDto
        {   
            FileName = "test-file-name"
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 2L,
        };
        
        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserIdAsync(userId))
            .ReturnsAsync(userSubscription);
        
        // Mock max uploads allowed
        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanMaxUploadsByIdAsync(userSubscription.SubscriptionPlanId))
            .ReturnsAsync(5);
        
        // Mock user's upload count
        _photoRepositoryMock.Setup(x => x.FindCountOfPhotosByUser(userId))
            .ReturnsAsync(7);
        
        var exception = await Assert.ThrowsAsync<MediaException>(async () => 
            await _service.GeneratePresignedUrl(userId, presignedUrlRequestDto)
        );

        // Assertions
        Assert.Equal("You have surpassed the allowed count of uploaded photos, please delete some and try again", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    
    [Fact]
    public async Task GeneratePresignedUrlTest_Success()
    {
        const string resultUrl = "test-presigned-url";
        const string userId = "test-user-id";
        var presignedUrlRequestDto = new PresignedUrlRequestDto
        {   
            FileName = "test-file-name"
        };

        var userSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = 2L,
        };
        
        _userRepositoryMock.Setup(x => x.FindUserSubscriptionByUserIdAsync(userId))
            .ReturnsAsync(userSubscription);
        
        // Mock max uploads allowed
        _subscriptionPlanRepositoryMock.Setup(x => x.FindSubscriptionPlanMaxUploadsByIdAsync(userSubscription.SubscriptionPlanId))
            .ReturnsAsync(5);
        
        // Mock user's upload count
        _photoRepositoryMock.Setup(x => x.FindCountOfPhotosByUser(userId))
            .ReturnsAsync(4);
        
        _uploadProviderMock.Setup(x => x.GeneratePresignedUrl(userId, presignedUrlRequestDto.FileName))
            .ReturnsAsync(resultUrl);
        
        var result = await _service.GeneratePresignedUrl(userId, presignedUrlRequestDto);

        Assert.NotNull(result);
        Assert.Equal(resultUrl, result);
    }
    
    
    
    // Delete Image Tests 
    
    [Fact]
    public async Task DeleteImage_ValidationFail_PhotoNotFound()
    {
        const string userId = "test-user-id";
        const long photoId = 1;
        
        Photo photo = null;
        
        _photoRepositoryMock.Setup(x => x.FindPhotoById(photoId))
            .ReturnsAsync(photo);
        
        var exception = await Assert.ThrowsAsync<MediaException>(async () => 
            await _service.DeleteImage(userId, photoId)
        );

        // Assertions
        Assert.Equal("Could not find photo in database.", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    [Fact]
    public async Task DeleteImage_ValidationFail_PhotoOfDifferentUser()
    {
        const string userId = "test-user-id";
        const long photoId = 1;
        
        var photo = new Photo
        {
            Id = photoId,
            UserId = "test-user-id-2",
            StorageKey = "test-storage-key",
            Provider = "test-provider",
            FileName = "test-file-name",
        };
        
        _photoRepositoryMock.Setup(x => x.FindPhotoById(photoId))
            .ReturnsAsync(photo);
        
        var exception = await Assert.ThrowsAsync<MediaException>(async () => 
            await _service.DeleteImage(userId, photoId)
        );

        // Assertions
        Assert.Equal("The provided image storage key is not associated with this user.", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    
    [Fact]
    public async Task DeleteImage_Success()
    {
        const string userId = "test-user-id";
        const long photoId = 1;
        
        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            StorageKey = "test-storage-key",
            Provider = "test-provider",
            FileName = "test-file-name",
        };
        
        _photoRepositoryMock.Setup(x => x.FindPhotoById(photoId))
            .ReturnsAsync(photo);
        
        _uploadProviderMock.Setup(x => x.DeleteMedia(photo.StorageKey))
            .Returns(Task.CompletedTask);
        

        await _service.DeleteImage(userId, photoId);
        
        _photoRepositoryMock.Verify(x => x.FindPhotoById(photoId), Times.Once);
        _uploadProviderMock.Verify(x => x.DeleteMedia(photo.StorageKey), Times.Once);
    }
    
    
    // Confirm Media upload tests.
    
    [Fact]
    public async Task ConfirmMediaUploadAsync_WrongStorageKey()
    {
        const string userId = "test-user-id";
        var photoRequestDto = new PhotoRequestDto
        {   
            StorageKey = "test-storage-key",
            FileName = "test-file-name",
        };
        
        var exception = await Assert.ThrowsAsync<MediaException>(async () => 
            await _service.ConfirmMediaUploadAsync(userId, photoRequestDto)
        );
        
        Assert.Equal("Wrong storage key provided", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.Status);
    }
    
    [Fact]
    public async Task ConfirmMediaUploadAsync_Success()
    {
        const string userId = "test-user-id";
        var photoRequestDto = new PhotoRequestDto
        {   
            StorageKey = $"uploads/{userId}/",
            FileName = "test-file-name",
        };
        
        var savedPhoto = new Photo
        {
            UserId = userId,
            FileName = photoRequestDto.FileName,
            StorageKey = photoRequestDto.StorageKey,
            Tags = photoRequestDto.Tags,
            IsProfilePicture = photoRequestDto.IsProfilePicture,
            Provider = "AmazonS3" 
        };

        var photoResponseDto = new PhotoResponseDto
        {
            Id = savedPhoto.Id,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Guid = savedPhoto.Guid,
        };
        
        _photoRepositoryMock.Setup(x => x.SavePhotoAsync(savedPhoto))
            .ReturnsAsync(savedPhoto);
        
        _mapperMock.Setup(mapper => mapper.Map<PhotoResponseDto>(It.IsAny<Photo>())).Returns(photoResponseDto);
        
        var result = await _service.ConfirmMediaUploadAsync(userId, photoRequestDto);
        
        Assert.Equal(photoResponseDto.Id, result.Id);
    }
}