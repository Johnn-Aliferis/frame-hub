using FrameHub.Modules.Auth.API.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Modules.Auth.Application.Services;

public interface ISsoService
{
    Task<LoginResponseDto> HandleCallbackAsync(AuthenticateResult result);
    
    SsoChallengeResultDto HandleSsoStart(string provider, IUrlHelper urlHelper);
}