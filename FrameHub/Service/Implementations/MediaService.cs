using FrameHub.Enum;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class MediaService(IUploadProvider uploadProvider, IUserRepository userRepository) : IMediaService
{
    public async Task<string> GeneratePresignedUrl(string userId, string email)
    {
        // Algorithm : 
        // 1) Check user plan , current upload count , and if he can upload yet.
        var userSubscription = await  userRepository.FindUserSubscriptionByUserIdAsync(userId);
        
        if (userSubscription is null || userSubscription.SubscriptionPlanId == (long)SubscriptionPlanId.Basic)
        {
            // todo : create the exception and handlers and move on according to algorithm.
            throw new MediaException();
        }
        // Next fpom the exception , check if lazy loading is enabled and get subscription plan to see his current plan.
        
        // Figure out how many uploads he gets to have from his plan -> i.e 5 .
        // Then perform a Count in our database , checking how many ACTIVE photo he currently has -- take into consideration status of ParentEntity.
        // If it is same or exceeds maximum , throw again exception.
        // If not , proceed with generation of presigned url .
        
        // OPTIONAL : For future enhancment , add some sort of locking , to prevent abuse.
        // 2) Generate pre-signed url from uploadProvider
        // 3) Persist row in DB with status pending-> do we get here image name or some id? 
        // 4) return the presigned url and await confirmation from front end. 
        
        throw new NotImplementedException();
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

    public Task<string> ConfirmMediaUpload(string url)
    {
        // Get confirmation or failure from front end.
        // it's either true or false , so we update db as uploaded or failed accordingly.
        // If true persist to db foreign key to user etc so as to make the relationships.
        // Return ok ? with message 
        throw new NotImplementedException();
    }
}