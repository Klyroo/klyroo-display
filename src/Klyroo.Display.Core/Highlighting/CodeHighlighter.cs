using System.Diagnostics.CodeAnalysis;

namespace Klyroo.Display;

/// <summary>A language <see cref="CodeHighlighter"/> can highlight. A code in no language (<see langword="null"/> where a <see cref="CodeLanguage"/>? is taken) is plain text.</summary>
public enum CodeLanguage
{
    /// <summary>Bash and other POSIX shells (<c>sh</c>, <c>zsh</c>, <c>shell</c>).</summary>
    Bash,

    /// <summary>C# (<c>cs</c>, <c>csx</c>).</summary>
    CSharp,

    /// <summary>CSS.</summary>
    Css,

    /// <summary>HTML and Razor (<c>htm</c>, <c>razor</c>, <c>cshtml</c>, <c>svg</c>): <c>script</c> and <c>style</c> contents are highlighted as JavaScript and CSS.</summary>
    Html,

    /// <summary>JavaScript (<c>js</c>, <c>jsx</c>, <c>mjs</c>, <c>cjs</c>).</summary>
    JavaScript,

    /// <summary>JSON, with comments (<c>jsonc</c>).</summary>
    Json,

    /// <summary>Less.</summary>
    Less,

    /// <summary>Python (<c>py</c>).</summary>
    Python,

    /// <summary>SCSS.</summary>
    Scss,

    /// <summary>SQL.</summary>
    Sql,

    /// <summary>TypeScript (<c>ts</c>, <c>tsx</c>, <c>mts</c>, <c>cts</c>).</summary>
    TypeScript,

    /// <summary>XML and its dialects (<c>xaml</c>, <c>csproj</c>, <c>props</c>, <c>xsd</c>).</summary>
    Xml,

