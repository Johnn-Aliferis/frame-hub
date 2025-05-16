using FrameHub.Model.Dto.Sso;

namespace FrameHub.Service.Interfaces;

public interface ISsoProviderStrategy
{
    Task<UserInfoSsoResponseDto> GetUserInfoAsync();
}