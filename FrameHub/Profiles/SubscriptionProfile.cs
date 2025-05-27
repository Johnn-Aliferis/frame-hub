using AutoMapper;
using FrameHub.Model.Dto.Subscription;
using FrameHub.Model.Entities;

namespace FrameHub.Profiles;

public class SubscriptionProfile : Profile
{
    public SubscriptionProfile()
    {
        CreateMap<UserSubscription, UserSubscriptionDto>();
    }
}