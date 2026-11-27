using Xunit;

namespace Klyroo.Display.Tests;

public sealed class CodeHighlighterTests
{
    private const string CSharp = """""
        #region helpers
        using System;
        namespace Demo;

        /// <summary>Doc.</summary>
        [Obsolete("x"), Serializable]
        public sealed class Greeter(string name) : IGreeter
        {
            private const int Max = 0x1F_FF;
            private readonly List<string> _items = new();
            public string Greet(int n = 3) => $"Hello {name}, {(n > 1 ? "many" : "one")}! {{literal}}";
            public string Path => @"C:\dir\""quoted""";
            public string Raw => """
                {"a": "b"} and "quotes"
                """;
            public char Q => '\'';
            // comment
            /* block
               comment */
            public double D => 1.5e-3 + .5f + 10m;
            public void Run() { var x = new Greeter("a"); Console.WriteLine(x.Greet()); }
        }
        #endregion
        """"";

    private const string JavaScript = """
        #!/usr/bin/env node
        import { a } from "./a.js";
        // line
        /* block */
        export async function load(url, opts = {}) {
          const re = /ab+c[/\]]/gi, n = 10 / 2 / 5;
          const msg = `hi ${user.name + `nested ${deep({ a: 1 })}`} bye \` tick`;
          if (!url) return null;
          @decorator class Foo extends Bar { #priv = 1; static x = 0x10n; }
          const o = { 'k': "v", n: 1_000, ...rest }; o?.p.q(1);
        }
        """;

    private const string TypeScript = """
        interface User { id: number; name?: string }
        type Id = string | number;
        enum Color { Red, Green }
        export class Repo<T extends User> implements Store<T> {
          private items: Map<string, T> = new Map();
          get(id: string): T | undefined { return this.items.get(id); }
        }
        """;

    private const string Json = """
        // jsonc
        {
          "name": "x\"y",
          "n": -12.5e+3,
          "ok": true, "nothing": null,
          "list": [1, 2, {"deep": "v"}], /* c */
          "esc": "a\\"
        }
        """;

    private const string Html = """
        <!DOCTYPE html>
        <!-- comment -->
        <html lang="en">
        <head>
          <style>.a > b:hover { color: #fff; margin: 0 auto; }</style>
          <script>const x = "s"; // c
          function f() { return 1; }</script>
        </head>
        <body class='x' data-a=b disabled>
          <KlyDisplay Kind="@Kind" @onclick="@(() => Go())" />
          <p>Hello @Model.Name, mail a@b.com @* razor *@ &amp; more</p>
          <br/><img src="a.png">
        </body>
        </html>
        """;

    private const string Xml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
          <![CDATA[ raw <text> ]]>
          <script>not javascript</script>
        </Project>
        """;

    private const string Css = """
        @charset "utf-8";
        @import url(foo.css);
        @media (min-width: 600px) and (max-width: 900px) {
          .card > .title:hover, #main a[href^="http"]::before { color: rgb(0 0 0 / 50%); margin: -1.5em auto !important; background: url(data:image/png;base64,AAAA) }
        }
        /* c */
        :root { --brand: #3b82f6; width: calc(100% - 2rem); }
        @keyframes spin { from { transform: rotate(0deg) } 50% { opacity: .5 } to { transform: rotate(360deg) } }
        """;

    private const string Scss = """
        $primary: #333; // line comment
        @mixin button($c) { color: $c; }
        .btn { @include button($primary); &:hover { color: darken($primary, 10%); } .#{$name}-x { width: #{$w}px; } }
        """;

    private const string Less = """
        @color: #4d926f; // c
        .bordered(@w: 2px) { border: @w solid black }
        #header { color: @color; .bordered(4px); }
        """;

    private const string Sql = """
        -- comment
        /* block */
        SELECT u.id, COUNT(*) AS total, 'it''s' AS s, "Quoted", `tick`
        FROM users u LEFT JOIN orders o ON o.user_id = u.id AND o.total > 10.5
        WHERE u.name LIKE '%a%' AND u.id IN (1, 2) AND u.d = @date AND x = :named AND y = $1
        GROUP BY u.id HAVING COUNT(*) > 1 ORDER BY total DESC LIMIT 10;
        CREATE TABLE t (id INT PRIMARY KEY, name VARCHAR(50) NOT NULL DEFAULT 'x');
        DO $$ BEGIN PERFORM 1; END $$;
        """;

    private const string Python = """""
        #!/usr/bin/env python
        import os
        @decorator(1)
        class Foo(Base):
            """Doc "string"
            more"""
            def method(self, x: int = 0x1F) -> str:
                s = f"v={x!r}" + r'\d+' + b"by" + 'it\'s'
                return os.path.join("a", 'b') if x is not None else str(1_000.5e3j)
        print(Foo().method(1))
        """"";

    private const string Bash = """
        #!/bin/bash
        # comment
        set -euo pipefail
        NAME="world $USER ${HOME}/x" ; COUNT=3 ./run.sh --flag -v 'single $quoted'
        if [ -f "$FILE" ]; then echo "yes #not comment" | grep -q yes && cat <<EOF
        heredoc $body "unbalanced
        EOF
        fi
        for i in 1 2 3; do echo $((i + 1)) $(date +%s) `uname`; done
        function greet { echo "hi $1"; }
        """;

    private const string Yaml = """
        # comment
        ---
        name: demo   # trailing
        "quoted key": 'it''s'
        version: 1.2
        enabled: true
        nothing: null
        url: http://example.com:8080/x
        list:
          - a
          - key: value
            other: [1, two, {x: 3}]
        anchors: &a !!str *a
        text: |
          literal block: not a key
            # not a comment
        after: done
        """;

    public static TheoryData<string, string> Samples => new()
    {
        { "csharp", CSharp }, { "javascript", JavaScript }, { "typescript", TypeScript }, { "json", Json }, { "html", Html }, { "xml", Xml },
        { "css", Css }, { "scss", Scss }, { "less", Less }, { "sql", Sql }, { "python", Python }, { "bash", Bash }, { "yaml", Yaml },
    };

    private static string Join(IEnumerable<CodeToken> tokens) => string.Concat(tokens.Select(t => t.Text));

    private static bool Has(IReadOnlyList<CodeToken> tokens, CodeTokenKind kind, string text) => tokens.Any(t => t.Kind == kind && t.Text == text);

    // The samples are named by what a Markdown fence or a file extension would say; the highlighter takes the typed language.
    private static CodeLanguage? Lang(string? name) => CodeHighlighter.ResolveLanguage(name);

    private static string? NameOf(CodeLanguage? language) => language is { } l ? CodeHighlighter.Name(l) : null;

    private static IReadOnlyList<CodeToken> Tokens(string language, string code) => CodeHighlighter.Tokenize(code, Lang(language));

    [Theory]
    [MemberData(nameof(Samples))]
    public void Tokens_concatenate_to_the_input_for_every_prefix_of_a_sample(string language, string code)
    {
        // Every prefix is an unterminated string, comment, tag or block somewhere: none may throw, lose text or produce an empty token.
        for (var length = 0; length <= code.Length; length++)
        {
            var prefix = code[..length];
            var tokens = CodeHighlighter.Tokenize(prefix, Lang(language));
            Assert.Equal(prefix, Join(tokens));
            Assert.All(tokens, t => Assert.NotEqual(0, t.Text.Length));
        }
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public void Tokens_concatenate_to_the_input_for_every_suffix_of_a_sample(string language, string code)
    {
        for (var start = 0; start < code.Length; start += 3)
        {
            var suffix = code[start..];
            Assert.Equal(suffix, Join(CodeHighlighter.Tokenize(suffix, Lang(language))));
        }
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public void Garbage_and_random_slices_never_throw_or_lose_text(string language, string code)
    {
        var random = new Random(12345);
        const string alphabet = "\"'`\\/*#@<>!?-=:;,.(){}[]$&|% \t\r\n_aZ09{{}}\u00e9\u4e2d\ud83d\ude00";
        for (var round = 0; round < 300; round++)
        {
            var garbage = new string(Enumerable.Range(0, random.Next(0, 200)).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
            Assert.Equal(garbage, Join(CodeHighlighter.Tokenize(garbage, Lang(language))));

            var from = random.Next(0, code.Length);
            var length = random.Next(0, code.Length - from);
            var slice = code.Substring(from, length);
            Assert.Equal(slice, Join(CodeHighlighter.Tokenize(slice, Lang(language))));

            var mixed = slice + garbage + code[..random.Next(0, code.Length)];
            Assert.Equal(mixed, Join(CodeHighlighter.Tokenize(mixed, Lang(language))));
        }
    }

    [Theory]
    [InlineData("csharp")]
    [InlineData("javascript")]
    [InlineData("html")]
    [InlineData("css")]
    [InlineData("scss")]
    [InlineData("sql")]
    [InlineData("python")]
    [InlineData("bash")]
    [InlineData("yaml")]
    [InlineData("json")]
    public void Pathological_repetitions_finish_quickly_and_keep_the_text(string language)
    {
        foreach (var unit in new[] { "(;", "\"a\" ", "/ ", "$\"{", "${", "`${", "<a ", "'", "{", "#{", "<<E ", "a: ", "- ", "/*", "\\", "@" })
        {
            var code = string.Concat(Enumerable.Repeat(unit, 20_000));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var tokens = CodeHighlighter.Tokenize(code, Lang(language));
            watch.Stop();
            Assert.Equal(code, Join(tokens));
            Assert.True(watch.ElapsedMilliseconds < 3000, $"{language} took {watch.ElapsedMilliseconds} ms on '{unit}'");
        }
    }

    [Fact]
    public void Csharp_tokens()
    {
        var t = Tokens("cs", CSharp);
        Assert.True(Has(t, CodeTokenKind.Keyword, "public"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "class"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "#region helpers"));
        Assert.True(Has(t, CodeTokenKind.Type, "Greeter"));
        Assert.True(Has(t, CodeTokenKind.Type, "IGreeter"));
        Assert.True(Has(t, CodeTokenKind.Function, "Greet"));
        Assert.True(Has(t, CodeTokenKind.Function, "WriteLine"));
        Assert.True(Has(t, CodeTokenKind.Property, "Obsolete"));
        Assert.True(Has(t, CodeTokenKind.Property, "Serializable"));
        Assert.True(Has(t, CodeTokenKind.Number, "0x1F_FF"));
        Assert.True(Has(t, CodeTokenKind.Number, "1.5e-3"));
        Assert.True(Has(t, CodeTokenKind.Number, ".5f"));
        Assert.True(Has(t, CodeTokenKind.Number, "10m"));
        Assert.True(Has(t, CodeTokenKind.Comment, "// comment"));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.Comment } && x.Text.StartsWith("/* block") && x.Text.EndsWith("*/"));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.Comment } && x.Text.StartsWith("/// <summary>"));
        // The interpolated string, with its nested string and escaped braces, is one token.
        Assert.True(Has(t, CodeTokenKind.String, "$\"Hello {name}, {(n > 1 ? \"many\" : \"one\")}! {{literal}}\""));
        Assert.True(Has(t, CodeTokenKind.String, "@\"C:\\dir\\\"\"quoted\"\"\""));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.StartsWith("\"\"\"") && x.Text.EndsWith("\"\"\"") && x.Text.Contains("\"quotes\""));
        Assert.True(Has(t, CodeTokenKind.String, "'\\''"));
        Assert.DoesNotContain(t, x => x.Kind == CodeTokenKind.Keyword && x.Text == "Max");
    }

    [Fact]
    public void Csharp_comment_markers_inside_strings_stay_in_the_string()
    {
        var t = Tokens("csharp", "var u = \"http://x\"; var v = 1; // c");
        Assert.True(Has(t, CodeTokenKind.String, "\"http://x\""));
        Assert.Single(t, x => x.Kind == CodeTokenKind.Comment);
    }

    [Fact]
    public void An_unterminated_csharp_string_ends_with_its_line()
    {
        var t = Tokens("csharp", "var s = \"open;\nint x = 1;");
        Assert.True(Has(t, CodeTokenKind.String, "\"open;"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "int"));
        Assert.True(Has(t, CodeTokenKind.Number, "1"));
    }

    [Fact]
    public void Javascript_tokens()
    {
        var t = Tokens("js", JavaScript);
        Assert.True(Has(t, CodeTokenKind.Comment, "#!/usr/bin/env node"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "import"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "async"));
        Assert.True(Has(t, CodeTokenKind.Function, "load"));
        Assert.True(Has(t, CodeTokenKind.String, "\"./a.js\""));
        Assert.True(Has(t, CodeTokenKind.String, "/ab+c[/\\]]/gi"));
        Assert.True(Has(t, CodeTokenKind.Type, "Foo"));
        Assert.True(Has(t, CodeTokenKind.Property, "@decorator"));
        Assert.True(Has(t, CodeTokenKind.Property, "#priv"));
        Assert.True(Has(t, CodeTokenKind.Number, "0x10n"));
        Assert.True(Has(t, CodeTokenKind.Number, "1_000"));
        Assert.True(Has(t, CodeTokenKind.Function, "q"));
        Assert.True(Has(t, CodeTokenKind.Property, "p"));
        Assert.True(Has(t, CodeTokenKind.Property, "name"));
        // Division is not a regular expression.
        Assert.True(Has(t, CodeTokenKind.Number, "10"));
        Assert.True(Has(t, CodeTokenKind.Number, "5"));
        // The nested template literal and the expression inside it are tokenized.
        Assert.True(Has(t, CodeTokenKind.Function, "deep"));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.StartsWith("`hi ${"));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.EndsWith(" bye \\` tick`"));
    }

    [Fact]
    public void Typescript_tokens()
    {
        var t = Tokens("tsx", TypeScript);
        Assert.True(Has(t, CodeTokenKind.Keyword, "interface"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "number"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "enum"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "implements"));
        Assert.True(Has(t, CodeTokenKind.Type, "User"));
        Assert.True(Has(t, CodeTokenKind.Type, "Repo"));
        Assert.True(Has(t, CodeTokenKind.Function, "get"));
        Assert.DoesNotContain(Tokens("js", "const number = 1;"), x => x.Kind == CodeTokenKind.Keyword && x.Text == "number");
    }

    [Fact]
    public void Json_tells_keys_from_values()
    {
        var t = Tokens("json", Json);
        Assert.True(Has(t, CodeTokenKind.Property, "\"name\""));
        Assert.True(Has(t, CodeTokenKind.Property, "\"deep\""));
        Assert.True(Has(t, CodeTokenKind.String, "\"x\\\"y\""));
        Assert.True(Has(t, CodeTokenKind.String, "\"v\""));
        Assert.True(Has(t, CodeTokenKind.Number, "-12.5e+3"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "true"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "null"));
        Assert.True(Has(t, CodeTokenKind.Comment, "// jsonc"));
        Assert.True(Has(t, CodeTokenKind.Comment, "/* c */"));
        Assert.True(Has(t, CodeTokenKind.String, "\"a\\\\\""));
    }

    [Fact]
    public void Html_tokens_with_embedded_script_and_style()
    {
        var t = Tokens("html", Html);
        Assert.True(Has(t, CodeTokenKind.Keyword, "<!DOCTYPE html>"));
        Assert.True(Has(t, CodeTokenKind.Comment, "<!-- comment -->"));
        Assert.True(Has(t, CodeTokenKind.Tag, "html"));
        Assert.True(Has(t, CodeTokenKind.Tag, "KlyDisplay"));
        Assert.True(Has(t, CodeTokenKind.Property, "lang"));
        Assert.True(Has(t, CodeTokenKind.String, "\"en\""));
        Assert.True(Has(t, CodeTokenKind.String, "'x'"));
        Assert.True(Has(t, CodeTokenKind.String, "b"));
        Assert.True(Has(t, CodeTokenKind.Property, "disabled"));
        Assert.True(Has(t, CodeTokenKind.Property, "@onclick"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "@Model.Name"));
        Assert.True(Has(t, CodeTokenKind.Comment, "@* razor *@"));
        Assert.DoesNotContain(t, x => x.Kind == CodeTokenKind.Keyword && x.Text.StartsWith("@b"));
        // CSS and JavaScript inside <style> and <script>.
        Assert.True(Has(t, CodeTokenKind.Type, ".a"));
        Assert.True(Has(t, CodeTokenKind.Property, "color"));
        Assert.True(Has(t, CodeTokenKind.Number, "#fff"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "const"));
        Assert.True(Has(t, CodeTokenKind.Function, "f"));
        Assert.True(Has(t, CodeTokenKind.Comment, "// c"));
    }

    [Fact]
    public void Xml_does_not_treat_script_as_javascript()
    {
        var t = Tokens("xml", Xml);
        Assert.True(Has(t, CodeTokenKind.Keyword, "<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
        Assert.True(Has(t, CodeTokenKind.Tag, "Project"));
        Assert.True(Has(t, CodeTokenKind.Property, "Sdk"));
        Assert.True(Has(t, CodeTokenKind.Tag, "TargetFramework"));
        Assert.True(Has(t, CodeTokenKind.String, "<![CDATA[ raw <text> ]]>"));
        Assert.DoesNotContain(t, x => x.Kind == CodeTokenKind.Keyword && x.Text == "not");
    }

    [Fact]
    public void Css_tokens()
    {
        var t = Tokens("css", Css);
        Assert.True(Has(t, CodeTokenKind.Keyword, "@media"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "@import"));
        Assert.True(Has(t, CodeTokenKind.Property, "min-width"));
        Assert.True(Has(t, CodeTokenKind.Number, "600px"));
        Assert.True(Has(t, CodeTokenKind.Type, ".card"));
        Assert.True(Has(t, CodeTokenKind.Type, "#main"));
        Assert.True(Has(t, CodeTokenKind.Tag, "a"));
        Assert.True(Has(t, CodeTokenKind.Keyword, ":hover"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "::before"));
        Assert.True(Has(t, CodeTokenKind.Property, "href"));
        Assert.True(Has(t, CodeTokenKind.Property, "color"));
        Assert.True(Has(t, CodeTokenKind.Function, "rgb"));
        Assert.True(Has(t, CodeTokenKind.Number, "50%"));
        Assert.True(Has(t, CodeTokenKind.Number, "-1.5em"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "!important"));
        Assert.True(Has(t, CodeTokenKind.Function, "url"));
        Assert.True(Has(t, CodeTokenKind.String, "(data:image/png;base64,AAAA)"));
        Assert.True(Has(t, CodeTokenKind.Property, "--brand"));
        Assert.True(Has(t, CodeTokenKind.Number, "#3b82f6"));
        Assert.True(Has(t, CodeTokenKind.Function, "calc"));
        Assert.True(Has(t, CodeTokenKind.Tag, "from"));
        Assert.True(Has(t, CodeTokenKind.Number, "0deg"));
        Assert.True(Has(t, CodeTokenKind.Comment, "/* c */"));
    }

    [Fact]
    public void Scss_and_less_tokens()
    {
        var scss = Tokens("scss", Scss);
        Assert.True(Has(scss, CodeTokenKind.Property, "$primary"));
        Assert.True(Has(scss, CodeTokenKind.Comment, "// line comment"));
        Assert.True(Has(scss, CodeTokenKind.Keyword, "@mixin"));
        Assert.True(Has(scss, CodeTokenKind.Keyword, "@include"));
        Assert.True(Has(scss, CodeTokenKind.Function, "darken"));
        Assert.True(Has(scss, CodeTokenKind.Type, ".btn"));

        var less = Tokens("less", Less);
        Assert.True(Has(less, CodeTokenKind.Property, "@color"));
        Assert.True(Has(less, CodeTokenKind.Comment, "// c"));
        Assert.True(Has(less, CodeTokenKind.Property, "border"));
        Assert.True(Has(less, CodeTokenKind.Type, "#header"));

        // // is not a comment in plain CSS.
        Assert.DoesNotContain(Tokens("css", "a { background: url(//x.com/a.png) }"), x => x.Kind == CodeTokenKind.Comment);
    }

    [Fact]
    public void Sql_tokens()
    {
        var t = Tokens("sql", Sql);
        Assert.True(Has(t, CodeTokenKind.Comment, "-- comment"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "SELECT"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "LEFT"));
        Assert.True(Has(t, CodeTokenKind.Function, "COUNT"));
        Assert.True(Has(t, CodeTokenKind.String, "'it''s'"));
        Assert.True(Has(t, CodeTokenKind.String, "\"Quoted\""));
        Assert.True(Has(t, CodeTokenKind.Property, "`tick`"));
        Assert.True(Has(t, CodeTokenKind.Number, "10.5"));
        Assert.True(Has(t, CodeTokenKind.Property, "@date"));
        Assert.True(Has(t, CodeTokenKind.Property, ":named"));
        Assert.True(Has(t, CodeTokenKind.Property, "$1"));
        Assert.True(Has(t, CodeTokenKind.Type, "INT"));
        Assert.True(Has(t, CodeTokenKind.Type, "VARCHAR"));
        Assert.True(Has(t, CodeTokenKind.String, "$$ BEGIN PERFORM 1; END $$"));
        Assert.True(Has(Tokens("sql", "select 1 from t"), CodeTokenKind.Keyword, "select"));
    }

    [Fact]
    public void Python_tokens()
    {
        var t = Tokens("py", Python);
        Assert.True(Has(t, CodeTokenKind.Comment, "#!/usr/bin/env python"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "import"));
        Assert.True(Has(t, CodeTokenKind.Property, "@decorator"));
        Assert.True(Has(t, CodeTokenKind.Type, "Foo"));
        Assert.True(Has(t, CodeTokenKind.Function, "method"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "self"));
        Assert.True(Has(t, CodeTokenKind.Type, "int"));
        Assert.True(Has(t, CodeTokenKind.Number, "0x1F"));
        Assert.True(Has(t, CodeTokenKind.String, "f\"v={x!r}\""));
        Assert.True(Has(t, CodeTokenKind.String, "r'\\d+'"));
        Assert.True(Has(t, CodeTokenKind.String, "b\"by\""));
        Assert.True(Has(t, CodeTokenKind.String, "'it\\'s'"));
        Assert.True(Has(t, CodeTokenKind.Number, "1_000.5e3j"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "None"));
        Assert.True(Has(t, CodeTokenKind.Function, "join"));
        Assert.True(Has(t, CodeTokenKind.Function, "print"));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.StartsWith("\"\"\"Doc \"string\"") && x.Text.EndsWith("\"\"\""));
    }

    [Fact]
    public void Bash_tokens()
    {
        var t = Tokens("sh", Bash);
        Assert.True(Has(t, CodeTokenKind.Comment, "# comment"));
        Assert.True(Has(t, CodeTokenKind.Function, "set"));
        Assert.True(Has(t, CodeTokenKind.Property, "-euo"));
        Assert.True(Has(t, CodeTokenKind.Property, "NAME"));
        Assert.True(Has(t, CodeTokenKind.Property, "COUNT"));
        Assert.True(Has(t, CodeTokenKind.String, "\"world $USER ${HOME}/x\""));
        Assert.True(Has(t, CodeTokenKind.Function, "./run.sh"));
        Assert.True(Has(t, CodeTokenKind.Property, "--flag"));
        Assert.True(Has(t, CodeTokenKind.String, "'single $quoted'"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "if"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "then"));
        Assert.True(Has(t, CodeTokenKind.Function, "echo"));
        Assert.True(Has(t, CodeTokenKind.Function, "grep"));
        Assert.True(Has(t, CodeTokenKind.Function, "cat"));
        Assert.True(Has(t, CodeTokenKind.String, "\"yes #not comment\""));
        Assert.False(Has(t, CodeTokenKind.Property, "$USER"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "fi"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "done"));
        Assert.True(Has(t, CodeTokenKind.Function, "date"));
        // The heredoc body, with an unbalanced quote, is one string and ends at its delimiter.
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.StartsWith("heredoc $body \"unbalanced") && x.Text.EndsWith("EOF"));
        Assert.Equal(1, t.Count(x => x.Kind == CodeTokenKind.Keyword && x.Text == "fi"));
        Assert.True(Has(t, CodeTokenKind.Function, "greet"));
    }

    [Fact]
    public void Yaml_tokens()
    {
        var t = Tokens("yml", Yaml);
        Assert.True(Has(t, CodeTokenKind.Comment, "# comment"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "---"));
        Assert.True(Has(t, CodeTokenKind.Property, "name"));
        Assert.True(Has(t, CodeTokenKind.Comment, "# trailing"));
        Assert.True(Has(t, CodeTokenKind.Property, "\"quoted key\""));
        Assert.True(Has(t, CodeTokenKind.String, "'it''s'"));
        Assert.True(Has(t, CodeTokenKind.Number, "1.2"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "true"));
        Assert.True(Has(t, CodeTokenKind.Keyword, "null"));
        Assert.DoesNotContain(t, x => x.Kind == CodeTokenKind.Property && x.Text.StartsWith("http"));
        Assert.True(Has(t, CodeTokenKind.Property, "other"));
        Assert.True(Has(t, CodeTokenKind.Number, "1"));
        Assert.True(Has(t, CodeTokenKind.Property, "x"));
        Assert.True(Has(t, CodeTokenKind.Type, "&a"));
        Assert.True(Has(t, CodeTokenKind.Type, "*a"));
        Assert.True(Has(t, CodeTokenKind.Type, "!!str"));
        // The block scalar keeps its colon and its hash as text; the key after it is a key again.
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.Contains("literal block: not a key"));
        Assert.Contains(t, x => x is { Kind: CodeTokenKind.String } && x.Text.Contains("# not a comment"));
        Assert.True(Has(t, CodeTokenKind.Property, "after"));
    }

    [Fact]
    public void Crlf_line_endings_are_kept_and_do_not_confuse_the_scanners()
    {
        foreach (var (language, code) in new[]
        {
            ("csharp", "var a = \"x\r\nvar b = 1; // c\r\n"), ("yaml", "a: 1\r\nb: |\r\n  text\r\nc: 2\r\n"), ("bash", "echo hi\r\n# c\r\nls\r\n"),
            ("python", "x = 'a\r\ny = 2\r\n"), ("css", "a {\r\n color: red;\r\n}\r\n"), ("html", "<p\r\nclass='a'>x</p>\r\n"),
        })
        {
            Assert.Equal(code, Join(CodeHighlighter.Tokenize(code, Lang(language))));
        }
        Assert.True(Has(Tokens("yaml", "a: 1\r\nb: |\r\n  text\r\nc: 2\r\n"), CodeTokenKind.Property, "c"));
        Assert.True(Has(Tokens("csharp", "var a = \"x\r\nvar b = 1;"), CodeTokenKind.Number, "1"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("brainfuck")]
    [InlineData("text")]
    public void An_unknown_language_is_plain_text(string? language)
    {
        var tokens = CodeHighlighter.Tokenize("if (x) { return \"a\"; }", Lang(language));
        var token = Assert.Single(tokens);
        Assert.Equal(CodeTokenKind.Plain, token.Kind);
        Assert.Equal("if (x) { return \"a\"; }", token.Text);
    }

    [Fact]
    public void Empty_or_null_code_has_no_tokens()
    {
        Assert.Empty(CodeHighlighter.Tokenize(null, Lang("cs")));
        Assert.Empty(CodeHighlighter.Tokenize("", Lang("cs")));
        Assert.Empty(CodeHighlighter.Tokenize("", null));
    }

    [Fact]
    public void A_code_over_the_size_caps_is_one_plain_token()
    {
        var longCode = string.Concat(Enumerable.Repeat("int x = 1;\n", CodeHighlighter.MaxLines + 1));
        Assert.True(longCode.Length < CodeHighlighter.MaxLength);
        Assert.Equal(CodeTokenKind.Plain, Assert.Single(CodeHighlighter.Tokenize(longCode, Lang("cs"))).Kind);

        var wide = new string('a', CodeHighlighter.MaxLength + 1);
        Assert.Equal(CodeTokenKind.Plain, Assert.Single(CodeHighlighter.Tokenize(wide, Lang("cs"))).Kind);
        Assert.False(CodeHighlighter.CanHighlight(wide, Lang("cs")));

        var limit = string.Join("\n", Enumerable.Repeat("int x = 1;", CodeHighlighter.MaxLines));
        Assert.True(CodeHighlighter.CanHighlight(limit, Lang("cs")));
        Assert.True(CodeHighlighter.Tokenize(limit, Lang("cs")).Count > 1);
    }

    [Theory]
    [InlineData("cs", "csharp")]
    [InlineData("C#", "csharp")]
    [InlineData("CSharp", "csharp")]
    [InlineData(".cs", "csharp")]
    [InlineData("language-js", "javascript")]
    [InlineData("jsx", "javascript")]
    [InlineData("TSX", "typescript")]
    [InlineData("jsonc", "json")]
    [InlineData("razor", "html")]
    [InlineData("svg", "html")]
    [InlineData("csproj", "xml")]
    [InlineData("scss", "scss")]
    [InlineData("less", "less")]
    [InlineData("psql", "sql")]
    [InlineData("py", "python")]
    [InlineData("zsh", "bash")]
    [InlineData("shell", "bash")]
    [InlineData("yml", "yaml")]
    public void Aliases_and_extensions_resolve_to_one_canonical_language(string name, string expected)
    {
        Assert.Equal(expected, NameOf(CodeHighlighter.ResolveLanguage(name)));
    }

    [Theory]
    [InlineData("Program.cs", "csharp")]
    [InlineData("/a/b/app.min.js?v=1", "javascript")]
    [InlineData("https://x.test/styles.css#top", "css")]
    [InlineData("deploy.yml", "yaml")]
    [InlineData("Dockerfile", null)]
    [InlineData("notes.txt", null)]
    [InlineData(null, null)]
    public void File_names_resolve_from_their_extension(string? name, string? expected)
    {
        Assert.Equal(expected, NameOf(CodeHighlighter.ResolveFileName(name)));
    }

    [Fact]
    public void Every_listed_language_resolves_to_itself_and_highlights()
    {
        foreach (var language in CodeHighlighter.Languages)
        {
            Assert.Equal(language, CodeHighlighter.ResolveLanguage(CodeHighlighter.Name(language)));
        }
        Assert.Equal(13, CodeHighlighter.Languages.Count);
        Assert.Equal(Enum.GetValues<CodeLanguage>().Order(), CodeHighlighter.Languages.Order());
        Assert.Equal(CodeHighlighter.Languages.Select(CodeHighlighter.Name).Order(StringComparer.Ordinal), CodeHighlighter.Languages.Select(CodeHighlighter.Name));
    }

    [Fact]
    public void An_undefined_language_has_no_name_and_is_scanned_as_plain_text()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CodeHighlighter.Name((CodeLanguage)99));
        Assert.Equal(CodeTokenKind.Plain, Assert.Single(CodeHighlighter.Tokenize("var a = 1;", (CodeLanguage)99)).Kind);
    }

    [Fact]
    public void Highlight_returns_escaped_markup_whose_text_is_the_code()
    {
        const string code = "if (a < b && c) { return \"x\"; } // 1 > 0";
        var html = CodeHighlighter.Highlight(code, CodeLanguage.CSharp);

        Assert.Contains("<span data-token=\"keyword\">if</span>", html);
        Assert.Contains("<span data-token=\"string\">&quot;x&quot;</span>", html);
        Assert.Contains("&lt;", html);
        Assert.Contains("&amp;&amp;", html);
        Assert.DoesNotContain("<b", html);
        Assert.DoesNotContain("class=", html);
        var text = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", ""));
        Assert.Equal(code, text);
    }

    [Fact]
    public void Highlight_of_markup_cannot_inject_a_tag()
    {
        var html = CodeHighlighter.Highlight("<script>alert(1)</script>", CodeLanguage.Html);

        Assert.DoesNotContain("<script", html);
        Assert.Contains("&lt;", html);
        Assert.Equal("", CodeHighlighter.Highlight("", CodeLanguage.Html));
    }

    [Theory]
    [InlineData(CodeTokenKind.Keyword, "keyword")]
    [InlineData(CodeTokenKind.Punctuation, "punctuation")]
    [InlineData(CodeTokenKind.Plain, "plain")]
    public void Token_names_are_the_values_of_the_data_token_attribute(CodeTokenKind kind, string name) =>
        Assert.Equal(name, CodeHighlighter.TokenName(kind));

    [Fact]
    public void A_line_range_contains_its_ends_and_a_line_converts_to_one()
    {
        CodeLineRange single = 3;
        Assert.True(single.Contains(3));
        Assert.False(single.Contains(2));
        Assert.False(single.Contains(4));

        var run = CodeLineRange.Of(6, 9);
        Assert.True(run.Contains(6) && run.Contains(9) && run.Contains(7));
        Assert.False(run.Contains(5) || run.Contains(10));
        Assert.False(new CodeLineRange(9, 6).Contains(7));
        Assert.Equal(new CodeLineRange(4, 4), new CodeLineRange(4));
    }
}
