using FrameHub.Model.Entities;

namespace FrameHub.Repository.Interfaces;

public interface IPhotoRepository
{
    Task<int> FindCountOfPhotosByUser(string userId);
    
    Task<Photo> SavePhotoAsync(Photo photo);
}