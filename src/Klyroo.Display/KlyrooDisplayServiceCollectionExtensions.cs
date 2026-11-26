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
    /// <param name="configure">
    /// Changes the options, such as the built-in texts of a locale with <see cref="KlyrooDisplayOptions.Texts"/>. The options are stored with
    /// the Klyroo.UI options, so a second call changes the same instance.
    /// </param>
    /// <returns>The same collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddKlyrooDisplay(this IServiceCollection services, Action<KlyrooDisplayOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddKlyrooUi(options => configure?.Invoke(options.Extension<KlyrooDisplayOptions>()));
        services.TryAddScoped<FilePreviewService>();
        return services;
    }
}
