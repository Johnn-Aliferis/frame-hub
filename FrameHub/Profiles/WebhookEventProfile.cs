using AutoMapper;
using FrameHub.Model.Entities;
using Newtonsoft.Json;
using Stripe;

namespace FrameHub.Profiles;

public class WebhookEventProfile : Profile
{

    public WebhookEventProfile()
    {
        CreateMap<Event, WebhookEvent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EventId, 
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.EventType, 
                opt => opt.MapFrom(src =>src.Type))
            .ForMember(dest => dest.CustomerEmail, 
                opt => opt.MapFrom(src => (src.Data.Object as Invoice)!.CustomerEmail))
            .ForMember(dest => dest.RawPayload, 
                opt => opt.MapFrom(src => JsonConvert.SerializeObject(src)));
        
    }
}