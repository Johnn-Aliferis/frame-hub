using System.Net;
using FrameHub.ContextConfiguration;
using FrameHub.Exceptions;
using FrameHub.Extensions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Repository.Implementations;

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