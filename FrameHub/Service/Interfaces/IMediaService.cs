using FrameHub.Model.Dto.Media;
using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Service.Interfaces;

public interface IMediaService
{
    Task<string> GeneratePresignedUrl(string userId, PresignedUrlRequestDto presignedUrlRequestDto);
    Task DeleteImage(string userId, long photoId);
    Task<PhotoResponseDto> ConfirmMediaUploadAsync(string userId, PhotoRequestDto photoRequest);
}