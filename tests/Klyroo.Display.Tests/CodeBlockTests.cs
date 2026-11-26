using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

// The Code block and the file preview highlight their code with CodeHighlighter.
public sealed class CodeBlockTests : BunitContext
{
    private const string Sample = "public class Greeter\n{\n    // say hi\n    string Hi() => \"hello\" + 42;\n}\n";

    public CodeBlockTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<KlyDisplay> RenderCode(string value, Action<ComponentParameterCollectionBuilder<KlyDisplay>>? configure = null) =>
        Render<KlyDisplay>(p =>
        {
            p.Add(x => x.Kind, DisplayKind.Code).Add(x => x.Value, value);
            configure?.Invoke(p);
        });

    [Fact]
    public void A_known_language_is_rendered_as_token_spans()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, "csharp"));

        var tokens = cut.FindAll("[data-slot=code-value] span[data-token]");
        Assert.Contains(tokens, t => t.GetAttribute("data-token") == "keyword" && t.TextContent == "public");
        Assert.Contains(tokens, t => t.GetAttribute("data-token") == "keyword" && t.TextContent == "class");
        Assert.Contains(tokens, t => t.GetAttribute("data-token") == "type" && t.TextContent == "Greeter");
        Assert.Contains(tokens, t => t.GetAttribute("data-token") == "string" && t.TextContent == "\"hello\"");
        Assert.Contains(tokens, t => t.GetAttribute("data-token") == "number" && t.TextContent == "42");
        Assert.Contains(tokens, t => t.GetAttribute("data-token") == "comment" && t.TextContent == "// say hi");
        Assert.All(tokens, t => Assert.False(string.IsNullOrWhiteSpace(t.GetAttribute("class"))));
        Assert.Equal("csharp", cut.Find("[data-slot=code-value]").GetAttribute("data-language"));
    }

    [Fact]
    public void The_text_content_and_the_whitespace_are_unchanged()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, "cs"));

        Assert.Equal(Sample, cut.Find("[data-slot=code-value]").TextContent);
        // Nothing but the code between <code> and </code>: whitespace there would show inside the <pre>.
        var code = cut.Find("[data-slot=code-value] > code").InnerHtml;
        Assert.StartsWith("<span data-token=\"keyword\"", code);
        Assert.EndsWith("</span>\n", code);
    }

    [Fact]
    public void Token_colors_are_semantic_theme_classes_and_there_is_no_aria_noise()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, "csharp"));

        var markup = cut.Find("[data-slot=code-value]").InnerHtml;
        Assert.Contains("text-(--klyroo-primary-text)", markup);
        Assert.Contains("text-(--klyroo-success-text)", markup);
        Assert.DoesNotContain("aria-", markup);
        Assert.DoesNotContain("role=", markup);
        Assert.DoesNotContain("tabindex", markup);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("brainfuck")]
    public void An_unknown_language_stays_plain_text(string? language)
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, language));

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal("<code>" + System.Net.WebUtility.HtmlEncode(Sample).Replace("&quot;", "\"") + "</code>", cut.Find("[data-slot=code-value]").InnerHtml);
        Assert.Null(cut.Find("[data-slot=code-value]").GetAttribute("data-language"));
    }

    [Fact]
    public void Highlight_false_renders_plain_text()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, "csharp").Add(x => x.Highlight, false));

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal(Sample, cut.Find("[data-slot=code-value]").TextContent);
        Assert.Equal("csharp", cut.Find("[data-slot=code-label]").TextContent);
    }

    [Fact]
    public void Markup_in_the_code_is_escaped()
    {
        var cut = RenderCode("<script>alert('x')</script>", p => p.Add(x => x.Language, "html"));

        Assert.Empty(cut.FindAll("[data-slot=code-value] script"));
        Assert.Equal("<script>alert('x')</script>", cut.Find("[data-slot=code-value]").TextContent);
        Assert.Contains(cut.FindAll("span[data-token=tag]"), t => t.TextContent == "script");
    }

    [Fact]
    public void A_secret_stays_masked_and_is_never_highlighted()
    {
        var cut = RenderCode("sk_live_9876543210", p => p.Add(x => x.Variant, DisplayVariant.Secret).Add(x => x.Language, "csharp"));

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal("••••••••3210", cut.Find("[data-slot=code-value]").TextContent);
        Assert.DoesNotContain("sk_live", cut.Markup);
        Assert.DoesNotContain("9876543210", cut.Markup);
        Assert.Null(cut.Find("[data-slot=code-value]").GetAttribute("data-language"));

        cut.Find("[data-slot=code-secret-toggle]").Click();
        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal("sk_live_9876543210", cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void The_copy_button_copies_the_raw_code()
    {
        SetRendererInfo(new RendererInfo("Server", true));
        var copy = JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true);
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, "csharp"));

        cut.Find("[data-slot=code-copy]").Click();

        var invocation = Assert.Single(copy.Invocations);
        Assert.Equal(Sample, invocation.Arguments[0]);
    }

    [Fact]
    public void A_code_over_the_size_cap_falls_back_to_plain_text()
    {
        var large = string.Concat(Enumerable.Repeat("int x = 1; // padding\n", 6_000));
        Assert.True(large.Length > CodeHighlighter.MaxLength);

        var cut = RenderCode(large, p => p.Add(x => x.Language, "csharp"));

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal(large, cut.Find("[data-slot=code-value]").TextContent);
        Assert.Null(cut.Find("[data-slot=code-value]").GetAttribute("data-language"));
    }

    [Fact]
    public void Changing_the_code_or_the_language_re_highlights()
    {
        var cut = RenderCode("var a = 1;", p => p.Add(x => x.Language, "csharp"));
        Assert.Contains(cut.FindAll("span[data-token=keyword]"), t => t.TextContent == "var");

        cut.Render(p => p.Add(x => x.Value, "SELECT 1").Add(x => x.Language, "sql"));
        Assert.Contains(cut.FindAll("span[data-token=keyword]"), t => t.TextContent == "SELECT");
        Assert.DoesNotContain(cut.FindAll("span[data-token=keyword]"), t => t.TextContent == "var");

        cut.Render(p => p.Add(x => x.Language, null));
        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
    }

    [Fact]
    public void A_code_block_with_a_title_keeps_its_label_and_highlights()
    {
        var cut = RenderCode("{\"a\": 1}", p => p.Add(x => x.Language, "json").Add(x => x.Title, "Payload"));

        Assert.Equal("Payload", cut.Find("[data-slot=code-label]").TextContent);
        Assert.Contains(cut.FindAll("span[data-token=property]"), t => t.TextContent == "\"a\"");
    }

    private IRenderedComponent<KlyFilePreviewProvider> OpenText(string name, string content)
    {
        var provider = Render<KlyFilePreviewProvider>();
        Services.GetRequiredService<FilePreviewService>().Open(new FilePreviewFile { Src = "/files/" + name, Name = name, Content = content });
        provider.Render();
        return provider;
    }

    [Fact]
    public void The_file_preview_highlights_a_code_file_from_its_extension()
    {
        var provider = OpenText("Greeter.cs", Sample);

        var stage = provider.Find("[data-slot=text-content]");
        Assert.Contains(stage.QuerySelectorAll("span[data-token=keyword]"), t => t.TextContent == "class");
        Assert.Equal("csharp", stage.GetAttribute("data-language"));
        Assert.Equal(Sample, stage.TextContent);
        Assert.Equal("5 lines", provider.Find("[data-slot=text-line-count]").TextContent);
    }

    [Fact]
    public void The_file_preview_keeps_line_wrapping_and_the_highlight()
    {
        var provider = OpenText("data.json", "{\"a\": [1, 2]}\n");

        Assert.Contains("whitespace-pre", provider.Find("[data-slot=text-content]").ClassList);
        provider.Find("[data-slot=text-wrap-toggle]").Click();

        var stage = provider.Find("[data-slot=text-content]");
        Assert.Contains("whitespace-pre-wrap", stage.ClassList);
        Assert.Contains(stage.QuerySelectorAll("span[data-token=property]"), t => t.TextContent == "\"a\"");
    }

    [Theory]
    [InlineData("notes.txt")]
    [InlineData("server.log")]
    [InlineData("settings.ini")]
    public void The_file_preview_leaves_other_text_files_plain(string name)
    {
        var provider = OpenText(name, "if (x) { return 1; }");

        Assert.Empty(provider.FindAll("[data-slot=text-content] span"));
        Assert.Equal("if (x) { return 1; }", provider.Find("[data-slot=text-content]").TextContent);
    }
}
