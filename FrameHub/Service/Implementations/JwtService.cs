using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FrameHub.Exceptions;
using FrameHub.Options;
using FrameHub.Service.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FrameHub.Service.Implementations;

public class JwtService(IOptions<JwtSettingsOptions> settings) : IJwtService
{
    public string GenerateJwtToken(string userId, string email)
    {
        var jwtSettings = settings.Value;
        
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new GeneralException("JWT_SECRET environment variable is missing.", HttpStatusCode.InternalServerError);
        }
        
        if (settings is null)
        {
            throw new GeneralException("jwtSettings config is missing.", HttpStatusCode.InternalServerError);
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, jwtSettings.Issuer),
            new Claim(JwtRegisteredClaimNames.Aud, jwtSettings.Audience)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}