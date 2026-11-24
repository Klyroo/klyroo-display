using System.Globalization;
using Lux.Display;
using Xunit;

namespace Lux.Display.Tests;

public sealed class ComprehensiveDisplayTests
{
    [Theory]
    [InlineData("spec.pdf", null, FilePreviewKind.Pdf, FilePreviewContainer.Drawer)]
    [InlineData("photo.webp", null, FilePreviewKind.Image, FilePreviewContainer.Modal)]
    [InlineData("clip.mp4", null, FilePreviewKind.Video, FilePreviewContainer.Modal)]
    [InlineData("voice.mp3", null, FilePreviewKind.Audio, FilePreviewContainer.Modal)]
    [InlineData("rates.csv", null, FilePreviewKind.Csv, FilePreviewContainer.Drawer)]
    [InlineData("README.md", null, FilePreviewKind.Markdown, FilePreviewContainer.Drawer)]
    [InlineData("notes.txt", null, FilePreviewKind.Text, FilePreviewContainer.Drawer)]
    [InlineData("archive.zip", null, FilePreviewKind.Archive, FilePreviewContainer.Modal)]
    public void FilePreviewService_detects_all_kinds_and_default_containers(
        string fileName, string? mime, FilePreviewKind expectedKind, FilePreviewContainer expectedContainer)
    {
        var kind = FilePreviewService.DetectKind(fileName, mime);
        Assert.Equal(expectedKind, kind);
        Assert.Equal(expectedContainer, FilePreviewService.DefaultContainerForKind(kind));
    }

    [Fact]
    public void FilePreviewService_parses_RFC4180_CSV_and_manages_gallery_navigation()
    {
        var parsed = FilePreviewService.ParseCsv("Region,Note,MRR\n\"Europe, West\",\"Tier \"\"A\"\"\",148900\nUS,Core,192400");
        Assert.Equal(3, parsed.Headers.Count);
        Assert.Equal(2, parsed.Rows.Count);
        Assert.Equal("Europe, West", parsed.Rows[0][0]);
        Assert.Equal("Tier \"A\"", parsed.Rows[0][1]);
        Assert.Equal("1.5 MB", FilePreviewService.FormatFileSize(1_572_864, CultureInfo.InvariantCulture));

        var svc = new FilePreviewService();
        svc.OpenGallery(
            [new FilePreviewFile { Src = "/a.png", Name = "A" }, new FilePreviewFile { Src = "/b.pdf", Name = "B" }],
            new FilePreviewOptions { Index = 0, Loop = true });

        Assert.True(svc.IsOpen);
        Assert.Equal("A", svc.CurrentFile?.Name);
        svc.Next();
        Assert.Equal("B", svc.CurrentFile?.Name);
        svc.Next();
        Assert.Equal("A", svc.CurrentFile?.Name); // Looped
        svc.Close();
        Assert.False(svc.IsOpen);
    }
}
