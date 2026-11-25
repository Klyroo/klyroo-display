using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klyroo.Display;

public static class KlyrooDisplayServiceCollectionExtensions
{
    public static IServiceCollection AddKlyrooDisplay(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddKlyrooUi();
        services.TryAddScoped<FilePreviewService>();
        return services;
    }
}
