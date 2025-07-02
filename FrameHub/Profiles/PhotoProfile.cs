using AutoMapper;
using FrameHub.Model.Dto.Media;
using FrameHub.Model.Entities;

namespace FrameHub.Profiles;

public class PhotoProfile  : Profile
{
    public PhotoProfile()
    {
        CreateMap<Photo, PhotoResponseDto>();
    }
}