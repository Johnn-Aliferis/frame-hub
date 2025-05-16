using FrameHub.Model.Dto.Sso;

namespace FrameHub.Service.Interfaces;

public interface ISsoService
{
    Task<AuthResponseDto> HandleCallbackAsync(string provider, string code);
}