using FrameHub.Service.Interfaces;

namespace FrameHub.Service.Implementations;

public class AmazonS3Provider : IUploadProvider
{
    public Task<string> GeneratePresignedUrl(string url)
    {
        throw new NotImplementedException();
    }

    public Task<string> DeleteMedia(string url)
    {
        throw new NotImplementedException();
    }
}