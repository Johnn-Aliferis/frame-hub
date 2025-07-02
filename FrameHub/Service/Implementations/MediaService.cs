using System.Net;
using AutoMapper;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Media;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class MediaService(
    IMapper mapper,
    IUploadProvider uploadProvider,
    IUserRepository userRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPhotoRepository photoRepository) : IMediaService
{
    public async Task<string> GeneratePresignedUrl(string userId, string email)
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
        
        return await uploadProvider.GeneratePresignedUrl(userId);
    }

    public Task<string> DeleteImage(string url)
    {
        // validate with storage key , and also validate via DB that this userId corresponds to this image.
        // After successful validation , call provider for deletion.
        // Update our DB to remove this photo from our Users etc foreign keys ,
        // Finally return message deleted or failed to delete.
        throw new NotImplementedException();
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
}