using FrameHub.Modules.Media.Application.Service;
using FrameHub.Modules.Media.Domain.Entity;
using FrameHub.Modules.Shared.Extensions;
using FrameHub.Modules.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Modules.Media.Infrastructure.Repository;

public class PhotoRepository(AppDbContext context) : IPhotoRepository
{
    private readonly DbSet<Photo> _photos = context.Set<Photo>();
    
    public async Task<int> FindCountOfPhotosByUser(string userId)
    {
        return await _photos.Where(photo => photo.UserId == userId)
            .CountAsync();
    }

    public async Task<Photo?> FindPhotoById(long photoId)
    {
        return await _photos.FindActiveByIdAsync(photoId);
    }

    public async Task<Photo> SavePhotoAsync(Photo photo)
    {
        await _photos.AddAsync(photo);
        await context.SaveChangesAsync();
        return photo;
    }

    public async Task<Photo?> FindPhotoUserIdByStorageKeyAndProvider(string storageKey, string provider)
    {
        return await _photos
            .Where(photo => photo.StorageKey == storageKey && photo.Provider == provider)
            .FirstOrDefaultAsync();
    }
}