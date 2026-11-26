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
    public static string TokenName(CodeTokenKind kind) => kind switch
    {
        CodeTokenKind.Keyword => "keyword",
        CodeTokenKind.String => "string",
        CodeTokenKind.Number => "number",
        CodeTokenKind.Comment => "comment",
        CodeTokenKind.Type => "type",
        CodeTokenKind.Function => "function",
        CodeTokenKind.Property => "property",
        CodeTokenKind.Tag => "tag",
        CodeTokenKind.Punctuation => "punctuation",
        _ => "plain",
    };

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

    public static RenderFragment Fragment(IReadOnlyList<CodeToken> tokens) => builder => Build(builder, tokens);

    public static void Build(RenderTreeBuilder builder, IReadOnlyList<CodeToken> tokens)
    {
        var sequence = 0;
        foreach (var token in tokens)
        {
            if (TokenClass(token.Kind) is not { } cssClass)
            {
                builder.AddContent(sequence++, token.Text);
                continue;
            }
            builder.OpenElement(sequence++, "span");
            builder.AddAttribute(sequence++, "data-token", TokenName(token.Kind));
            builder.AddAttribute(sequence++, "class", cssClass);
            builder.AddContent(sequence++, token.Text);
            builder.CloseElement();
        }
    }
}
