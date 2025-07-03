namespace FrameHub.Modules.Media.Application.Service;

public interface IUploadProvider
{
    Task<string> GeneratePresignedUrl(string userId, string fileName);
    Task DeleteMedia(string storageKey);
    string ProviderId { get; }
}