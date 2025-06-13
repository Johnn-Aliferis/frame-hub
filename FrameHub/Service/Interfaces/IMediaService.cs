namespace FrameHub.Service.Interfaces;

public interface IMediaService
{
    Task<string> GeneratePresignedUrl(string url);
    Task<string> DeleteImage(string url);
    Task<string> ConfirmMediaUpload(string url);
}