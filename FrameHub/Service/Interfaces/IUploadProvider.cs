namespace FrameHub.Service.Interfaces;

public interface IUploadProvider
{
    Task<string> GeneratePresignedUrl(string userId);
    Task DeleteMedia(string storageKey);
    string ProviderId { get; }
}