namespace FrameHub.Service.Interfaces;

public interface IUploadProvider
{
    Task<string> GeneratePresignedUrl(string userId);
    Task<string> DeleteMedia(string url);
}