    /// <summary>YAML (<c>yml</c>).</summary>
    Yaml,
}

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
/// <para>The work is capped: a code longer than <see cref="MaxLength"/> characters or <see cref="MaxLines"/> lines, and a code with no language, come back as one <see cref="CodeTokenKind.Plain"/> token.</para>
/// <para>Supported languages (<see cref="CodeLanguage"/>, with the aliases <see cref="ResolveLanguage"/> accepts): <c>csharp</c> (cs, c#, csx), <c>javascript</c> (js, jsx, mjs, cjs), <c>typescript</c> (ts, tsx, mts, cts),
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

    private static readonly Dictionary<string, CodeLanguage> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = CodeLanguage.CSharp, ["cs"] = CodeLanguage.CSharp, ["c#"] = CodeLanguage.CSharp, ["csx"] = CodeLanguage.CSharp, ["cake"] = CodeLanguage.CSharp,
        ["javascript"] = CodeLanguage.JavaScript, ["js"] = CodeLanguage.JavaScript, ["jsx"] = CodeLanguage.JavaScript, ["mjs"] = CodeLanguage.JavaScript, ["cjs"] = CodeLanguage.JavaScript, ["node"] = CodeLanguage.JavaScript,
        ["typescript"] = CodeLanguage.TypeScript, ["ts"] = CodeLanguage.TypeScript, ["tsx"] = CodeLanguage.TypeScript, ["mts"] = CodeLanguage.TypeScript, ["cts"] = CodeLanguage.TypeScript,
        ["json"] = CodeLanguage.Json, ["jsonc"] = CodeLanguage.Json, ["json5"] = CodeLanguage.Json, ["webmanifest"] = CodeLanguage.Json, ["geojson"] = CodeLanguage.Json,
        ["html"] = CodeLanguage.Html, ["htm"] = CodeLanguage.Html, ["xhtml"] = CodeLanguage.Html, ["razor"] = CodeLanguage.Html, ["cshtml"] = CodeLanguage.Html, ["svg"] = CodeLanguage.Html, ["vue"] = CodeLanguage.Html, ["svelte"] = CodeLanguage.Html,
        ["xml"] = CodeLanguage.Xml, ["xaml"] = CodeLanguage.Xml, ["axaml"] = CodeLanguage.Xml, ["csproj"] = CodeLanguage.Xml, ["fsproj"] = CodeLanguage.Xml, ["vbproj"] = CodeLanguage.Xml, ["props"] = CodeLanguage.Xml, ["targets"] = CodeLanguage.Xml,
        ["xsd"] = CodeLanguage.Xml, ["xsl"] = CodeLanguage.Xml, ["xslt"] = CodeLanguage.Xml, ["rss"] = CodeLanguage.Xml, ["atom"] = CodeLanguage.Xml, ["plist"] = CodeLanguage.Xml, ["resx"] = CodeLanguage.Xml, ["nuspec"] = CodeLanguage.Xml, ["slnx"] = CodeLanguage.Xml,
        ["css"] = CodeLanguage.Css, ["scss"] = CodeLanguage.Scss, ["less"] = CodeLanguage.Less,
        ["sql"] = CodeLanguage.Sql, ["psql"] = CodeLanguage.Sql, ["mysql"] = CodeLanguage.Sql, ["pgsql"] = CodeLanguage.Sql, ["tsql"] = CodeLanguage.Sql,
        ["python"] = CodeLanguage.Python, ["py"] = CodeLanguage.Python, ["pyw"] = CodeLanguage.Python, ["python3"] = CodeLanguage.Python,
        ["bash"] = CodeLanguage.Bash, ["sh"] = CodeLanguage.Bash, ["zsh"] = CodeLanguage.Bash, ["shell"] = CodeLanguage.Bash, ["ksh"] = CodeLanguage.Bash, ["console"] = CodeLanguage.Bash, ["shellscript"] = CodeLanguage.Bash,
        ["yaml"] = CodeLanguage.Yaml, ["yml"] = CodeLanguage.Yaml,
    };

    /// <summary>Every supported language, in alphabetical order of <see cref="Name"/>.</summary>
    public static IReadOnlyList<CodeLanguage> Languages { get; } =
        [CodeLanguage.Bash, CodeLanguage.CSharp, CodeLanguage.Css, CodeLanguage.Html, CodeLanguage.JavaScript, CodeLanguage.Json, CodeLanguage.Less,
         CodeLanguage.Python, CodeLanguage.Scss, CodeLanguage.Sql, CodeLanguage.TypeScript, CodeLanguage.Xml, CodeLanguage.Yaml];

    /// <summary>The canonical lowercase name of a language (<c>csharp</c>, <c>typescript</c>), as it is written in a <c>language-</c> class and a code block label.</summary>
    /// <param name="language">The language.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="language"/> is not a defined <see cref="CodeLanguage"/>.</exception>
    public static string Name(CodeLanguage language) => language switch
    {
        CodeLanguage.Bash => "bash",
        CodeLanguage.CSharp => "csharp",
        CodeLanguage.Css => "css",
        CodeLanguage.Html => "html",
        CodeLanguage.JavaScript => "javascript",
        CodeLanguage.Json => "json",
        CodeLanguage.Less => "less",
        CodeLanguage.Python => "python",
        CodeLanguage.Scss => "scss",
        CodeLanguage.Sql => "sql",
        CodeLanguage.TypeScript => "typescript",
        CodeLanguage.Xml => "xml",
        CodeLanguage.Yaml => "yaml",
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Not a defined CodeLanguage."),
    };

    /// <summary>Maps a language name, an alias or a file extension, which come from outside the program (a Markdown fence, a file name), to a <see cref="CodeLanguage"/>.</summary>
    /// <param name="languageOrExtension">A name such as <c>csharp</c>, <c>c#</c>, <c>TSX</c>, <c>.yml</c> or <c>language-js</c>. The case, a leading dot and a <c>language-</c> prefix are ignored.</param>
    /// <returns>The language, or <see langword="null"/> when it is not supported.</returns>
    public static CodeLanguage? ResolveLanguage(string? languageOrExtension)
    {
        if (string.IsNullOrWhiteSpace(languageOrExtension)) return null;
        var name = languageOrExtension.Trim();
        if (name.StartsWith("language-", StringComparison.OrdinalIgnoreCase)) name = name["language-".Length..];
        name = name.TrimStart('.');
        return Aliases.TryGetValue(name, out var language) ? language : null;
    }

    /// <summary>Maps a file name or URL to a <see cref="CodeLanguage"/>, from its extension.</summary>
    /// <param name="fileNameOrUrl">A file name or URL. A query string or fragment is ignored.</param>
    /// <returns>The language, or <see langword="null"/> when there is no extension or the language is not supported.</returns>
    public static CodeLanguage? ResolveFileName(string? fileNameOrUrl)
    {
        var extension = FilePreviewService.ExtractExtension(fileNameOrUrl);
        return extension is null ? null : ResolveLanguage(extension);
    }

    /// <summary>Whether <see cref="Tokenize"/> would highlight this code: the language is set and the code is within the size caps.</summary>
    /// <param name="code">The source code.</param>
    /// <param name="language">The language, or <see langword="null"/> for plain text.</param>
    public static bool CanHighlight([NotNullWhen(true)] string? code, [NotNullWhen(true)] CodeLanguage? language) =>
        !string.IsNullOrEmpty(code) && language is not null && WithinCaps(code);

    /// <summary>Splits source code into tokens.</summary>
    /// <param name="code">The source code. It may be malformed, truncated or not code at all.</param>
    /// <param name="language">The language, or <see langword="null"/> for plain text.</param>
    /// <returns>
    /// The tokens, in order: their texts concatenate to exactly <paramref name="code"/>. An empty or <see langword="null"/> code gives no token.
    /// A code that is not highlighted (no language, or over <see cref="MaxLength"/> characters or <see cref="MaxLines"/> lines) gives one <see cref="CodeTokenKind.Plain"/> token.
    /// </returns>
    public static IReadOnlyList<CodeToken> Tokenize(string? code, CodeLanguage? language)
    {
        if (string.IsNullOrEmpty(code)) return [];
        if (language is not { } canonical || !WithinCaps(code)) return [new CodeToken(CodeTokenKind.Plain, code)];
        try
        {
            var writer = new TokenWriter(code);
            Create(canonical, code, writer).Run();
            return writer.Finish();
        }
        catch (Exception)
        {
            // A scanner never throws by design; this keeps a bug in one from breaking the rendering of the code.
            return [new CodeToken(CodeTokenKind.Plain, code)];
        }
    }

    /// <summary>
    /// Highlights a code as HTML-safe markup, for a page that is not rendered by Blazor (an e-mail, a static site, a PDF). Text that has a category is wrapped in
    /// <c>&lt;span data-token="keyword"&gt;</c> (<c>keyword</c>, <c>string</c>, <c>number</c>, <c>comment</c>, <c>type</c>, <c>function</c>, <c>property</c>, <c>tag</c> or
    /// <c>punctuation</c>), and plain text is bare. Every character of the code is escaped, so the result can be placed inside an element as it is.
    /// The spans carry no style: color them with your own CSS on <c>[data-token]</c>.
    /// </summary>
    /// <param name="code">The source code.</param>
    /// <param name="language">The language of the code.</param>
    /// <returns>The markup. Its text, once the tags are removed and the entities decoded, is exactly <paramref name="code"/>.</returns>
    public static string Highlight(string code, CodeLanguage language)
    {
        ArgumentNullException.ThrowIfNull(code);
        var html = new System.Text.StringBuilder(code.Length * 2);
        foreach (var token in Tokenize(code, language))
        {
            if (token.Kind == CodeTokenKind.Plain)
            {
                html.Append(System.Net.WebUtility.HtmlEncode(token.Text));
                continue;
            }
            html.Append("<span data-token=\"").Append(TokenName(token.Kind)).Append("\">")
                .Append(System.Net.WebUtility.HtmlEncode(token.Text)).Append("</span>");
        }
        return html.ToString();
    }

    /// <summary>The value of the <c>data-token</c> attribute of a token category (<c>keyword</c>, <c>string</c>...); <c>plain</c> for <see cref="CodeTokenKind.Plain"/>.</summary>
    /// <param name="kind">The category.</param>
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

    private static Lexer Create(CodeLanguage language, string code, TokenWriter writer) => language switch
    {
        CodeLanguage.CSharp => new CSharpLexer(code, 0, code.Length, writer),
        CodeLanguage.JavaScript => new JavaScriptLexer(code, 0, code.Length, writer, typescript: false),
        CodeLanguage.TypeScript => new JavaScriptLexer(code, 0, code.Length, writer, typescript: true),
        CodeLanguage.Json => new JsonLexer(code, 0, code.Length, writer),
        CodeLanguage.Html => new MarkupLexer(code, 0, code.Length, writer, html: true),
        CodeLanguage.Xml => new MarkupLexer(code, 0, code.Length, writer, html: false),
        CodeLanguage.Css => new CssLexer(code, 0, code.Length, writer, CssDialect.Css),
        CodeLanguage.Scss => new CssLexer(code, 0, code.Length, writer, CssDialect.Scss),
        CodeLanguage.Less => new CssLexer(code, 0, code.Length, writer, CssDialect.Less),
        CodeLanguage.Sql => new SqlLexer(code, 0, code.Length, writer),
        CodeLanguage.Python => new PythonLexer(code, 0, code.Length, writer),
        CodeLanguage.Bash => new BashLexer(code, 0, code.Length, writer),
        CodeLanguage.Yaml => new YamlLexer(code, 0, code.Length, writer),
        _ => new PlainLexer(code, 0, code.Length, writer),
    };
}
