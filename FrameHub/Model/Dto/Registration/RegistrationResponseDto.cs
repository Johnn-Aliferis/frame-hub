namespace FrameHub.Model.Dto.Registration;

public class RegistrationResponseDto
{
    public required long Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string Status { get; set; }
    public DateTime? LastLogin { get; set; }
    public required Guid Guid { get; set; }
}