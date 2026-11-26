using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Klyroo.Display.Tests;

public sealed class MediaPlayerControlsTests : BunitContext
{
    private readonly BunitJSModuleInterop _media;

    public MediaPlayerControlsTests()
    {
        Services.AddKlyrooDisplay();
        _media = JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js");
        _media.Mode = JSRuntimeMode.Loose;
    }

    private Task CallFromMediaAsync(string method, params object[] args)
    {
        var reference = _media.Invocations["attach"].Single().Arguments[1]!;
        var target = reference.GetType().GetProperty("Value")!.GetValue(reference)!;
        return (Task)target.GetType().GetMethod(method)!.Invoke(target, args)!;
    }

    private Dictionary<string, object?> LastOptions() =>
        Assert.IsType<Dictionary<string, object?>>(_media.Invocations["update"].Last().Arguments[1]);

    [Fact]
    public void Download_link_names_the_file_from_its_url_when_no_name_is_given()
    {
        var audio = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief%20one.mp3?sig=1")
            .Add(x => x.Download, true));
        Assert.Equal("brief one.mp3", audio.Find("[data-slot=media-download]").GetAttribute("download"));

        var video = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "data:video/mp4;base64,AAAA")
            .Add(x => x.Download, true));
        // A data URI has no name: a bare attribute lets the browser pick one.
        Assert.Equal("", video.Find("[data-slot=media-download]").GetAttribute("download"));

        var named = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.DownloadUrl, "/files/demo-hd.mp4")
            .Add(x => x.DownloadFileName, "Demo.mp4"));
        Assert.Equal("Demo.mp4", named.Find("[data-slot=media-download]").GetAttribute("download"));
    }

    [Fact]
    public async Task A_play_requested_while_locked_raises_OnLimitReached()
    {
        var reached = 0;
        var cut = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.Plays, 1)
            .Add(x => x.OnLimitReached, () => reached++));
        Assert.True(cut.Instance.IsLocked);

        await cut.InvokeAsync(() => cut.Instance.TogglePlayAsync());
        await cut.InvokeAsync(() => CallFromMediaAsync("Key", " "));

        Assert.Equal(2, reached);
        Assert.Empty(_media.Invocations["play"]);
    }

    [Fact]
    public async Task Turning_captions_off_survives_a_re_render()
    {
        var cut = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.Tracks, [new VideoPlayerTrack("/media/en.vtt", "en", "English", Default: true)]));
        Assert.Equal("true", cut.Find("[data-slot=media-captions]").GetAttribute("aria-pressed"));

        await cut.Find("[data-slot=media-captions]").ClickAsync();
        cut.Render(p => p.Add(x => x.Title, "Demo"));

        Assert.Equal("false", cut.Find("[data-slot=media-captions]").GetAttribute("aria-pressed"));
        Assert.Equal(-1, LastOptions()["track"]);
    }

    [Fact]
    public async Task Keys_are_handled_only_when_the_player_root_has_the_focus()
    {
        var cut = Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/media/brief.mp3"));

        // No Blazor key handler on the player: a key on a focused button would bubble to it and act twice.
        Assert.Throws<MissingEventHandlerException>(() =>
            cut.Find("[data-slot=media-mute]").KeyDown(new KeyboardEventArgs { Key = "m" }));

        // The module listens on the root it receives and reports only keys aimed at it.
        var attach = Assert.Single(_media.Invocations["attach"]);
        Assert.IsType<ElementReference>(attach.Arguments[3]);
        Assert.Equal(true, Assert.IsType<Dictionary<string, object?>>(attach.Arguments[2])["keyboard"]);

        await cut.InvokeAsync(() => CallFromMediaAsync("Key", "m"));
        Assert.Equal("true", cut.Find("[data-slot=media-mute]").GetAttribute("aria-pressed"));
        await cut.InvokeAsync(() => CallFromMediaAsync("Key", " "));
        Assert.Single(_media.Invocations["play"]);
    }

    [Fact]
    public async Task Keyboard_false_ignores_keys_reported_by_the_module()
    {
        var cut = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.Keyboard, false));

        await cut.InvokeAsync(() => CallFromMediaAsync("Key", "m"));

        Assert.Equal("false", cut.Find("[data-slot=media-mute]").GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task Volume_slider_raises_VolumeChanged_and_sets_the_element_volume()
    {
        var volumes = new List<double>();
        var cut = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.VolumeChanged, volumes.Add));

        await cut.Find("[data-slot=media-volume]").InputAsync(new ChangeEventArgs { Value = "0.4" });

        Assert.Equal([0.4], volumes);
        Assert.Equal(0.4, LastOptions()["volume"]);

        var hidden = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.Hide, [MediaPlayerControl.Volume]));
        Assert.Empty(hidden.FindAll("[data-slot=media-volume]"));
        Assert.Empty(hidden.FindAll("[data-slot=media-mute]"));
    }

    [Fact]
    public void Compact_renders_a_denser_bar()
    {
        var regular = Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/media/brief.mp3"));
        var compact = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.Compact, true));

        Assert.Null(regular.Find("[data-slot=audio-player]").GetAttribute("data-compact"));
        Assert.Equal("true", compact.Find("[data-slot=audio-player]").GetAttribute("data-compact"));
        Assert.Contains("size-8", regular.Find("[data-slot=media-play]").ClassList);
        Assert.Contains("size-7", compact.Find("[data-slot=media-play]").ClassList);
        Assert.Contains("py-1", compact.Find("[data-slot=media-controls]").ClassList);

        var video = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.Compact, true));
        Assert.Contains("px-2", video.Find("[data-slot=media-controls]").ClassList);
    }

    [Fact]
    public async Task Picture_in_picture_and_fullscreen_buttons_follow_browser_support()
    {
        var cut = Render<KlyVideoPlayer>(p => p.Add(x => x.Src, "/media/demo.mp4"));
        Assert.Empty(cut.FindAll("[data-slot=media-pip]"));
        Assert.Empty(cut.FindAll("[data-slot=media-fullscreen]"));

        await cut.InvokeAsync(() => CallFromMediaAsync("Capabilities", true, true));
        await cut.Find("[data-slot=media-pip]").ClickAsync();
        await cut.Find("[data-slot=media-fullscreen]").ClickAsync();

        Assert.Single(_media.Invocations["togglePictureInPicture"]);
        Assert.Single(_media.Invocations["toggleFullscreen"]);

        await cut.InvokeAsync(() => CallFromMediaAsync("FullscreenChange", true));
        Assert.Equal("Exit fullscreen", cut.Find("[data-slot=media-fullscreen]").GetAttribute("aria-label"));
        await cut.InvokeAsync(() => CallFromMediaAsync("PictureInPictureChange", true));
        Assert.Equal("true", cut.Find("[data-slot=media-pip]").GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task Picture_in_picture_is_hidden_when_not_seekable_or_hidden()
    {
        var cut = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.Seekable, false));
        await cut.InvokeAsync(() => CallFromMediaAsync("Capabilities", true, true));
        Assert.Empty(cut.FindAll("[data-slot=media-pip]"));
        Assert.NotEmpty(cut.FindAll("[data-slot=media-fullscreen]"));

        cut.Render(p => p
            .Add(x => x.Seekable, true)
            .Add(x => x.Hide, [MediaPlayerControl.PictureInPicture, MediaPlayerControl.Fullscreen]));
        Assert.Empty(cut.FindAll("[data-slot=media-pip]"));
        Assert.Empty(cut.FindAll("[data-slot=media-fullscreen]"));
    }

    [Fact]
    public async Task Seek_slider_speaks_its_position_and_follows_the_playback()
    {
        var audio = Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/media/a.mp3").Add(x => x.Duration, 60));
        Assert.Equal("0:00 of 1:00", audio.Find("[data-slot=media-seek]").GetAttribute("aria-valuetext"));
        await audio.InvokeAsync(() => audio.Instance.TogglePlayAsync());
        await audio.InvokeAsync(() => CallFromMediaAsync("TimeUpdate", 37d));
        Assert.Equal("0:37 of 1:00", audio.Find("[data-slot=media-seek]").GetAttribute("aria-valuetext"));

        var video = Render<KlyVideoPlayer>(p => p.Add(x => x.Src, "/media/v.mp4").Add(x => x.Duration, 3725).Add(x => x.Start, 25));
        Assert.Equal("0:00 of 1:01:40", video.Find("[data-slot=media-seek]").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void Progress_bar_of_a_non_seekable_player_also_has_a_spoken_value()
    {
        var cut = Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/media/a.mp3").Add(x => x.Duration, 90).Add(x => x.Seekable, false));
        Assert.Equal("0:00 of 1:30", cut.Find("[data-slot=media-progress]").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void Seek_value_text_is_localized_and_replaceable()
    {
        Services.AddKlyrooDisplay(d => d.Texts("en", t => t with { SeekValueText = (p, l) => $"{p} sur {l}" }));
        var cut = Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/media/a.mp3").Add(x => x.Duration, 60));
        Assert.Equal("0:00 sur 1:00", cut.Find("[data-slot=media-seek]").GetAttribute("aria-valuetext"));
        Assert.Equal("0:37 sur 1:00", DisplayLocales.Fr.SeekValueText("0:37", "1:00"));
    }

    [Fact]
    public void Player_texts_come_from_DisplayTexts()
    {
        Services.AddKlyrooDisplay(d => d.Texts("en", t => t with
        {
            NoPlaysLeft = "Plus de lecture",
            PlaysLeft = n => $"{n} restante(s)",
        }));

        var locked = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.Plays, 1));
        var video = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.MaxPlays, 3));

        Assert.Contains("Plus de lecture", locked.Find("[data-slot=media-locked]").TextContent);
        Assert.Equal("3 restante(s)", video.Find("[data-slot=video-plays-badge]").TextContent.Trim());
    }
}
