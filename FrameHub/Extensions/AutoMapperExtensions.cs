using FrameHub.Model.Entities;

namespace FrameHub.Extensions;

public static class AutoMapperExtensions
{
    public static void AddCustomAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(User));
    }
}