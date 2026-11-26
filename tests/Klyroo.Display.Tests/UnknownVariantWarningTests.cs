using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Klyroo.Display.Tests;

// A variant the kind does not know renders the default layout; in Development that is worth a warning, because it is usually a typo.
public sealed class UnknownVariantWarningTests : BunitContext
{
    private sealed class Capture : ILoggerProvider, ILogger
    {
        public List<(LogLevel Level, string Category, string Message)> Entries { get; init; } = [];
        private string _category = "";
        public ILogger CreateLogger(string categoryName) => new Capture { _category = categoryName, Entries = Entries };
        public void Dispose() { }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, _category, formatter(state, exception)));
    }

    private readonly Capture _log = new();

    private void Setup(bool? warn)
    {
        Services.AddLogging(b => b.AddProvider(_log));
        Services.AddKlyrooDisplay(d => d.WarnOnUnknownVariant = warn);
    }

    private IRenderedComponent<KlyDisplay> Render(DisplayKind kind, string? variant) =>
        Render<KlyDisplay>(p => p.Add(x => x.Kind, kind).Add(x => x.Variant, variant).Add(x => x.Title, "T"));

    [Fact]
    public void An_unknown_variant_logs_a_warning_naming_the_kind_and_the_known_variants()
    {
        Setup(true);

        var cut = Render(DisplayKind.Chart, "linee");

        var entry = Assert.Single(_log.Entries, e => e.Level == LogLevel.Warning);
        Assert.Contains("'linee'", entry.Message);
        Assert.Contains("Chart", entry.Message);
        Assert.Contains("line, funnel", entry.Message);
        Assert.Equal(typeof(KlyDisplay).FullName, entry.Category);
        // The default layout still renders.
        Assert.NotNull(cut.Find("[data-slot=display-chart]"));
    }

    [Fact]
    public void A_kind_without_variants_says_none()
    {
        Setup(true);

        Render(DisplayKind.Alert, "fancy");

        Assert.Contains("Known variants: none", Assert.Single(_log.Entries).Message);
    }

    [Fact]
    public void A_known_variant_in_any_case_and_an_empty_variant_log_nothing()
    {
        Setup(true);

        Render(DisplayKind.Chart, "LINE");
        Render(DisplayKind.Chart, null);
        Render(DisplayKind.Chart, " ");

        Assert.Empty(_log.Entries);
    }

    [Fact]
    public void Every_documented_variant_is_accepted()
    {
        Setup(true);

        // The variants documented on the kinds and in the Variant parameter.
        (DisplayKind, string)[] documented =
        [
            (DisplayKind.Heading, "eyebrow"), (DisplayKind.Text, "quote"), (DisplayKind.Stat, "row"), (DisplayKind.Breakdown, "collapsible"),
            (DisplayKind.Meter, "ring"), (DisplayKind.Meter, "scale"), (DisplayKind.Chart, "line"), (DisplayKind.Chart, "funnel"),
            (DisplayKind.Table, "sticky"), (DisplayKind.Person, "group"), (DisplayKind.List, "steps"), (DisplayKind.List, "checks"),
            (DisplayKind.List, "files"), (DisplayKind.Timeline, "pipeline"), (DisplayKind.Timeline, "log"), (DisplayKind.Timeline, "schedule"),
            (DisplayKind.Date, "countdown"), (DisplayKind.Date, "range"), (DisplayKind.Media, "gallery"), (DisplayKind.Code, "secret"),
        ];

        foreach (var (kind, variant) in documented)
        {
            Render(kind, variant);
        }

        Assert.Empty(_log.Entries);
    }

    [Fact]
    public void The_same_unknown_variant_warns_once_per_component_and_a_change_warns_again()
    {
        Setup(true);
        var cut = Render(DisplayKind.Chart, "linee");

        cut.Render(p => p.Add(x => x.Title, "Other"));
        cut.Render(p => p.Add(x => x.Title, "More"));
        Assert.Single(_log.Entries);

        cut.Render(p => p.Add(x => x.Variant, "bars"));
        Assert.Equal(2, _log.Entries.Count);
    }

    [Fact]
    public void WarnOnUnknownVariant_false_keeps_it_quiet_even_in_Development()
    {
        var previous = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        try
        {
            Setup(false);
            Render(DisplayKind.Chart, "linee");
            Assert.Empty(_log.Entries);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previous);
        }
    }

    [Fact]
    public void By_default_the_warning_is_on_in_Development_and_off_otherwise()
    {
        var previous = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            Setup(null);
            Render(DisplayKind.Chart, "linee");
            Assert.Empty(_log.Entries);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Render(DisplayKind.Chart, "linee");
            Assert.Single(_log.Entries);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previous);
        }
    }
}
