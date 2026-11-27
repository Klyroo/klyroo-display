using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Klyroo.Display;

/// <summary>
/// Renders the tokens of <see cref="CodeHighlighter"/>. Each token that has a color is a <c>span</c> with a <c>data-token</c> attribute and a class
/// made of literal Tailwind utilities (the Tailwind scan of <c>klyroo-display.css</c> only finds class names written out in full, so they are
/// never built from pieces). The colors are the semantic text colors of Klyroo.UI, which change with light and dark mode and with the palette.
/// Plain tokens are text nodes, so a code without highlighting renders the same markup as before.
/// </summary>
internal static class CodeRendering
{
    public static string? TokenClass(CodeTokenKind kind) => kind switch
    {
        CodeTokenKind.Keyword => "text-(--klyroo-primary-text)",
        CodeTokenKind.String => "text-(--klyroo-success-text)",
        CodeTokenKind.Number => "text-(--klyroo-warning-text)",
        CodeTokenKind.Comment => "italic text-(--klyroo-text-muted)",
        CodeTokenKind.Type => "text-(--klyroo-secondary-text)",
        CodeTokenKind.Function => "text-(--klyroo-info-text)",
        CodeTokenKind.Property => "text-(--klyroo-info-text)",
        CodeTokenKind.Tag => "text-(--klyroo-error-text)",
        CodeTokenKind.Punctuation => "text-(--klyroo-text-toned)",
        _ => null,
    };

    public static RenderFragment Fragment(IReadOnlyList<CodeToken> tokens) => builder => Build(builder, tokens, TokenClass);

    /// <summary>Writes the tokens as spans and text nodes. <paramref name="tokenClass"/> gives the classes of a category, or <see langword="null"/> for bare text.</summary>
    public static void Build(RenderTreeBuilder builder, IReadOnlyList<CodeToken> tokens, Func<CodeTokenKind, string?> tokenClass)
    {
        var sequence = 0;
        foreach (var token in tokens)
        {
            sequence = AddToken(builder, sequence, token.Kind, token.Text, tokenClass);
        }
    }

    /// <summary>The lines of a code as lists of tokens: a token that spans several lines is cut at each line break. The last line break ends a line, it does not start an empty one.</summary>
    public static List<List<CodeToken>> SplitLines(IReadOnlyList<CodeToken> tokens)
    {
        var lines = new List<List<CodeToken>>();
        var current = new List<CodeToken>();
        foreach (var token in tokens)
        {
            var text = token.Text;
            var start = 0;
            while (start < text.Length)
            {
                var newline = text.IndexOf('\n', start);
                if (newline < 0)
                {
                    current.Add(new CodeToken(token.Kind, text[start..]));
                    break;
                }
                // The line break stays at the end of its line, so the text of the block is the code itself.
                current.Add(new CodeToken(token.Kind, text[start..(newline + 1)]));
                lines.Add(current);
                current = [];
                start = newline + 1;
            }
        }
        if (current.Count > 0) lines.Add(current);
        return lines;
    }

    /// <summary>
    /// Writes a code as one block-level span per line, for line numbers and emphasized lines. A line is <c>data-slot="code-line"</c> with its
    /// number in <c>data-line</c> and <c>data-highlighted="true"</c> when emphasized. The line break stays inside its line, so the text content is the code itself.
    /// </summary>
    public static void BuildLines(RenderTreeBuilder builder, IReadOnlyList<CodeToken> tokens, Func<CodeTokenKind, string?> tokenClass,
        Func<int, bool> isHighlighted, string lineClass, string idleClass, string highlightedClass, string numberedClass, bool numbered)
    {
        var sequence = 0;
        var number = 0;
        foreach (var line in SplitLines(tokens))
        {
            number++;
            var highlighted = isHighlighted(number);
            builder.OpenElement(sequence++, "span");
            builder.AddAttribute(sequence++, "class", $"{lineClass} {(highlighted ? highlightedClass : idleClass)}{(numbered ? " " + numberedClass : "")}");
            builder.AddAttribute(sequence++, "data-slot", "code-line");
            builder.AddAttribute(sequence++, "data-line", number.ToString(System.Globalization.CultureInfo.InvariantCulture));
            builder.AddAttribute(sequence++, "data-highlighted", highlighted ? "true" : null);
            foreach (var token in line)
            {
                sequence = AddToken(builder, sequence, token.Kind, token.Text, tokenClass);
            }
            builder.CloseElement();
        }
    }

    private static int AddToken(RenderTreeBuilder builder, int sequence, CodeTokenKind kind, string text, Func<CodeTokenKind, string?> tokenClass)
    {
        if (tokenClass(kind) is not { } cssClass)
        {
            builder.AddContent(sequence++, text);
            return sequence;
        }
        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "data-token", CodeHighlighter.TokenName(kind));
        builder.AddAttribute(sequence++, "class", cssClass);
        builder.AddContent(sequence++, text);
        builder.CloseElement();
        return sequence;
    }
}
