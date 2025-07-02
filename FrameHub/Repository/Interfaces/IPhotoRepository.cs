using FrameHub.Model.Entities;

namespace FrameHub.Repository.Interfaces;

public interface IPhotoRepository
{
    Task<int> FindCountOfPhotosByUser(string userId);
    Task<Photo?> FindPhotoById(long photoId);
    
    Task<Photo> SavePhotoAsync(Photo photo);
    Task<Photo?> FindPhotoUserIdByStorageKeyAndProvider(string storageKey, string provider);
}