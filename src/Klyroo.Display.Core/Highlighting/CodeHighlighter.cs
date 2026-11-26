using System.Diagnostics.CodeAnalysis;

namespace Klyroo.Display;

/// <summary>The category of a <see cref="CodeToken"/>. A theme gives each category a color.</summary>
public enum CodeTokenKind
{
    /// <summary>Text with no special meaning: identifiers, whitespace, and everything in a language that is not highlighted.</summary>
    Plain,

    /// <summary>A reserved word, a literal such as <c>true</c> or <c>null</c>, a preprocessor directive, an at-rule or a document declaration.</summary>
    Keyword,

    /// <summary>A string or character literal, a regular expression literal, a heredoc body or an attribute value.</summary>
    String,

    /// <summary>A numeric literal, with its suffix or unit, and a hexadecimal color.</summary>
    Number,

    /// <summary>A line or block comment.</summary>
    Comment,

    /// <summary>The name of a type or class, or a class or identifier selector.</summary>
    Type,

    /// <summary>The name of a function or method, at its call or declaration.</summary>
    Function,

    /// <summary>A member, attribute, object key, decorator, CSS property or shell variable.</summary>
    Property,

    /// <summary>A markup tag name or a CSS element selector.</summary>
    Tag,

    /// <summary>An operator or a punctuation mark, grouped in runs.</summary>
    Punctuation,
}

/// <summary>A slice of source code and its category. The texts of the tokens of a code, in order, concatenate to exactly that code.</summary>
/// <param name="Kind">The category of the slice.</param>
/// <param name="Text">The slice. It is never empty.</param>
public readonly record struct CodeToken(CodeTokenKind Kind, string Text);

/// <summary>
/// A dependency-free syntax highlighter: it splits source code into <see cref="CodeToken"/> slices of a category, with hand-written scanners
/// (no regular expressions, no reflection), so it is safe to trim and to compile ahead of time.
/// </summary>
/// <remarks>
/// <para>The scanners are lexical, not parsers: they recognize comments, strings, numbers, keywords and a few naming heuristics (a name followed
/// by <c>(</c> is a function, a capitalized name is a type). <see cref="Tokenize"/> never throws, whatever the input: an unterminated string or
/// comment runs to the end of its line or of the code, and the tokens always concatenate to the input.</para>
/// <para>The work is capped: a code longer than <see cref="MaxLength"/> characters or <see cref="MaxLines"/> lines, and a code in a language that is not
/// supported, come back as one <see cref="CodeTokenKind.Plain"/> token.</para>
/// <para>Supported languages (and their aliases): <c>csharp</c> (cs, c#, csx), <c>javascript</c> (js, jsx, mjs, cjs), <c>typescript</c> (ts, tsx, mts, cts),
/// <c>json</c> (jsonc), <c>html</c> (htm, xhtml, razor, cshtml, svg: <c>script</c> and <c>style</c> contents are highlighted as JavaScript and CSS),
/// <c>xml</c> (xaml, csproj, props, targets, xsd, xslt, config and others), <c>css</c>, <c>scss</c>, <c>less</c>, <c>sql</c>, <c>python</c> (py),
/// <c>bash</c> (sh, shell, zsh) and <c>yaml</c> (yml).</para>
/// </remarks>
public static class CodeHighlighter
{
    /// <summary>The longest code, in characters, that is highlighted (<c>100000</c>). A longer code is returned as one plain token.</summary>
    public const int MaxLength = 100_000;

