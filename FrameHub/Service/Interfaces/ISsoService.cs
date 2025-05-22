using FrameHub.Model.Dto.Login;
using FrameHub.Model.Dto.Sso;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace FrameHub.Service.Interfaces;

public interface ISsoService
{
    Task<LoginResponseDto> HandleCallbackAsync(AuthenticateResult result);
    
    SsoChallengeResultDto HandleSsoStart(string provider, IUrlHelper urlHelper);
}