using Microsoft.Extensions.Logging;

namespace Klyroo.Display;

/// <summary>The <see cref="KlyDisplay.Variant"/> values each <see cref="DisplayKind"/> knows. Kept next to the kinds' summaries; a test ties it to the markup.</summary>
internal static partial class DisplayVariants
{
    private static readonly Dictionary<DisplayKind, string[]> Known = new()
    {
        [DisplayKind.Heading] = ["eyebrow"],
        [DisplayKind.Text] = ["quote"],
        [DisplayKind.Stat] = ["row"],
        [DisplayKind.Breakdown] = ["collapsible"],
        [DisplayKind.Meter] = ["ring", "scale"],
        [DisplayKind.Chart] = ["line", "funnel"],
        [DisplayKind.Table] = ["sticky"],
        [DisplayKind.Person] = ["group"],
        [DisplayKind.List] = ["steps", "checks", "files"],
        [DisplayKind.Timeline] = ["pipeline", "log", "schedule"],
        [DisplayKind.Date] = ["countdown", "range"],
        [DisplayKind.Media] = ["gallery"],
        [DisplayKind.Code] = ["secret"],
    };

    /// <summary>The variants of a kind (none for most kinds without variants).</summary>
    public static IReadOnlyList<string> For(DisplayKind kind) => Known.TryGetValue(kind, out var variants) ? variants : [];

    /// <summary>Whether a variant is set and the kind does not know it. Matching is case-insensitive, like the rendering.</summary>
    public static bool IsUnknown(DisplayKind kind, string? variant) =>
        !string.IsNullOrWhiteSpace(variant) && !For(kind).Contains(variant, StringComparer.OrdinalIgnoreCase);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning,
        Message = "KlyDisplay: Variant '{Variant}' is not a variant of Kind {Kind}, so the default layout is rendered. Known variants: {Known}.")]
    public static partial void LogUnknownVariant(ILogger logger, string variant, DisplayKind kind, string known);
}
