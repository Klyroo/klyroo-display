using Bunit;
using Xunit;

namespace Klyroo.Display.Tests;

// The closed sets of text values are enums, and the markup they produce is what the text values produced.
public sealed class EnumParameterTests : BunitContext
{
    public EnumParameterTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(DisplayDeltaDirection.Up, "success")]
    [InlineData(DisplayDeltaDirection.Down, "error")]
    [InlineData(DisplayDeltaDirection.Flat, "neutral")]
    public void The_delta_direction_picks_the_color_of_the_badge(DisplayDeltaDirection direction, string color)
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Stat)
            .Add(x => x.Value, "42")
            .Add(x => x.Delta, "+1%")
            .Add(x => x.DeltaDirection, direction));

        Assert.Contains(color, cut.Find("[data-slot=stat-delta]").OuterHtml, StringComparison.Ordinal);
    }

    [Fact]
    public void A_column_aligned_to_the_end_and_a_numeric_one_use_text_end()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Table)
            .Add(x => x.TableColumns, [new DisplayTableColumn("a", "A"), new DisplayTableColumn("b", "B", Align: DisplayAlign.End), new DisplayTableColumn("c", "C", Numeric: true)])
            .Add(x => x.TableRows, [new Dictionary<string, string?> { ["a"] = "1", ["b"] = "2", ["c"] = "3" }]));

        var heads = cut.FindAll("thead th").Select(h => h.ClassList.Contains("text-end")).ToList();
        Assert.Equal([false, true, true], heads);
    }

    [Fact]
    public void A_code_block_exposes_its_variant_in_lowercase_and_block_by_default()
    {
        Assert.Equal("block", Render<KlyDisplay>(p => p.Add(x => x.Kind, DisplayKind.Code).Add(x => x.Value, "x")).Find("[data-slot=display-code]").GetAttribute("data-variant"));
        Assert.Equal("secret", Render<KlyDisplay>(p => p.Add(x => x.Kind, DisplayKind.Code).Add(x => x.Variant, DisplayVariant.Secret).Add(x => x.Value, "x")).Find("[data-slot=display-code]").GetAttribute("data-variant"));
    }

    [Fact]
    public void The_preload_and_the_track_kind_are_written_in_lowercase()
    {
        var video = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/m.mp4")
            .Add(x => x.Preload, MediaPreload.Auto)
            .Add(x => x.Tracks, [new VideoPlayerTrack("/en.vtt", "en", "English", VideoTrackKind.Captions)]));
        Assert.Equal("auto", video.Find("video").GetAttribute("preload"));
        Assert.Equal("captions", video.Find("track").GetAttribute("kind"));

        Assert.Equal("metadata", Render<KlyVideoPlayer>(p => p.Add(x => x.Src, "/m.mp4")).Find("video").GetAttribute("preload"));
        Assert.Equal("none", Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/m.mp3").Add(x => x.Preload, MediaPreload.None)).Find("audio").GetAttribute("preload"));
        Assert.Equal("subtitles", Render<KlyVideoPlayer>(p => p.Add(x => x.Src, "/m.mp4").Add(x => x.Tracks, [new VideoPlayerTrack("/en.vtt", "en", "English")])).Find("track").GetAttribute("kind"));
    }
}
