namespace Klyroo.Display.Maui;

/// <summary>Cuts the tokens of <see cref="CodeHighlighter"/> into the lines of the code, and gives each category of token its color.</summary>
internal static class DisplayCodeLines
{
    /// <summary>
    /// The lines of a code as lists of tokens: a token that spans several lines is cut at each line break. The line break stays at the end of its line
    /// (the text of all the lines is the code itself), and the last line break ends a line instead of starting an empty one.
    /// </summary>
    public static List<List<CodeToken>> Split(IReadOnlyList<CodeToken> tokens)
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
                current.Add(new CodeToken(token.Kind, text[start..(newline + 1)]));
                lines.Add(current);
                current = [];
                start = newline + 1;
            }
        }
        if (current.Count > 0) lines.Add(current);
        return lines;
    }

    /// <summary>The tokens of a line without its line break (<c>\n</c> or <c>\r\n</c>): a native label would draw it as an empty line more.</summary>
    public static List<CodeToken> WithoutBreak(IReadOnlyList<CodeToken> line)
    {
        var result = new List<CodeToken>(line);
        if (result.Count == 0) return result;
        var last = result[^1];
        var text = last.Text;
        if (text.EndsWith('\n')) text = text[..^1];
        if (text.EndsWith('\r')) text = text[..^1];
        if (text.Length == 0) result.RemoveAt(result.Count - 1);
        else result[^1] = last with { Text = text };
        return result;
    }

    /// <summary>The tokens of a whole code without its last line break.</summary>
    public static List<CodeToken> WithoutFinalBreak(IReadOnlyList<CodeToken> tokens)
    {
        var lines = Split(tokens);
        var result = new List<CodeToken>(tokens.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            if (i == lines.Count - 1) result.AddRange(WithoutBreak(lines[i]));
            else result.AddRange(lines[i]);
        }
        return result;
    }

    /// <summary>The color of a category of token, or <see langword="null"/> for plain text, which keeps the text color of the block. The same tokens as the web theme.</summary>
    public static KlyThemeColor? ColorOf(CodeTokenKind kind) => kind switch
    {
        CodeTokenKind.Keyword => KlyTheme.Resolve(KlyColor.Primary).Text,
        CodeTokenKind.String => KlyTheme.Resolve(KlyColor.Success).Text,
        CodeTokenKind.Number => KlyTheme.Resolve(KlyColor.Warning).Text,
        CodeTokenKind.Comment => KlyTheme.TextMuted,
        CodeTokenKind.Type => KlyTheme.Resolve(KlyColor.Secondary).Text,
        CodeTokenKind.Function => KlyTheme.Resolve(KlyColor.Info).Text,
        CodeTokenKind.Property => KlyTheme.Resolve(KlyColor.Info).Text,
        CodeTokenKind.Tag => KlyTheme.Resolve(KlyColor.Error).Text,
        CodeTokenKind.Punctuation => DisplayUi.TextToned,
        _ => null,
    };

    /// <summary>The number of lines of a text: a trailing line break ends the last line instead of starting an empty one, and an empty text has no lines.</summary>
    public static int CountLines(string text)
    {
        if (text.Length == 0) return 0;
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        var count = normalized.Count(c => c == '\n') + 1;
        return normalized.EndsWith('\n') ? count - 1 : count;
    }
}
