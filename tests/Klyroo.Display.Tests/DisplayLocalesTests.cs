using System.Reflection;
using Bunit;
using Klyroo.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

// The built-in texts follow the locale of Klyroo.UI, one set per locale, each changeable on its own.
public sealed class DisplayLocalesTests : BunitContext
{
    public DisplayLocalesTests()
    {
        Services.AddKlyrooDisplay();
    }

    private IRenderedComponent<CascadingValue<KlyLocale>> RenderList(KlyLocale locale) =>
        Render<CascadingValue<KlyLocale>>(p => p
            .Add(x => x.Value, locale)
            .AddChildContent<KlyDisplay>(d => d
                .Add(x => x.Kind, DisplayKind.List)
                .Add(x => x.Max, 1)
                .Add(x => x.ListItems, [new DisplayListItem { Text = "a" }, new DisplayListItem { Text = "b" }])));

    [Fact]
    public void A_component_shows_the_texts_of_its_locale()
    {
        Assert.Equal("Afficher plus · 1", RenderList(KlyLocales.Fr).Find("[data-slot=display-more]").TextContent.Trim());
        Assert.Equal("Mehr anzeigen · 1", RenderList(KlyLocales.De).Find("[data-slot=display-more]").TextContent.Trim());
        Assert.Equal("Show more · 1", RenderList(KlyLocales.En).Find("[data-slot=display-more]").TextContent.Trim());
    }

    [Fact]
    public void A_change_applies_to_its_locale_and_to_the_locales_that_resolve_to_it()
    {
        Services.AddKlyrooDisplay(d => d.Texts("fr", t => t with { ShowMore = n => $"Encore {n}" }));
        var canadian = KlyLocales.Extend(KlyLocales.Fr, code: "fr-CA", name: "Français (Canada)");

        Assert.Equal("Encore 1", RenderList(KlyLocales.Fr).Find("[data-slot=display-more]").TextContent.Trim());
        Assert.Equal("Encore 1", RenderList(canadian).Find("[data-slot=display-more]").TextContent.Trim());
        Assert.Equal("Show more · 1", RenderList(KlyLocales.En).Find("[data-slot=display-more]").TextContent.Trim());
    }

    [Fact]
    public void Texts_cannot_change_once_a_component_resolved_one()
    {
        RenderList(KlyLocales.En);
        var options = Services.GetRequiredService<KlyrooOptions>().Extension<KlyrooDisplayOptions>();

        Assert.Throws<InvalidOperationException>(() => options.Texts("en", t => t));
    }

    [Fact]
    public void Every_locale_of_Klyroo_UI_has_a_complete_set()
    {
        Assert.Equal(KlyLocales.All.Select(l => l.Code).Order(StringComparer.OrdinalIgnoreCase), DisplayLocales.Tables.Keys.Order(StringComparer.OrdinalIgnoreCase));
        foreach (var (code, texts) in DisplayLocales.Tables)
        {
            foreach (var property in typeof(DisplayTexts).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(texts);
                var shown = value switch
                {
                    string s => new[] { s },
                    Func<int, string> f => new[] { f(0), f(1), f(2), f(5), f(21), f(1500) },
                    Func<string, string> f => new[] { f("Office") },
                    Func<int, int, string, string> f => new[] { f(1, 1, "tab"), f(2, 12, ";") },
                    _ => throw new InvalidOperationException($"{property.Name} has an unexpected type."),
                };
                Assert.All(shown, s => Assert.False(string.IsNullOrWhiteSpace(s), $"{code}.{property.Name}"));
            }
            if (code != "en")
            {
                // A translated set does not fall back to the English text of a label.
                Assert.NotEqual(DisplayLocales.En.NoPlaysLeft, texts.NoPlaysLeft);
                Assert.NotEqual(DisplayLocales.En.NoPreview, texts.NoPreview);
            }
        }
    }

    [Theory]
    [InlineData(1, "день")]
    [InlineData(21, "день")]
    [InlineData(2, "дня")]
    [InlineData(24, "дня")]
    [InlineData(5, "дней")]
    [InlineData(11, "дней")]
    [InlineData(12, "дней")]
    [InlineData(14, "дней")]
    [InlineData(111, "дней")]
    public void Russian_counts_take_their_plural_form(int n, string word) =>
        Assert.Equal($"{n} {word}", DisplayLocales.Ru.DayCount(n));

    [Fact]
    public void Counts_group_thousands_the_way_the_language_writes_them()
    {
        Assert.Equal("Showing first 1,000 rows", DisplayLocales.En.CsvTruncated(1000));
        Assert.Equal("Affichage des 1 000 premières lignes", DisplayLocales.Fr.CsvTruncated(1000));
        Assert.Equal("Die ersten 1.000 Zeilen werden angezeigt", DisplayLocales.De.CsvTruncated(1000));
        Assert.Equal("1 ligne · 3 colonnes · séparateur « tabulation »", DisplayLocales.Fr.CsvSummary(1, 3, "tab"));
        Assert.Equal("1 row · 2 columns · delimiter ','", DisplayLocales.En.CsvSummary(1, 2, ","));
    }
}
