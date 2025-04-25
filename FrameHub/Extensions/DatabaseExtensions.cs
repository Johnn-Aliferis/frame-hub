using FrameHub.ContextConfiguration;
using Microsoft.EntityFrameworkCore;

namespace FrameHub.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        var server = Environment.GetEnvironmentVariable("DB_SERVER");
        var db = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        
        var connectionString = $"Server={server};Database={db};User Id={user};Password={password};TrustServerCertificate=True;";
        
        // Register DB context
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}