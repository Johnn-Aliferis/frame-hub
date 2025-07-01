using System.Net;
using FrameHub.Enum;
using FrameHub.Exceptions;
using FrameHub.Model.Dto.Media;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class MediaService(
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
        // get identification from UI , also userId from JWT ,
        // Of course try to see if user has this specific photo associated with him
        // After successful validation , call provider for deletion.
        // Update our DB to remove this photo from our Users etc foreign keys ,
        // Finally return message deleted or failed to delete.
        throw new NotImplementedException();
    }

    public Task<string> ConfirmMediaUploadAsync(string userId, PhotoRequestDto photoRequestDto)
    {
        // Get confirmation or failure from front end. -- Front end only calls if successful.
        // If true persist to db foreign key to user etc so as to make the relationships.
        // Return created.
        
        
        // Also , figure out how do we get the active provider, i.e runtime etc? and save it here too.
        // Create necessary relationships etc.
        // validate that this confirmation tries to alter the users correct resources etc
        throw new NotImplementedException();
    }
}