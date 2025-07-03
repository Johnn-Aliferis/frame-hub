using FrameHub.Model.Entities;
using FrameHub.Profiles;

namespace FrameHub.Extensions;

public static class AutoMapperExtensions
{
    public static void AddCustomAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserSubscription));
        services.AddAutoMapper(typeof(Photo));
    }
}