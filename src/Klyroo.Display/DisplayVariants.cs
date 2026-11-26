using Microsoft.Extensions.Logging;

namespace Klyroo.Display;

/// <summary>The <see cref="KlyDisplay.Variant"/> values each <see cref="DisplayKind"/> knows. Kept next to the kinds' summaries; a test ties it to the markup.</summary>
internal static partial class DisplayVariants
{
    private static readonly Dictionary<DisplayKind, DisplayVariant[]> Known = new()
    {
        [DisplayKind.Heading] = [DisplayVariant.Eyebrow],
        [DisplayKind.Text] = [DisplayVariant.Quote],
        [DisplayKind.Stat] = [DisplayVariant.Row],
        [DisplayKind.Breakdown] = [DisplayVariant.Collapsible],
        [DisplayKind.Meter] = [DisplayVariant.Ring, DisplayVariant.Scale],
        [DisplayKind.Chart] = [DisplayVariant.Line, DisplayVariant.Funnel],
        [DisplayKind.Table] = [DisplayVariant.Sticky],
        [DisplayKind.Person] = [DisplayVariant.Group],
        [DisplayKind.List] = [DisplayVariant.Steps, DisplayVariant.Checks, DisplayVariant.Files],
        [DisplayKind.Timeline] = [DisplayVariant.Pipeline, DisplayVariant.Log, DisplayVariant.Schedule],
        [DisplayKind.Date] = [DisplayVariant.Countdown, DisplayVariant.Range],
        [DisplayKind.Media] = [DisplayVariant.Gallery],
        [DisplayKind.Code] = [DisplayVariant.Secret],
    };

    /// <summary>The variants of a kind (none for most kinds without variants), as the lowercase names the markup and the log use.</summary>
    public static IReadOnlyList<string> For(DisplayKind kind) =>
        Known.TryGetValue(kind, out var variants) ? variants.Select(v => v.ToString().ToLowerInvariant()).ToList() : [];

    /// <summary>Whether a variant is set and the kind does not have it (for example <see cref="DisplayVariant.Gallery"/> on a heading).</summary>
    public static bool IsUnknown(DisplayKind kind, DisplayVariant variant) =>
        variant != DisplayVariant.Default && !(Known.TryGetValue(kind, out var variants) && variants.Contains(variant));

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning,
        Message = "KlyDisplay: Variant '{Variant}' is not a variant of Kind {Kind}, so the default layout is rendered. Known variants: {Known}.")]
    public static partial void LogUnknownVariant(ILogger logger, string variant, DisplayKind kind, string known);
}
