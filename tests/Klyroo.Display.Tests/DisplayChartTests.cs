using Bunit;
using Klyroo.UI;
using Xunit;

namespace Klyroo.Display.Tests;

// The Chart kind: bar and line are Klyroo.UI's KlyChart, funnel is drawn here.
public sealed class DisplayChartTests : BunitContext
{
    public DisplayChartTests()
    {
        Services.AddKlyrooDisplay();
    }

    private IRenderedComponent<KlyDisplay> Chart(string? variant, IReadOnlyList<DisplayChartPoint> data, Func<double, string>? format = null, string? title = "Revenue") =>
        Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Chart)
            .Add(x => x.Variant, variant)
            .Add(x => x.Title, title)
            .Add(x => x.ChartFormat, format)
            .Add(x => x.ChartData, data));

    [Fact]
    public void Bar_chart_is_a_KlyChart_with_axes_tooltip_and_a_data_table_toggle()
    {
        var cut = Chart(null, [new("Mon", 10), new("Tue", 20)]);

        Assert.NotNull(cut.Find("[data-slot=chart-bars] kly-chart"));
        Assert.NotEmpty(cut.FindAll("kly-chart text"));
        Assert.NotEmpty(cut.FindAll("kly-chart [data-slot=tips] [data-tip]"));
        Assert.NotNull(cut.Find("kly-chart details table"));
        // The description names the series (the title) and gives the values, not the title alone, which the heading already reads.
        var label = cut.Find("kly-chart [data-slot=plot]").GetAttribute("aria-label")!;
        Assert.Contains("Revenue", label);
        Assert.Contains("20", label);
        var rows = cut.FindAll("kly-chart table tbody tr");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Mon", rows[0].TextContent);
        Assert.Contains("10", rows[0].TextContent);
    }

    [Fact]
    public void Bar_chart_keeps_negative_and_zero_values_honest()
    {
        var cut = Chart(null, [new("A", -5), new("B", 0), new("C", 10)]);

        var heights = cut.FindAll("kly-chart svg rect[rx]").Select(r => double.Parse(r.GetAttribute("height")!, System.Globalization.CultureInfo.InvariantCulture)).ToList();
        // The negative value has a real bar as tall as half the positive one; zero is at most the 1px baseline hairline.
        Assert.DoesNotContain(heights, h => h < 0);
        Assert.True(heights[0] > 20, string.Join(",", heights));
        Assert.True(heights[1] <= 1, string.Join(",", heights));
        Assert.Equal(heights[2] / 2, heights[0], 3);
        var tips = cut.FindAll("kly-chart [data-tip]").Select(t => t.GetAttribute("data-text")).ToList();
        Assert.Contains("-5", tips[0]);
    }

    [Fact]
    public void Bar_chart_is_one_series_in_the_shared_tone_or_the_primary_color()
    {
        var mixed = Chart(null, [new("A", 1, KlyColor.Error), new("B", 2, KlyColor.Success), new("C", 3, KlyColor.Error)]);

        // One series, so the data table has one value column and screen readers hear no color names.
        Assert.Equal(["primary"], mixed.FindAll("kly-chart svg g[data-series]").Select(g => g.GetAttribute("data-color")));
        Assert.Equal(2, mixed.FindAll("kly-chart table thead th").Count);

        var single = Chart(null, [new("A", 1, KlyColor.Error), new("B", 2, KlyColor.Error)]);
        Assert.Equal("error", single.Find("kly-chart svg g[data-series]").GetAttribute("data-color"));
    }

    [Fact]
    public void A_funnel_ignores_non_finite_steps_and_scales_to_the_largest_positive_one()
    {
        var cut = Chart("funnel", [new("A", 0.8), new("B", 0.4), new("C", double.NaN), new("D", double.PositiveInfinity)]);

        var widths = cut.FindAll("[data-slot=funnel-step] [aria-hidden=true]").Select(b => b.GetAttribute("style")).ToList();
        Assert.Equal(["width:100%", "width:50%", "width:0%", "width:0%"], widths);
    }

    [Fact]
    public void Line_chart_is_exposed_to_assistive_technology_with_values()
    {
        var cut = Chart("line", [new("Mon", 0), new("Tue", 50), new("Wed", 100)], title: null);

        Assert.NotNull(cut.Find("[data-slot=chart-line] kly-chart"));
        var label = cut.Find("kly-chart [data-slot=plot]").GetAttribute("aria-label")!;
        Assert.Contains("Mon", label);
        Assert.Contains("Wed", label);
        Assert.Contains("100", label);
        Assert.Equal(3, cut.FindAll("kly-chart table tbody tr").Count);
        // Without a title, the series carries the localized default name.
        Assert.Contains("Value", cut.Find("kly-chart table").TextContent);
    }

    [Fact]
    public void ChartFormat_formats_axis_tooltip_table_and_funnel_values()
    {
        Func<double, string> euro = v => $"{v:0}EUR";

        var bars = Chart(null, [new("A", 12)], euro);
        Assert.Contains("12EUR", bars.Find("kly-chart [data-tip]").GetAttribute("data-text"));
        Assert.Contains("12EUR", bars.Find("kly-chart table").TextContent);
        Assert.Contains(bars.FindAll("kly-chart svg text"), t => t.TextContent.EndsWith("EUR"));

        var funnel = Chart("funnel", [new("Visits", 100), new("Signups", 25)], euro);
        Assert.Contains("100EUR", funnel.FindAll("[data-slot=funnel-step]")[0].TextContent);
        Assert.Contains("25EUR (25%)", funnel.FindAll("[data-slot=funnel-step]")[1].TextContent);
    }

    [Fact]
    public void Default_format_follows_the_format_culture()
    {
        var cut = Chart("funnel", [new("Visits", 12345.5)]);
        // Grouping and decimal separators of the default (English) culture.
        Assert.Contains("12,345.5", cut.Find("[data-slot=funnel-step]").TextContent);
    }

    [Fact]
    public void Funnel_zero_and_negative_steps_have_no_bar_and_positive_ones_stay_visible()
    {
        var cut = Chart("funnel", [new("Visits", 1000), new("Tiny", 1), new("None", 0), new("Refunds", -20)]);

        var widths = cut.FindAll("[data-slot=funnel-step] div.h-2").Select(d => d.GetAttribute("style")!).ToList();
        Assert.Equal("width:100%", widths[0]);
        Assert.Equal("width:2%", widths[1]);
        Assert.Equal("width:0%", widths[2]);
        Assert.Equal("width:0%", widths[3]);
    }

    [Fact]
    public void Chart_without_data_says_so()
    {
        var cut = Chart(null, []);
        Assert.Equal("No data", cut.Find("[data-slot=chart-empty]").TextContent);
    }
}
