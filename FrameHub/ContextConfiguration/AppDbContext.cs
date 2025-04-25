using System.Reflection;
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
}