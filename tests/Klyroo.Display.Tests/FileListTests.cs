using Bunit;
using Klyroo.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

public sealed class FileListTests : BunitContext
{
    public FileListTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<KlyFileList> Render(params FileListItem[] items) =>
        Render<KlyFileList>(p => p.Add(x => x.Files, items));

    [Fact]
    public void A_file_converts_to_an_item_and_shows_badge_name_and_size()
    {
        FileListItem item = new FilePreviewFile { Src = "/files/spec.pdf", Size = 1_572_864 };
        var cut = Render(item);

        Assert.Equal("PDF", cut.Find("[data-slot=file-badge]").TextContent.Trim());
        Assert.Equal("spec.pdf", cut.Find("[data-slot=file-name]").TextContent);
        Assert.Equal("1.5 MB", cut.Find("[data-slot=file-description]").TextContent);
        Assert.Empty(cut.FindAll("[data-slot=file-meta]"));
    }

    [Fact]
    public void A_list_of_files_is_a_list_of_items()
    {
        var cut = Render<KlyFileList>(p => p.Add(x => x.Files, [new FilePreviewFile { Src = "/a.txt" }, new FilePreviewFile { Src = "/b.txt" }]));

        Assert.Equal(2, cut.FindAll("[data-slot=list-item]").Count);
    }

    [Fact]
    public void The_description_shows_under_the_name_before_the_size()
    {
        var cut = Render(
            new FileListItem(new FilePreviewFile { Src = "/a.csv", Size = 2_048 }, Description: "Export of March"),
            new FileListItem(new FilePreviewFile { Src = "/b.csv" }, Description: "No size known"));

        var lines = cut.FindAll("[data-slot=file-description]").Select(l => l.TextContent).ToList();
        Assert.Equal(["Export of March · 2.0 KB", "No size known"], lines);
    }

    [Fact]
    public void Meta_wins_over_the_date_and_the_date_fills_in_without_it()
    {
        var updated = new DateTimeOffset(2026, 11, 20, 9, 30, 0, TimeSpan.Zero);
        var cut = Render(
            new FileListItem(new FilePreviewFile { Src = "/a.pdf", UpdatedAt = updated }, Meta: "Signed"),
            new FileListItem(new FilePreviewFile { Src = "/b.pdf", UpdatedAt = updated }));

        var meta = cut.FindAll("[data-slot=file-meta]");
        Assert.Equal("Signed", meta[0].TextContent);
        Assert.Contains("2026", meta[1].TextContent);
    }

    [Fact]
    public void An_icon_replaces_the_extension_in_the_badge_and_a_tone_colors_it()
    {
        var cut = Render(
            new FileListItem(new FilePreviewFile { Src = "/a.pdf" }, Icon: "lock", Tone: KlyColor.Success),
            new FileListItem(new FilePreviewFile { Src = "/b.pdf" }));

        var badges = cut.FindAll("[data-slot=file-badge]");
        Assert.DoesNotContain("PDF", badges[0].TextContent);
        Assert.NotNull(badges[0].QuerySelector("svg, span, i"));
        Assert.Equal("success", badges[0].GetAttribute("data-color"));
        Assert.Contains("bg-(--kly-color)/10", badges[0].ClassName);
        Assert.DoesNotContain("bg-(--klyroo-bg-muted)", badges[0].ClassName);
        Assert.Equal("PDF", badges[1].TextContent.Trim());
        Assert.Null(badges[1].GetAttribute("data-color"));
        Assert.Contains("bg-(--klyroo-bg-muted)", badges[1].ClassName);
    }

    [Fact]
    public async Task A_click_opens_the_files_of_the_items_at_the_clicked_one()
    {
        SetRendererInfo(new RendererInfo("Server", true));
        Render<KlyFilePreviewProvider>();
        var preview = Services.GetRequiredService<FilePreviewService>();
        var cut = Render(
            new FileListItem(new FilePreviewFile { Src = "/a.pdf" }, Description: "first"),
            new FileListItem(new FilePreviewFile { Src = "/b.pdf" }, Meta: "second"));

        await cut.FindAll("a")[1].ClickAsync();

        Assert.Equal(["/a.pdf", "/b.pdf"], preview.Files.Select(f => f.Src));
        Assert.Equal(1, preview.CurrentIndex);
    }

    [Fact]
    public void Ui_overrides_the_badge_slots()
    {
        var cut = Render<KlyFileList>(p => p
            .Add(x => x.Files, [new FileListItem(new FilePreviewFile { Src = "/a.pdf" }, Tone: KlyColor.Error)])
            .Add(x => x.Ui, new FileListUi { BadgeTone = "ring-1" }));

        Assert.Contains("ring-1", cut.Find("[data-slot=file-badge]").ClassName);
    }
}
