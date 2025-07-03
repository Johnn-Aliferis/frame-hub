using FrameHub.Modules.Media.API.DTO;
using FrameHub.Modules.Media.Domain.Entity;

namespace FrameHub.Modules.Media.API.Profile;

public class PhotoProfile  : AutoMapper.Profile
{
    public PhotoProfile()
    {
        CreateMap<Photo, PhotoResponseDto>();
    }
}