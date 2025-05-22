namespace FrameHub.Options;

public class JwtSettingsOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}