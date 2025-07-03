using FrameHub.Modules.Media.API.DTO;

namespace FrameHub.Modules.Media.Application.Service;

public interface IMediaService
{
    Task<string> GeneratePresignedUrl(string userId, PresignedUrlRequestDto presignedUrlRequestDto);
    Task DeleteImage(string userId, long photoId);
    Task<PhotoResponseDto> ConfirmMediaUploadAsync(string userId, PhotoRequestDto photoRequest);
}