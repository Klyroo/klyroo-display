using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

// The code block, the secret block and the file preview highlight their code with CodeHighlighter.
public sealed class CodeBlockTests : BunitContext
{
    private const string Sample = "public class Greeter\n{\n    // say hi\n    string Hi() => \"hello\" + 42;\n}\n";

    public CodeBlockTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<KlyCodeBlock> RenderCode(string code, Action<ComponentParameterCollectionBuilder<KlyCodeBlock>>? configure = null) =>
        Render<KlyCodeBlock>(p =>
        {
            p.Add(x => x.Code, code);
            configure?.Invoke(p);
        });

    private IRenderedComponent<KlySecretBlock> RenderSecret(string value, Action<ComponentParameterCollectionBuilder<KlySecretBlock>>? configure = null) =>
        Render<KlySecretBlock>(p =>
        {
            p.Add(x => x.Value, value);
            configure?.Invoke(p);
        });

    [Fact]
    public void A_known_language_is_rendered_as_token_spans()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp));

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
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp));

        Assert.Equal(Sample, cut.Find("[data-slot=code-value]").TextContent);
        // Nothing but the code between <code> and </code>: whitespace there would show inside the <pre>.
        var code = cut.Find("[data-slot=code-value] > code").InnerHtml;
        Assert.StartsWith("<span data-token=\"keyword\"", code);
        Assert.EndsWith("</span>\n", code);
    }

    [Fact]
    public void Token_colors_are_semantic_theme_classes_and_there_is_no_aria_noise()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp));

        var markup = cut.Find("[data-slot=code-value]").InnerHtml;
        Assert.Contains("text-(--klyroo-primary-text)", markup);
        Assert.Contains("text-(--klyroo-success-text)", markup);
        Assert.DoesNotContain("aria-", markup);
        Assert.DoesNotContain("role=", markup);
        Assert.DoesNotContain("tabindex", markup);
    }

    [Fact]
    public void Without_a_language_the_code_stays_plain_text_and_the_label_is_Code()
    {
        var cut = RenderCode(Sample);

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal("<code>" + System.Net.WebUtility.HtmlEncode(Sample).Replace("&quot;", "\"") + "</code>", cut.Find("[data-slot=code-value]").InnerHtml);
        Assert.Null(cut.Find("[data-slot=code-value]").GetAttribute("data-language"));
        Assert.Equal("Code", cut.Find("[data-slot=code-label]").TextContent);
    }

    [Fact]
    public void Highlight_false_renders_plain_text()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp).Add(x => x.Highlight, false));

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal(Sample, cut.Find("[data-slot=code-value]").TextContent);
        Assert.Equal("csharp", cut.Find("[data-slot=code-label]").TextContent);
    }

    [Fact]
    public void Markup_in_the_code_is_escaped()
    {
        var cut = RenderCode("<script>alert('x')</script>", p => p.Add(x => x.Language, CodeLanguage.Html));

        Assert.Empty(cut.FindAll("[data-slot=code-value] script"));
        Assert.Equal("<script>alert('x')</script>", cut.Find("[data-slot=code-value]").TextContent);
        Assert.Contains(cut.FindAll("span[data-token=tag]"), t => t.TextContent == "script");
    }

    [Fact]
    public void A_secret_stays_masked_and_is_never_highlighted()
    {
        var cut = RenderSecret("sk_live_9876543210");

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal("••••••••3210", cut.Find("[data-slot=code-value]").TextContent);
        Assert.DoesNotContain("sk_live", cut.Markup);
        Assert.DoesNotContain("9876543210", cut.Markup);
        Assert.Null(cut.Find("[data-slot=code-value]").GetAttribute("data-language"));
        Assert.Equal("Secret", cut.Find("[data-slot=code-label]").TextContent);
        Assert.Equal("false", cut.Find("[data-slot=code-secret-toggle]").GetAttribute("aria-pressed"));

        cut.Find("[data-slot=code-secret-toggle]").Click();
        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal("sk_live_9876543210", cut.Find("[data-slot=code-value]").TextContent);
        Assert.Equal("true", cut.Find("[data-slot=code-secret-toggle]").GetAttribute("aria-pressed"));
        Assert.Equal("Conceal", cut.Find("[data-slot=code-secret-toggle] span").TextContent);

        cut.Find("[data-slot=code-secret-toggle]").Click();
        Assert.Equal("••••••••3210", cut.Find("[data-slot=code-value]").TextContent);
    }

    [Theory]
    [InlineData("abcd", 4, "••••••••")]
    [InlineData("abc", 4, "•••••••" + "•")]
    [InlineData("abcdefghij", 4, "••••••••ghij")]
    [InlineData("abcdefghij", 0, "••••••••••")]
    [InlineData("", 4, "••••••••")]
    public void A_short_secret_is_fully_masked_and_a_long_one_keeps_its_visible_end(string value, int visible, string shown)
    {
        var cut = RenderSecret(value, p => p.Add(x => x.VisibleChars, visible));

        Assert.Equal(shown, cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void The_copy_button_copies_the_raw_code()
    {
        SetRendererInfo(new RendererInfo("Server", true));
        var copy = JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true);
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp));

        cut.Find("[data-slot=code-copy]").Click();

        var invocation = Assert.Single(copy.Invocations);
        Assert.Equal(Sample, invocation.Arguments[0]);
    }

    [Fact]
    public void The_copy_button_of_a_secret_copies_the_whole_value_while_it_is_masked()
    {
        SetRendererInfo(new RendererInfo("Server", true));
        var copy = JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true);
        var cut = RenderSecret("sk_live_9876543210");

        cut.Find("[data-slot=code-copy]").Click();

        Assert.Equal("sk_live_9876543210", Assert.Single(copy.Invocations).Arguments[0]);
        Assert.Equal("••••••••3210", cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void Copy_false_hides_the_copy_button()
    {
        Assert.Empty(RenderCode(Sample, p => p.Add(x => x.Copy, false)).FindAll("[data-slot=code-copy]"));
        Assert.Empty(RenderSecret("abc", p => p.Add(x => x.Copy, false)).FindAll("[data-slot=code-copy]"));
    }

    [Fact]
    public void Nothing_is_copied_in_static_rendering()
    {
        SetRendererInfo(new RendererInfo("Static", false));
        var copy = JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true);
        var cut = RenderCode(Sample);

        cut.Find("[data-slot=code-copy]").Click();

        Assert.Empty(copy.Invocations);
    }

    [Fact]
    public void A_code_over_the_size_cap_falls_back_to_plain_text()
    {
        var large = string.Concat(Enumerable.Repeat("int x = 1; // padding\n", 6_000));
        Assert.True(large.Length > CodeHighlighter.MaxLength);

        var cut = RenderCode(large, p => p.Add(x => x.Language, CodeLanguage.CSharp));

        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
        Assert.Equal(large, cut.Find("[data-slot=code-value]").TextContent);
        Assert.Null(cut.Find("[data-slot=code-value]").GetAttribute("data-language"));
    }

    [Fact]
    public void Changing_the_code_or_the_language_re_highlights()
    {
        var cut = RenderCode("var a = 1;", p => p.Add(x => x.Language, CodeLanguage.CSharp));
        Assert.Contains(cut.FindAll("span[data-token=keyword]"), t => t.TextContent == "var");

        cut.Render(p => p.Add(x => x.Code, "SELECT 1").Add(x => x.Language, CodeLanguage.Sql));
        Assert.Contains(cut.FindAll("span[data-token=keyword]"), t => t.TextContent == "SELECT");
        Assert.DoesNotContain(cut.FindAll("span[data-token=keyword]"), t => t.TextContent == "var");

        cut.Render(p => p.Add(x => x.Language, null));
        Assert.Empty(cut.FindAll("[data-slot=code-value] span"));
    }

    [Fact]
    public void A_code_block_with_a_label_keeps_it_and_highlights()
    {
        var cut = RenderCode("{\"a\": 1}", p => p.Add(x => x.Language, CodeLanguage.Json).Add(x => x.Label, "Payload"));

        Assert.Equal("Payload", cut.Find("[data-slot=code-label]").TextContent);
        Assert.Contains(cut.FindAll("span[data-token=property]"), t => t.TextContent == "\"a\"");
    }

    [Fact]
    public void Lines_are_plain_inline_text_until_numbers_or_emphasis_are_asked_for()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp));

        Assert.Empty(cut.FindAll("[data-slot=code-line]"));
    }

    [Fact]
    public void Each_line_is_its_own_element_and_the_text_content_stays_the_code()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.Language, CodeLanguage.CSharp).Add(x => x.LineNumbers, true));

        var lines = cut.FindAll("[data-slot=code-line]");
        Assert.Equal(5, lines.Count);
        Assert.Equal(["1", "2", "3", "4", "5"], lines.Select(l => l.GetAttribute("data-line")));
        Assert.Equal("public class Greeter\n", lines[0].TextContent);
        Assert.Equal("}\n", lines[4].TextContent);
        Assert.Equal(Sample, cut.Find("[data-slot=code-value]").TextContent);
        Assert.All(lines, l => Assert.Contains("before:content-[attr(data-line)]", l.ClassName));
        Assert.Contains(cut.FindAll("[data-slot=code-line] span[data-token=keyword]"), t => t.TextContent == "class");
    }

    [Fact]
    public void Line_numbers_are_off_by_default_when_only_emphasis_is_used()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.HighlightLines, [2]));

        Assert.All(cut.FindAll("[data-slot=code-line]"), l => Assert.DoesNotContain("before:content", l.ClassName));
        Assert.Equal(Sample, cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void Emphasized_lines_take_an_accent_tinted_fill_and_an_accent_gutter_marker()
    {
        var cut = RenderCode(Sample, p => p.Add(x => x.HighlightLines, [2, new CodeLineRange(4, 5)]));

        var marked = cut.FindAll("[data-slot=code-line]")
            .Select(l => (Line: l.GetAttribute("data-line"), On: l.GetAttribute("data-highlighted") == "true", l.ClassName))
            .ToList();
        Assert.Equal(["2", "4", "5"], marked.Where(m => m.On).Select(m => m.Line));
        Assert.All(marked.Where(m => m.On), m =>
        {
            Assert.Contains("bg-(--klyroo-primary)/15", m.ClassName);
            Assert.Contains("border-(--klyroo-primary)", m.ClassName);
        });
        Assert.All(marked.Where(m => !m.On), m =>
        {
            Assert.Contains("border-transparent", m.ClassName);
            Assert.DoesNotContain("bg-(--klyroo-primary)/15", m.ClassName);
        });
    }

    [Fact]
    public void Lines_past_the_end_and_a_missing_final_newline_are_handled()
    {
        var cut = RenderCode("a\nb", p => p.Add(x => x.HighlightLines, [2, 9]));

        var lines = cut.FindAll("[data-slot=code-line]");
        Assert.Equal(2, lines.Count);
        Assert.Equal("a\n", lines[0].TextContent);
        Assert.Equal("b", lines[1].TextContent);
        Assert.Equal("true", lines[1].GetAttribute("data-highlighted"));
        Assert.Null(lines[0].GetAttribute("data-highlighted"));
    }

    [Fact]
    public void Blank_lines_keep_their_own_element_and_a_multi_line_token_is_cut_per_line()
    {
        var cut = RenderCode("/* a\n\n b */\nx", p => p.Add(x => x.Language, CodeLanguage.CSharp).Add(x => x.LineNumbers, true));

        var lines = cut.FindAll("[data-slot=code-line]");
        Assert.Equal(4, lines.Count);
        Assert.Equal("\n", lines[1].TextContent);
        Assert.All(lines.Take(3), l => Assert.Single(l.QuerySelectorAll("span[data-token=comment]")));
        Assert.Equal("/* a\n\n b */\nx", cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void An_empty_code_has_no_line()
    {
        var cut = RenderCode("", p => p.Add(x => x.LineNumbers, true));

        Assert.Empty(cut.FindAll("[data-slot=code-line]"));
        Assert.Equal("", cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void Ui_and_Class_override_the_slots()
    {
        var cut = RenderCode(Sample, p => p
            .Add(x => x.Language, CodeLanguage.CSharp)
            .Add(x => x.Class, "mt-8")
            .Add(x => x.Ui, new CodeBlockUi { Pre = "rounded-none", TokenKeyword = "text-red-500" }));

        Assert.Contains("mt-8", cut.Find("[data-slot=code-block]").ClassName);
        Assert.Contains("rounded-none", cut.Find("[data-slot=code-value]").ClassName);
        Assert.Contains(cut.FindAll("span[data-token=keyword]"), t => t.ClassName!.Contains("text-red-500"));
        Assert.DoesNotContain("text-(--klyroo-primary-text)", cut.Markup);
    }

    [Fact]
    public void The_theme_of_the_app_can_add_classes_to_a_slot()
    {
        Services.AddKlyrooDisplay(d => d.CodeBlock.Slot("label", "tracking-wide"));

        var cut = RenderCode(Sample);

        Assert.Contains("tracking-wide", cut.Find("[data-slot=code-label]").ClassName);
    }

    [Fact]
    public void Additional_attributes_reach_the_root()
    {
        var cut = RenderCode(Sample, p => p.AddUnmatched("data-testid", "snippet"));

        Assert.Equal("snippet", cut.Find("[data-slot=code-block]").GetAttribute("data-testid"));
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
