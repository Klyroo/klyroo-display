using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blux.UI.Tools;

public static class LuxDisplayServiceCollectionExtensions
{
    public static IServiceCollection AddLuxDisplay(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddBluxUi();
        services.TryAddScoped<FilePreviewService>();
        return services;
    }
}
