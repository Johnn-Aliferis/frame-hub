using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Extensions;

public static class DatabaseSetExtensions
{
    public static async Task<T?> FindActiveByIdAsync<T>(this DbSet<T> dbSet, long id) where T : BaseEntity
    {
        return await dbSet.FirstOrDefaultAsync(e => e.Id == id && e.Status);
    }
    
    public static async Task<T?> FindActiveByIdAsync<T>(this DbSet<T> dbSet, string id) where T : ApplicationUser
    {
        return await dbSet.FirstOrDefaultAsync(e => e.Id == id && e.Status);
    }
}