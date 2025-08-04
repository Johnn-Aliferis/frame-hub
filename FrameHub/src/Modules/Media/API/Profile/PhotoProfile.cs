using FrameHub.Modules.Media.API.DTO;
using FrameHub.Modules.Media.Domain.Entities;

namespace FrameHub.Modules.Media.API.Profile;

public class PhotoProfile  : AutoMapper.Profile
{
    public PhotoProfile()
    {
        CreateMap<Photo, PhotoResponseDto>();
    }
}