using FrameHub.Modules.Media.Domain.Entities;
using FrameHub.Modules.Subscriptions.Domain.Entities;

namespace FrameHub.Modules.Shared.Extensions;

public static class AutoMapperExtensions
{
    public static void AddCustomAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserSubscription));
        services.AddAutoMapper(typeof(Photo));
    }
}