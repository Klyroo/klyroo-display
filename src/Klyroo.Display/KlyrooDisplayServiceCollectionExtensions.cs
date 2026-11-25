using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klyroo.Display;

/// <summary>Service registration for Klyroo.Display.</summary>
public static class KlyrooDisplayServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FilePreviewService"/> as a scoped service and calls <c>AddKlyrooUi()</c>, which Klyroo.Display depends on.
    /// Call it once in <c>Program.cs</c>, and also in the client project of an Interactive WebAssembly or Auto app.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddKlyrooDisplay(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddKlyrooUi();
        services.TryAddScoped<FilePreviewService>();
        return services;
    }
}
