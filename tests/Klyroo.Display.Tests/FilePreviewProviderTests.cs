using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

public sealed class FilePreviewProviderTests : BunitContext
{
    public FilePreviewProviderTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    private (IRenderedComponent<KlyFilePreviewProvider> Provider, FilePreviewService Service) RenderProvider()
    {
        var provider = Render<KlyFilePreviewProvider>();
        return (provider, Services.GetRequiredService<FilePreviewService>());
    }

    [Fact]
    public void Closing_the_preview_drops_the_expanded_state()
    {
        var (provider, service) = RenderProvider();
        var image = new FilePreviewFile { Src = "/files/photo.png" };

        service.Open(image);
        provider.Render();
        provider.Find("[data-slot=file-preview-expand]").Click();
        Assert.Equal("fullscreen", provider.Find("[data-slot=file-preview-provider]").GetAttribute("data-container"));

        provider.Find("[data-slot=file-preview-close]").Click();
        service.Open(image);
        provider.Render();

        Assert.Equal("modal", provider.Find("[data-slot=file-preview-provider]").GetAttribute("data-container"));
    }

    [Fact]
    public void Opening_the_preview_moves_the_focus_into_the_dialog_once()
    {
        var (provider, service) = RenderProvider();

        service.OpenGallery([new FilePreviewFile { Src = "/files/a.png" }, new FilePreviewFile { Src = "/files/b.png" }]);
        provider.Render();
        provider.Find("[data-slot=file-preview-next]").Click();

        var focus = JSInterop.VerifyFocusAsyncInvoke();
        Assert.Equal(1, JSInterop.Invocations.Count(i => i.Identifier == focus.Identifier));
        Assert.IsType<ElementReference>(focus.Arguments[0]);

        provider.Find("[data-slot=file-preview-close]").Click();
        service.Open(new FilePreviewFile { Src = "/files/c.png" });
        provider.Render();
        Assert.Equal(2, JSInterop.Invocations.Count(i => i.Identifier == focus.Identifier));
    }

    [Fact]
    public void A_custom_action_with_a_link_renders_an_anchor_with_its_icon()
    {
        var (provider, service) = RenderProvider();
        FilePreviewFile? selected = null;
        service.Open(new FilePreviewFile
        {
            Src = "/files/report.pdf",
            Actions =
            [
                new FilePreviewAction { Label = "Open in editor", Icon = "external-link", Href = "/edit/report" },
                new FilePreviewAction { Label = "Share", OnSelect = f => { selected = f; return Task.CompletedTask; } },
            ],
        });
        provider.Render();

        var actions = provider.FindAll("[data-slot=file-preview-custom-action]");
        Assert.Equal("A", actions[0].TagName);
        Assert.Equal("/edit/report", actions[0].GetAttribute("href"));
        Assert.NotNull(actions[0].QuerySelector("svg"));
        Assert.Equal("BUTTON", actions[1].TagName);
        Assert.Null(actions[1].QuerySelector("svg"));

        actions[1].Click();
        Assert.Equal("/files/report.pdf", selected?.Src);
    }

    [Theory]
    [InlineData("", "0 lines")]
    [InlineData("single", "1 line")]
    [InlineData("first\r\nsecond\n", "2 lines")]
    [InlineData("a\nb\nc", "3 lines")]
    public void Text_files_count_their_lines(string content, string expected)
    {
        var (provider, service) = RenderProvider();
        service.Open(new FilePreviewFile { Src = "/files/notes.txt", Content = content });
        provider.Render();

        Assert.Equal(expected, provider.Find("[data-slot=text-line-count]").TextContent);
    }

    [Fact]
    public void Provider_texts_come_from_DisplayTexts()
    {
        Services.AddKlyrooDisplay(d => d.Texts("en", t => t with
        {
            ClosePreview = "Fermer",
            LineCount = n => $"{n} ligne(s)",
        }));
        var (provider, service) = RenderProvider();
        service.Open(new FilePreviewFile { Src = "/files/notes.txt", Content = "x" });
        provider.Render();

        Assert.Equal("Fermer", provider.Find("[data-slot=file-preview-close]").GetAttribute("aria-label"));
        Assert.Equal("1 ligne(s)", provider.Find("[data-slot=text-line-count]").TextContent);
    }
}
