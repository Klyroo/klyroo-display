using Bunit;
using Klyroo.UI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

public sealed class DisplayMediaFilePreviewTests : BunitContext
{
    public DisplayMediaFilePreviewTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void KlySecretBlock_masks_the_value_and_KlyFileList_reveals_more_files()
    {
        var code = Render<KlySecretBlock>(p => p
            .Add(x => x.Value, "sk_live_9876543210")
            .Add(x => x.VisibleChars, 4));

        Assert.Equal("••••••••3210", code.Find("[data-slot=code-value]").TextContent);
        code.Find("[data-slot=code-secret-toggle]").Click();
        Assert.Equal("sk_live_9876543210", code.Find("[data-slot=code-value]").TextContent);

        var list = Render<KlyFileList>(p => p
            .Add(x => x.Max, 2)
            .Add(x => x.Step, 2)
            .Add(x => x.Files, [
                new FilePreviewFile { Src = "/f/step-1.txt" },
                new FilePreviewFile { Src = "/f/step-2.txt" },
                new FilePreviewFile { Src = "/f/step-3.txt" }
            ]));

        Assert.Equal(2, list.FindAll("[data-slot=list-item]").Count);
        list.Find("[data-slot=file-list-more]").Click();
        Assert.Equal(3, list.FindAll("[data-slot=list-item]").Count);
        Assert.Empty(list.FindAll("[data-slot=file-list-more]"));
    }

    [Fact]
    public async Task KlyAudioPlayer_and_LxVideoPlayer_enforce_clip_rules_and_max_plays()
    {
        var plays = 0;
        var limitReached = false;
        var cut = Render<KlyVideoPlayer>(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.Start, 5)
            .Add(x => x.End, 25)
            .Add(x => x.Seekable, false)
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.Plays, plays)
            .Add(x => x.PlaysChanged, v => plays = v)
            .Add(x => x.OnLimitReached, () => { limitReached = true; }));

        Assert.Equal(20, cut.Instance.ClipLength);
        Assert.NotNull(cut.Find("[data-slot=media-progress]"));
        Assert.Empty(cut.FindAll("[data-slot=media-seek]"));
        Assert.Contains("1 play left", cut.Find("[data-slot=video-plays-badge]").TextContent);

        await cut.InvokeAsync(() => cut.Instance.TogglePlayAsync());
        Assert.Equal(1, plays);

        // Re-render with updated Plays=1 and finish playback
        cut.Render(p => p
            .Add(x => x.Src, "/media/demo.mp4")
            .Add(x => x.Start, 5)
            .Add(x => x.End, 25)
            .Add(x => x.Seekable, true)
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.Plays, plays)
            .Add(x => x.OnLimitReached, () => { limitReached = true; }));

        await cut.InvokeAsync(() => cut.Instance.SeekAsync(20));
        Assert.True(limitReached);
        Assert.True(cut.Instance.IsLocked);
        Assert.Contains("No plays left", cut.Find("[data-slot=video-locked-text]").TextContent);
    }

    [Fact]
    public void FilePreviewService_detects_kinds_parses_csv_and_drives_LxFilePreviewProvider()
    {
        Assert.Equal(FilePreviewKind.Pdf, FilePreviewService.DetectKind("invoice.pdf"));
        Assert.Equal(FilePreviewKind.Image, FilePreviewService.DetectKind("avatar.webp"));
        Assert.Equal(FilePreviewKind.Video, FilePreviewService.DetectKind("clip.mp4"));
        Assert.Equal(FilePreviewKind.Audio, FilePreviewService.DetectKind("voice.opus"));
        Assert.Equal(FilePreviewKind.Csv, FilePreviewService.DetectKind("export.csv"));
        Assert.Equal(FilePreviewKind.Markdown, FilePreviewService.DetectKind("README.md"));
        Assert.Equal(FilePreviewKind.Text, FilePreviewService.DetectKind("payload.json"));
        Assert.Equal(FilePreviewKind.Office, FilePreviewService.DetectKind("report.xlsx"));
        Assert.Equal(FilePreviewKind.Archive, FilePreviewService.DetectKind("bundle.tar.gz"));

        var csv = FilePreviewService.ParseCsv("\uFEFFName;Amount;Note\r\n\"Alice; Bob\";125.50;\"Line 1\nLine 2\"\r\nCharlie;80;OK", "export.csv");
        Assert.Equal(';', csv.Delimiter);
        Assert.Equal(["Name", "Amount", "Note"], csv.Headers);
        Assert.Equal(2, csv.Rows.Count);
        Assert.Equal("Alice; Bob", csv.Rows[0][0]);
        Assert.True(csv.NumericColumns[1]);
        Assert.False(csv.NumericColumns[0]);

        var provider = Render<KlyFilePreviewProvider>();
        var service = Services.GetRequiredService<FilePreviewService>();
        Assert.True(service.IsMounted);

        service.OpenGallery([
            new FilePreviewFile
            {
                Src = "/files/report.docx",
                Name = "report.docx",
                Size = 20480,
                Rendition = new FilePreviewFile { Src = "/files/report.pdf", Name = "report.pdf", Kind = FilePreviewKind.Pdf }
            },
            new FilePreviewFile
            {
                Src = "/files/data.csv",
                Name = "data.csv",
                Content = "ColA,ColB\n1,10\n2,20"
            }
        ], new FilePreviewOptions { Loop = true });

        provider.Render();
        Assert.NotNull(provider.Find("[data-slot=file-preview-rendition-banner]"));
        Assert.NotNull(provider.Find("[data-slot=renderer-pdf]"));
        Assert.Equal("1 / 2", provider.Find("[data-slot=file-preview-counter]").TextContent.Trim());

        provider.Find("[data-slot=file-preview-next]").Click();
        Assert.NotNull(provider.Find("[data-slot=renderer-csv]"));
        Assert.Equal(2, provider.FindAll("[data-slot=csv-row]").Count);

        // Loop wrap around to index 0
        provider.Find("[data-slot=file-preview-next]").Click();
        Assert.Equal(0, service.CurrentIndex);

        provider.Find("[data-slot=file-preview-close]").Click();
        Assert.False(service.IsOpen);
    }
}
