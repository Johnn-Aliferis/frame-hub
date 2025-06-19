using Microsoft.AspNetCore.Mvc;

namespace FrameHub.Service.Interfaces;

public interface IMediaService
{
    Task<string> GeneratePresignedUrl(string userId, string userEmail);
    Task<string> DeleteImage(string url);
    Task<string> ConfirmMediaUpload(string url);
}