using FrameHub.Model.Dto.Sso;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Service.Interfaces;

public interface ISsoService
{
    Task<AuthResponseDto> HandleCallbackAsync(string provider, string code);
    
    SsoChallengeResultDto HandleSsoStart(string provider, IUrlHelper urlHelper);
}