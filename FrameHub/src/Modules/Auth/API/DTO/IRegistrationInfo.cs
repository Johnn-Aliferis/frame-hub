namespace FrameHub.Modules.Auth.API.DTO;

public interface IRegistrationInfo
{
    public string DisplayName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}