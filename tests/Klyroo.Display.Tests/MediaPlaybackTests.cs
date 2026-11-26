using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

public sealed class MediaPlaybackTests : BunitContext
{
    private readonly BunitJSModuleInterop _media;

    public MediaPlaybackTests()
    {
        Services.AddKlyrooDisplay();
        _media = JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js");
        _media.Mode = JSRuntimeMode.Loose;
    }

    // The [JSInvokable] target that klyroo-display-media.js calls back, taken from the attach call.
    private Task CallFromMediaAsync(string method, params object[] args)
    {
        var reference = _media.Invocations["attach"].Single().Arguments[1]!;
        var target = reference.GetType().GetProperty("Value")!.GetValue(reference)!;
        return (Task)target.GetType().GetMethod(method)!.Invoke(target, args)!;
    }

    [Fact]
    public async Task Play_button_plays_the_element_from_the_clip_start_and_pause_pauses_it()
    {
        var cut = Render<KlyAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.Start, 10)
            .Add(x => x.End, 40));

        var attach = Assert.Single(_media.Invocations["attach"]);
        var options = Assert.IsType<Dictionary<string, object?>>(attach.Arguments[2]);
        Assert.Equal(10d, options["start"]);
        Assert.Equal(40d, options["end"]);

        await cut.Find("[data-slot=media-play]").ClickAsync();
        Assert.Equal(10d, Assert.Single(_media.Invocations["play"]).Arguments[1]);

        await cut.Find("[data-slot=media-play]").ClickAsync();
        Assert.Single(_media.Invocations["pause"]);
    }

    [Fact]
    public async Task Element_time_and_end_drive_the_position_and_the_play_limit()
    {
        var progress = new List<MediaPlayerProgress>();
        var ended = 0;
        var cut = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.Start, 5)
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.OnTimeUpdate, progress.Add)
            .Add(x => x.OnEnded, () => ended++));

        await cut.InvokeAsync(() => CallFromMediaAsync("DurationChange", 125d));
        Assert.Equal(120, cut.Instance.ClipLength);

        await cut.InvokeAsync(() => cut.Instance.TogglePlayAsync());
        await cut.InvokeAsync(() => CallFromMediaAsync("TimeUpdate", 35d));
        Assert.Equal(30, cut.Instance.ClipPosition);
        Assert.Equal(30, Assert.Single(progress).Position);

        await cut.InvokeAsync(() => CallFromMediaAsync("Ended"));
        Assert.Equal(1, ended);
        Assert.True(cut.Instance.IsLocked);
    }

    [Fact]
    public async Task Mute_and_speed_are_pushed_to_the_element()
    {
        var cut = Render<KlyAudioPlayer>(p => p.Add(x => x.Src, "/media/brief.mp3"));

        await cut.Find("[data-slot=media-mute]").ClickAsync();
        await cut.Find("[data-slot=media-rate]").ClickAsync();

        var last = Assert.IsType<Dictionary<string, object?>>(_media.Invocations["update"].Last().Arguments[1]);
        Assert.Equal(true, last["muted"]);
        Assert.Equal(1.25d, last["rate"]);
    }

    [Fact]
    public async Task Clicking_a_file_opens_that_file_when_earlier_items_have_no_link()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Render<KlyFilePreviewProvider>();
        var preview = Services.GetRequiredService<FilePreviewService>();
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.List)
            .Add(x => x.Variant, "files")
            .Add(x => x.ListItems,
            [
                new DisplayListItem { Text = "Pending upload" },
                new DisplayListItem { Text = "contract.pdf", Href = "/files/contract.pdf" },
                new DisplayListItem { Text = "invoice.pdf", Href = "/files/invoice.pdf" },
                new DisplayListItem { Text = "receipt.pdf", Href = "/files/receipt.pdf" },
            ]));

        await cut.FindAll("a").Single(a => a.TextContent == "invoice.pdf").ClickAsync();

        Assert.Equal(3, preview.Files.Count);
        Assert.Equal(1, preview.CurrentIndex);
        Assert.Equal("/files/invoice.pdf", preview.Files[preview.CurrentIndex].Src);
    }
}
