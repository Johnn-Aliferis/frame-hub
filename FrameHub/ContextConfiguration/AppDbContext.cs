using System.Reflection;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.ContextConfiguration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var entityTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, Namespace: "FrameHub.Model.Entities" });

        // Reflection to dynamically register entities
        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType);
        }

        // Apply configurations via IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
        // For each entity modified , update updated_at column to date time now.utc
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
    
            return await base.SaveChangesAsync(cancellationToken);
        }
}