    /// <summary>The largest number of lines of a code that is highlighted (<c>5000</c>). A longer code is returned as one plain token.</summary>
    public const int MaxLines = 5_000;

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = "csharp", ["cs"] = "csharp", ["c#"] = "csharp", ["csx"] = "csharp", ["cake"] = "csharp",
        ["javascript"] = "javascript", ["js"] = "javascript", ["jsx"] = "javascript", ["mjs"] = "javascript", ["cjs"] = "javascript", ["node"] = "javascript",
        ["typescript"] = "typescript", ["ts"] = "typescript", ["tsx"] = "typescript", ["mts"] = "typescript", ["cts"] = "typescript",
        ["json"] = "json", ["jsonc"] = "json", ["json5"] = "json", ["webmanifest"] = "json", ["geojson"] = "json",
        ["html"] = "html", ["htm"] = "html", ["xhtml"] = "html", ["razor"] = "html", ["cshtml"] = "html", ["svg"] = "html", ["vue"] = "html", ["svelte"] = "html",
        ["xml"] = "xml", ["xaml"] = "xml", ["axaml"] = "xml", ["csproj"] = "xml", ["fsproj"] = "xml", ["vbproj"] = "xml", ["props"] = "xml", ["targets"] = "xml",
        ["xsd"] = "xml", ["xsl"] = "xml", ["xslt"] = "xml", ["rss"] = "xml", ["atom"] = "xml", ["plist"] = "xml", ["resx"] = "xml", ["nuspec"] = "xml", ["slnx"] = "xml",
        ["css"] = "css", ["scss"] = "scss", ["less"] = "less",
        ["sql"] = "sql", ["psql"] = "sql", ["mysql"] = "sql", ["pgsql"] = "sql", ["tsql"] = "sql",
        ["python"] = "python", ["py"] = "python", ["pyw"] = "python", ["python3"] = "python",
        ["bash"] = "bash", ["sh"] = "bash", ["zsh"] = "bash", ["shell"] = "bash", ["ksh"] = "bash", ["console"] = "bash", ["shellscript"] = "bash",
        ["yaml"] = "yaml", ["yml"] = "yaml",
    };

    /// <summary>The canonical names of the supported languages, in alphabetical order.</summary>
    public static IReadOnlyList<string> Languages { get; } =
        ["bash", "csharp", "css", "html", "javascript", "json", "less", "python", "scss", "sql", "typescript", "xml", "yaml"];

    /// <summary>Maps a language name, an alias or a file extension to the canonical name of a supported language.</summary>
    /// <param name="languageOrExtension">A name such as <c>csharp</c>, <c>c#</c>, <c>TSX</c>, <c>.yml</c> or <c>language-js</c>. The case, a leading dot and a <c>language-</c> prefix are ignored.</param>
    /// <returns>An element of <see cref="Languages"/>, or <see langword="null"/> when the language is not supported.</returns>
    public static string? ResolveLanguage(string? languageOrExtension)
    {
        if (string.IsNullOrWhiteSpace(languageOrExtension)) return null;
        var name = languageOrExtension.Trim();
        if (name.StartsWith("language-", StringComparison.OrdinalIgnoreCase)) name = name["language-".Length..];
        name = name.TrimStart('.');
        return Aliases.TryGetValue(name, out var canonical) ? canonical : null;
    }

    /// <summary>Maps a file name or URL to the canonical name of a supported language, from its extension.</summary>
    /// <param name="fileNameOrUrl">A file name or URL. A query string or fragment is ignored.</param>
    /// <returns>An element of <see cref="Languages"/>, or <see langword="null"/> when there is no extension or the language is not supported.</returns>
    public static string? ResolveFileName(string? fileNameOrUrl)
    {
        var extension = FilePreviewService.ExtractExtension(fileNameOrUrl);
        return extension is null ? null : ResolveLanguage(extension);
    }

    /// <summary>Whether <see cref="Tokenize"/> would highlight this code: the language is supported and the code is within the size caps.</summary>
    /// <param name="code">The source code.</param>
    /// <param name="language">A language name, alias or file extension, see <see cref="ResolveLanguage"/>.</param>
    public static bool CanHighlight([NotNullWhen(true)] string? code, string? language) =>
        !string.IsNullOrEmpty(code) && ResolveLanguage(language) is not null && WithinCaps(code);

    /// <summary>Splits source code into tokens.</summary>
    /// <param name="code">The source code. It may be malformed, truncated or not code at all.</param>
    /// <param name="language">A language name, alias or file extension, see <see cref="ResolveLanguage"/>. An unsupported language, or <see langword="null"/>, gives plain text.</param>
    /// <returns>
    /// The tokens, in order: their texts concatenate to exactly <paramref name="code"/>. An empty or <see langword="null"/> code gives no token.
    /// A code that is not highlighted (unsupported language, or over <see cref="MaxLength"/> characters or <see cref="MaxLines"/> lines) gives one <see cref="CodeTokenKind.Plain"/> token.
    /// </returns>
    public static IReadOnlyList<CodeToken> Tokenize(string? code, string? language)
    {
        if (string.IsNullOrEmpty(code)) return [];
        var canonical = ResolveLanguage(language);
        if (canonical is null || !WithinCaps(code)) return [new CodeToken(CodeTokenKind.Plain, code)];
        try
        {
            var writer = new TokenWriter(code);
            Create(canonical, code, writer).Run();
            var tokens = writer.Finish();
            return tokens;
        }
        catch (Exception)
        {
            // A scanner never throws by design; this keeps a bug in one from breaking the rendering of the code.
            return [new CodeToken(CodeTokenKind.Plain, code)];
        }
    }

    private static bool WithinCaps(string code)
    {
        if (code.Length > MaxLength) return false;
        var lines = 1;
        foreach (var c in code)
        {
            if (c == '\n' && ++lines > MaxLines) return false;
        }
        return true;
    }

    private static Lexer Create(string language, string code, TokenWriter writer) => language switch
    {
        "csharp" => new CSharpLexer(code, 0, code.Length, writer),
        "javascript" => new JavaScriptLexer(code, 0, code.Length, writer, typescript: false),
        "typescript" => new JavaScriptLexer(code, 0, code.Length, writer, typescript: true),
        "json" => new JsonLexer(code, 0, code.Length, writer),
        "html" => new MarkupLexer(code, 0, code.Length, writer, html: true),
        "xml" => new MarkupLexer(code, 0, code.Length, writer, html: false),
        "css" => new CssLexer(code, 0, code.Length, writer, CssDialect.Css),
        "scss" => new CssLexer(code, 0, code.Length, writer, CssDialect.Scss),
        "less" => new CssLexer(code, 0, code.Length, writer, CssDialect.Less),
        "sql" => new SqlLexer(code, 0, code.Length, writer),
        "python" => new PythonLexer(code, 0, code.Length, writer),
        "bash" => new BashLexer(code, 0, code.Length, writer),
        "yaml" => new YamlLexer(code, 0, code.Length, writer),
        _ => new PlainLexer(code, 0, code.Length, writer),
    };
}
