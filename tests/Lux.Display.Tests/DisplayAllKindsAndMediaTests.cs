using System.Globalization;
using Bunit;
using Lux.Display;
using Lux.UI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lux.Display.Tests;

public sealed class DisplayAllKindsAndMediaTests : BunitContext
{
    public DisplayAllKindsAndMediaTests()
    {
        Services.AddLuxDisplay();
    }

    [Theory]
    [InlineData(null, "application/pdf", FilePreviewKind.Pdf)]
    [InlineData(null, "image/avif; charset=utf-8", FilePreviewKind.Image)]
    [InlineData(null, "video/webm", FilePreviewKind.Video)]
    [InlineData(null, "audio/flac", FilePreviewKind.Audio)]
    [InlineData(null, "text/csv", FilePreviewKind.Csv)]
    [InlineData(null, "text/markdown", FilePreviewKind.Markdown)]
    [InlineData(null, "application/json", FilePreviewKind.Text)]
    [InlineData(null, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FilePreviewKind.Office)]
    [InlineData(null, "application/zip", FilePreviewKind.Archive)]
    [InlineData("https://cdn.luxstack.dev/docs/manual.PDF?token=abc#p=2", null, FilePreviewKind.Pdf)]
    [InlineData("unknown-blob", "application/octet-stream", FilePreviewKind.Other)]
    public void FilePreviewService_DetectKind_handles_MIME_types_query_strings_and_fallbacks(
        string? nameOrUrl, string? mime, FilePreviewKind expected)
    {
        Assert.Equal(expected, FilePreviewService.DetectKind(nameOrUrl, mime));
    }

    [Fact]
    public void FilePreviewService_url_and_size_helpers_handle_edge_cases()
    {
        Assert.Equal("pdf", FilePreviewService.ExtractExtension("https://host/path/file.PDF?v=1#top"));
        Assert.Null(FilePreviewService.ExtractExtension("noextension"));
        Assert.Equal("Report 2026.pdf", FilePreviewService.FileNameFromUrl("https://host/files/Report%202026.pdf?dl=1"));
        Assert.Equal("Untitled", FilePreviewService.FileNameFromUrl("data:text/plain;base64,SGVsbG8="));
        Assert.Equal("Untitled", FilePreviewService.FileNameFromUrl("blob:https://host/1234"));
        Assert.Equal("JSON", FilePreviewService.ExtensionLabel(null, "application/json"));
        Assert.Equal("—", FilePreviewService.ExtensionLabel(null, null));

        Assert.Equal("512 B", FilePreviewService.FormatFileSize(512, CultureInfo.InvariantCulture));
        Assert.Equal("2.5 KB", FilePreviewService.FormatFileSize(2560, CultureInfo.InvariantCulture));
        Assert.Equal("150 KB", FilePreviewService.FormatFileSize(153_600, CultureInfo.InvariantCulture));
        Assert.Equal("2.0 GB", FilePreviewService.FormatFileSize(2L * 1024 * 1024 * 1024, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FilePreviewService_ParseCsv_handles_TSV_pipe_truncation_and_empty_inputs()
    {
        var empty = FilePreviewService.ParseCsv("");
        Assert.Empty(empty.Headers);
        Assert.Empty(empty.Rows);

        var tsv = FilePreviewService.ParseCsv("Key\tScore\nA\t99.5\nB\t84.0", "scores.tsv");
        Assert.Equal('\t', tsv.Delimiter);
        Assert.Equal(2, tsv.Rows.Count);
        Assert.True(tsv.NumericColumns[1]);

        var pipe = FilePreviewService.ParseCsv("Col1|Col2\nHello|42\nWorld|18");
        Assert.Equal('|', pipe.Delimiter);
        Assert.Equal("Hello", pipe.Rows[0][0]);
        Assert.True(pipe.NumericColumns[1]);

        var trunc = FilePreviewService.ParseCsv("Id\n1\n2\n3\n4\n5", maxRows: 2);
        Assert.True(trunc.Truncated);
        Assert.Equal(2, trunc.Rows.Count);
    }

    [Fact]
    public void FilePreviewService_gallery_navigation_clamps_without_loop_and_resolves_largest_container()
    {
        var svc = new FilePreviewService();
        svc.OpenGallery(
        [
            new FilePreviewFile { Src = "/img.png", Name = "img.png" },
            new FilePreviewFile { Src = "/doc.pdf", Name = "doc.pdf" }
        ], new FilePreviewOptions { Index = 0, Loop = false });

        Assert.Equal(FilePreviewContainer.Drawer, svc.ResolveContainer());

        svc.Previous();
        Assert.Equal(0, svc.CurrentIndex); // Clamped at start

        svc.GoTo(1);
        Assert.Equal(1, svc.CurrentIndex);

        svc.Next();
        Assert.Equal(1, svc.CurrentIndex); // Clamped at end

        svc.Close();
        Assert.Null(svc.CurrentFile);
    }

    [Fact]
    public void LxDisplay_renders_Text_Table_Record_Person_Timeline_and_Empty_kinds()
    {
        var text = Render<LxDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Text)
            .Add(x => x.Variant, "quote")
            .Add(x => x.Value, "Simplicity is prerequisite for reliability."));
        Assert.Contains("Simplicity is prerequisite for reliability.", text.Markup);

        var table = Render<LxDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Table)
            .Add(x => x.TableColumns, [new DisplayTableColumn("k", "Key", Mono: true), new DisplayTableColumn("v", "Value", Numeric: true)])
            .Add(x => x.TableRows, [new Dictionary<string, string?> { ["k"] = "latency_p95", ["v"] = "14ms" }]));
        Assert.Contains("latency_p95", table.Markup);
        Assert.Contains("14ms", table.Markup);

