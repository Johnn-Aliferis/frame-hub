using AutoMapper;
using FrameHub.Model.Dto.Registration;
using FrameHub.Model.Entities;

namespace FrameHub.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, RegistrationResponseDto>();
    }
}