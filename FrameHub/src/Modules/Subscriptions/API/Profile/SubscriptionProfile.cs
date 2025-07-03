using FrameHub.Modules.Subscriptions.API.DTO;
using FrameHub.Modules.Subscriptions.Domain.Entities;

namespace FrameHub.Modules.Subscriptions.API.Profile;

public class SubscriptionProfile : AutoMapper.Profile
{
    public SubscriptionProfile()
    {
        CreateMap<UserSubscription, UserSubscriptionDto>();
    }
}