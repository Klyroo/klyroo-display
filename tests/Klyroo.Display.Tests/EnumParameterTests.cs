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
