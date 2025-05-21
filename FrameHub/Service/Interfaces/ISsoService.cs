using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Sso;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Service.Interfaces;

public interface ISsoService
{
    Task<LoginResponseDto> HandleCallbackAsync(string provider, string code);
    
    SsoChallengeResultDto HandleSsoStart(string provider, IUrlHelper urlHelper);
}