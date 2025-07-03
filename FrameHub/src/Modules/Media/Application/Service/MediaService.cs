using System.Net;
using AutoMapper;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Media.API.DTO;
using FrameHub.Modules.Media.Application.Exception;
using FrameHub.Modules.Media.Domain.Entity;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Domain.Enum;

namespace FrameHub.Modules.Media.Application.Service;

public class MediaService(
    IMapper mapper,
    IUploadProvider uploadProvider,
    IUserRepository userRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPhotoRepository photoRepository) : IMediaService
{
    public async Task<string> GeneratePresignedUrl(string userId, PresignedUrlRequestDto presignedUrlRequestDto)
    {
        var userSubscription = await userRepository.FindUserSubscriptionByUserIdAsync(userId);
        if (userSubscription is null || userSubscription.SubscriptionPlanId == (long)SubscriptionPlanId.Basic)
        {
            throw new MediaException("Current plan does not support upload", HttpStatusCode.BadRequest);
        }
        
        var maxUploadsAllowed = await subscriptionPlanRepository.FindSubscriptionPlanMaxUploadsByIdAsync(userSubscription.SubscriptionPlanId);
        var photosUploaded = await photoRepository.FindCountOfPhotosByUser(userId);

        if (photosUploaded >= maxUploadsAllowed)
        {
            throw new MediaException("You have surpassed the allowed count of uploaded photos, please delete some and try again", HttpStatusCode.BadRequest);
        }
        // OPTIONAL : For future enhancment , add some sort of locking , to prevent abuse
        // OPTIONAL : For future enhancment , use cache mechanism to avoid unnecessary calls to find MaxUploadsAllowed.(i.e Redis)
        
        return await uploadProvider.GeneratePresignedUrl(userId, presignedUrlRequestDto.FileName);
    }

    public async Task DeleteImage(string userId, long photoId)
    {
        var photo = await photoRepository.FindPhotoById(photoId);
        ValidatePhotoDeletionRequest(photo, userId);
        await uploadProvider.DeleteMedia(photo.StorageKey);
    }

    public async Task<PhotoResponseDto> ConfirmMediaUploadAsync(string userId, PhotoRequestDto photoRequestDto)
    {
        if(!photoRequestDto.StorageKey.StartsWith($"uploads/{userId}/"))
        {
            throw new MediaException("Wrong storage key provided", HttpStatusCode.BadRequest);
        }

        var photo = new Photo
        {
            UserId = userId,
            FileName = photoRequestDto.FileName,
            StorageKey = photoRequestDto.StorageKey,
            Tags = photoRequestDto.Tags,
            IsProfilePicture = photoRequestDto.IsProfilePicture,
            Provider = uploadProvider.ProviderId // Dynamic , according to which provider is selected.
        };
        
        var savedPhoto = await photoRepository.SavePhotoAsync(photo);
        return  mapper.Map<PhotoResponseDto>(savedPhoto);
    }


    private static void ValidatePhotoDeletionRequest(Photo photo, string userId)
    {
        if (photo is null)
        {
            throw new MediaException("Could not find photo in database.", HttpStatusCode.BadRequest);
        }

        if (photo.UserId != userId)
        {
            throw new MediaException("The provided image storage key is not associated with this user.", HttpStatusCode.BadRequest);
        }
    }
}