        var record = Render<LxDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Record)
            .Add(x => x.Records, [new DisplayRecordItem { Reference = "INV-42", Title = "Acme Annual Plan", Trailing = "€12,000" }]));
        Assert.Contains("INV-42", record.Markup);
        Assert.Contains("Acme Annual Plan", record.Markup);

        var person = Render<LxDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Person)
            .Add(x => x.People, [new DisplayPersonItem("Ada Lovelace", "Principal Engineer")]));
        Assert.Contains("Ada Lovelace", person.Markup);

        var timeline = Render<LxDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Timeline)
            .Add(x => x.TimelineItems, [
                new DisplayTimelineItem("Created", State: DisplayTimelineState.Done),
                new DisplayTimelineItem("In Review", State: DisplayTimelineState.Current),
                new DisplayTimelineItem("Published", State: DisplayTimelineState.Upcoming)
            ]));
        Assert.Contains("In Review", timeline.Markup);
        Assert.Contains("Published", timeline.Markup);

        var empty = Render<LxDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Empty)
            .Add(x => x.Title, "No artifacts")
            .Add(x => x.Description, "Upload a file to start."));
        Assert.Contains("No artifacts", empty.Markup);
    }

    [Fact]
    public async Task LxAudioPlayer_enforces_clip_boundaries_and_max_plays_lock()
    {
        var plays = 0;
        var limitReached = false;
        var cut = Render<LxAudioPlayer>(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.Start, 10)
            .Add(x => x.End, 40)
            .Add(x => x.Seekable, false)
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.Plays, plays)
            .Add(x => x.PlaysChanged, v => plays = v)
            .Add(x => x.OnLimitReached, () => { limitReached = true; }));

        Assert.Equal(30, cut.Instance.ClipLength);
        Assert.False(cut.Instance.IsLocked);

        await cut.InvokeAsync(() => cut.Instance.TogglePlayAsync());
        Assert.Equal(1, plays);

        cut.Render(p => p
            .Add(x => x.Src, "/media/brief.mp3")
            .Add(x => x.Start, 10)
            .Add(x => x.End, 40)
            .Add(x => x.Seekable, true)
            .Add(x => x.MaxPlays, 1)
            .Add(x => x.Plays, plays)
            .Add(x => x.OnLimitReached, () => { limitReached = true; }));

        await cut.InvokeAsync(() => cut.Instance.SeekAsync(30));
        Assert.True(limitReached);
        Assert.True(cut.Instance.IsLocked);
    }
}
