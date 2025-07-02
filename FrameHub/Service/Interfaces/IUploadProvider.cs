namespace FrameHub.Service.Interfaces;

public interface IUploadProvider
{
    Task<string> GeneratePresignedUrl(string userId, string fileName);
    Task DeleteMedia(string storageKey);
    string ProviderId { get; }
}