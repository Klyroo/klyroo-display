using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lux.Display;

public static class LuxDisplayServiceCollectionExtensions
{
    public static IServiceCollection AddLuxDisplay(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLuxUi();
        services.TryAddScoped<FilePreviewService>();
        return services;
    }
}
