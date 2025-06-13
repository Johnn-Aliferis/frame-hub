namespace FrameHub.Service.Interfaces;

public interface IUploadProvider
{
    Task<string> GeneratePresignedUrl(string url);
    Task<string> DeleteMedia(string url);
}