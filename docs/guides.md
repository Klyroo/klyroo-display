# Guides

Task-oriented recipes. Each one is complete and compiled against the current code. They assume the setup from [Getting started](getting-started.md): `AddKlyrooDisplay()` is registered, the namespaces are imported and the stylesheets are loaded.

- [Highlight a code sample](#highlight-a-code-sample)
- [Number the lines and emphasize some of them](#number-the-lines-and-emphasize-some-of-them)
- [Highlight code outside Blazor](#highlight-code-outside-blazor)
- [Mask a secret and let the user reveal it](#mask-a-secret-and-let-the-user-reveal-it)
- [Open files from an attachment list](#open-files-from-an-attachment-list)
- [Reveal a long file list a few files at a time](#reveal-a-long-file-list-a-few-files-at-a-time)
- [Preview a gallery with a converted Office document](#preview-a-gallery-with-a-converted-office-document)
- [Preview CSV, Markdown and text that you already hold](#preview-csv-markdown-and-text-that-you-already-hold)
- [Limit how many times a clip can be started](#limit-how-many-times-a-clip-can-be-started)
- [Detect file kinds and parse CSV on the server](#detect-file-kinds-and-parse-csv-on-the-server)
- [Restyle a block](#restyle-a-block)
- [Translate the built-in texts](#translate-the-built-in-texts)
- [Test your usage](#test-your-usage)

## Highlight a code sample

Goal: show a snippet with syntax colors, a label and a copy button.

```razor
<KlyCodeBlock Code="@Snippet" Language="CodeLanguage.CSharp" Label="Order.cs" />

@code {
    private const string Snippet = "public record Order(int Id) { public string Label => $\"Order {Id}\"; }";
}
```

`Language` is a `CodeLanguage`: `Bash`, `CSharp`, `Css`, `Html`, `JavaScript`, `Json`, `Less`, `Python`, `Scss`, `Sql`, `TypeScript`, `Xml` or `Yaml` (`CodeHighlighter.Languages` lists them). Without a `Language` the code is plain text, and `Label` defaults to the name of the language, or "Code". The text of the block stays plain text: selecting it and the copy button give the code as it is, and markup in the code is escaped. `Highlight="false"` and a code over 100,000 characters or 5,000 lines render without colors. The file preview highlights text files the same way, from the extension of the file name.

When the language comes from outside the program, such as a Markdown fence (`language-js`), a configuration value or a file name, resolve it first. A name that is not supported gives `null`, which is plain text:

```razor
<KlyCodeBlock Code="@file.Content" Language="CodeHighlighter.ResolveFileName(file.Name)" Label="@file.Name" />
```

`ResolveLanguage` takes a name, an alias or a file extension (`csharp`, `cs`, `ts`, `yml`, `.sh`, `language-py`, ignoring case) and `ResolveFileName` takes a file name or a URL.

## Number the lines and emphasize some of them

Goal: show a query with line numbers, and draw the eye to two of its lines in a tutorial.

```razor
<KlyCodeBlock Code="@Query" Language="CodeLanguage.Sql" LineNumbers
              HighlightLines="@([2, new CodeLineRange(4, 5)])" />

@code {
    private const string Query = "SELECT id, total\nFROM orders\nWHERE status = 'paid'\n  AND total > 100\n  AND shipped_at IS NULL;\n";
}
```

`HighlightLines` is a list of `CodeLineRange`, counted from 1 and inclusive at both ends: `2` is line 2 (an `int` converts to a one-line range) and `new CodeLineRange(4, 5)` is lines 4 and 5. An emphasized line gets a neutral fill and an accent marker in the gutter, and carries `data-highlighted="true"`. Lines past the end of the code are ignored. `LineNumbers` draws the numbers with CSS (`content: attr(data-line)`), so selecting and copying the code never picks them up. Either option makes the block write one element per line (`data-slot="code-line"`, with its number in `data-line`); without them the code is a flat run of tokens. Each line keeps its line break, so the text content is still exactly the code.

## Highlight code outside Blazor

Goal: color a snippet in an e-mail, a static page or a PDF, with no component.

```csharp
using Klyroo.Display;

var html = CodeHighlighter.Highlight("SELECT 1 -- one", CodeLanguage.Sql);
// <span data-token="keyword">SELECT</span> <span data-token="number">1</span> <span data-token="comment">-- one</span>
```

`Highlight` returns markup that is safe to place inside an element: every character of the code is escaped, so a `<script>` in the code stays text. Text with a category is wrapped in `<span data-token="keyword">` (`keyword`, `string`, `number`, `comment`, `type`, `function`, `property`, `tag` or `punctuation`), and the rest is bare. The spans carry no style, so you color them with your own CSS on `[data-token]`.

For your own rendering, `CodeHighlighter.Tokenize(code, language)` returns `CodeToken` values with a `CodeTokenKind` and a `Text`. The texts concatenate to exactly the code. Both methods live in `Klyroo.Display.Core`, they never throw, and they work in a NativeAOT app.

## Mask a secret and let the user reveal it

Goal: show an API key as `••••••••3210`, with a Reveal button and a copy button.

```razor
<KlySecretBlock Label="API key" Value="@ApiKey" VisibleChars="4" />

@code {
    [Parameter, EditorRequired] public string ApiKey { get; set; } = "";
}
```

The block shows eight bullets followed by the last `VisibleChars` characters (4 by default). A value that is not longer than `VisibleChars` is fully masked. While it is masked, only the bullets and those characters are in the page. Reveal and copy need an interactive render mode, so in static rendering the secret stays masked. The copy button copies the whole value, even while it is masked, and a secret is never highlighted.

The mask is a display feature and does not protect the value. In Interactive Server the value stays in server memory, but in WebAssembly it is in the browser's memory, and the copy button copies the real value. Do not send a secret to a client that must not hold it.

## Open files from an attachment list

Goal: a list of attachments that opens in the preview when clicked.

```razor
<KlyFileList Title="Attachments" Files="_attachments" />

@code {
    private readonly IReadOnlyList<FilePreviewFile> _attachments =
    [
        new() { Src = "/files/spec.pdf", Name = "Specification.pdf", Size = 1_572_864 },
        new() { Src = "/files/metrics.csv", Size = 18_400, UpdatedAt = new DateTimeOffset(2026, 11, 20, 9, 30, 0, TimeSpan.Zero) },
        new() { Src = "/api/files/8f2c", Name = "Q3 summary", Mime = "application/pdf" },
    ];
}
```

Each file shows an extension badge, its name (the last segment of `Src` when `Name` is not set), its size and its last-modified date when it has them. A click opens the whole list as a gallery, starting at the clicked file, when a `KlyFilePreviewProvider` is mounted and the page is interactive, with the same `FilePreviewFile` values the service takes (`Content`, `Rendition`, `Details` and the rest). Without a provider, or before the page is interactive, the name is a plain link to `Src`.

The badge comes from the extension of the name, or from `Mime` when the name has none: the last file above has no extension, so `Mime` gives it `PDF`. Mount `<KlyFilePreviewProvider />` somewhere interactive, see [Getting started](getting-started.md#6-mount-the-file-preview-provider).

## Reveal a long file list a few files at a time

Goal: show three files and let the user load three more per click.

```razor
<KlyFileList Title="Documents" Files="_files" Max="3" Step="3" />
```

`Max` sets how many files show first and `Step` how many each click on Show more adds. Without `Step`, one click shows everything. The gallery that a click opens always holds the whole list, hidden files included. Reveal state lives in the component, so it resets when the component is recreated.

## Preview a gallery with a converted Office document

Goal: open a Word document and an image in one overlay, with the document shown through its PDF conversion.

```razor
<KlyButton Label="Open documents" OnClick="OpenAll" />

@code {
    private void OpenAll()
    {
        var files = new List<FilePreviewFile>
        {
            new()
            {
                Src = "/files/contract.docx",
                Name = "contract.docx",
                Size = 48_213,
                UpdatedAt = new DateTimeOffset(2026, 11, 20, 9, 30, 0, TimeSpan.Zero),
                // The browser cannot render a .docx, so show a PDF conversion and keep the original for download.
                Rendition = new FilePreviewFile { Src = "/files/contract.pdf", Name = "contract.pdf" },
                Details = [new FilePreviewDetail("Owner", "Legal team")],
            },
            new() { Src = "/files/site-plan.png", Name = "site-plan.png" },
        };

        Preview.OpenGallery(files, new FilePreviewOptions { Index = 0, Loop = true, PdfPage = 2 });
    }
}
```

The provider cannot render `.docx`, so `Rendition` points at a PDF that stands in for it. The header shows a notice that the preview is converted and offers the original for download. With a `PdfPage`, the browser viewer opens on that page, unless the URL already contains a `#` fragment. With no `Mode`, the container comes from `ResolveContainer`: a drawer here, because the gallery contains a PDF. `Loop = true` makes Previous and Next wrap around.

Mount `<KlyFilePreviewProvider />` somewhere interactive for the overlay to appear, see [Getting started](getting-started.md#6-mount-the-file-preview-provider).

## Preview CSV, Markdown and text that you already hold

Goal: preview an export without serving it as a file.

```razor
<KlyFilePreviewProvider MarkdownRenderer="RenderMarkdown" />

<KlyButton Label="Preview export" OnClick="OpenExport" />

@code {
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().DisableHtml().Build();

    private static string RenderMarkdown(string markdown) => Markdown.ToHtml(markdown, Pipeline);

    private void OpenExport()
    {
        // CSV, Markdown and text files are rendered from Content: the provider never downloads Src for them.
        var csv = "Region,Revenue\nEurope,148900\nUS,192400";
        var notes = "# Q3 notes\n\nRevenue grew in **both** regions.";

        Preview.OpenGallery(
        [
            new FilePreviewFile { Src = "/exports/q3.csv", Name = "q3.csv", Content = csv },
            new FilePreviewFile { Src = "/exports/q3.md", Name = "q3.md", Content = notes },
        ]);
    }
}
```

CSV, Markdown and text files render from `Content`, and the provider never downloads `Src` for them: without `Content` they show an empty table or an empty block. `Src` still serves the download link, so keep it meaningful.

Markdown is shown as source unless you give the provider a `MarkdownRenderer`, and then the user gets a Preview and a Source tab. The renderer's output is inserted as markup without sanitizing. The example uses Markdig with `DisableHtml()` so that raw HTML in the file is not passed through. Use a configuration like it for any Markdown you did not write.

The CSV table shows the first 1,000 rows, then says so in its footer.

## Limit how many times a clip can be started

Goal: a listening exercise that lets the learner start a 30-second extract twice, without seeking.

```razor
<KlyAudioPlayer Src="/media/listening-1.mp3" Title="Listening extract 1"
                Start="15" End="45" MaxPlays="2" Seekable="false"
                @bind-Plays="_plays" OnLimitReached="OnLimitReached" />

@if (_exhausted)
{
    <p>You have used both plays.</p>
}

@code {
    private int _plays;
    private bool _exhausted;

    private void OnLimitReached() => _exhausted = true;
}
```

`Start` and `End` define the clip, so positions and the total length are relative to it (30 seconds here). `MaxPlays` caps the number of starts, and `@bind-Plays` keeps the count in your component: the player increments its own copy and raises `PlaysChanged`, and you pass the value back on each render. Once both plays are used and the second one ends, the player shows a locked state and raises `OnLimitReached`.

Play starts the extract at 15 seconds and the player pauses it at 45, which ends the play. The player needs an interactive render mode: under static server rendering the control bar shows but nothing plays. To end a play from your own code, for example when a timer runs out, call `CompletePlayAsync` on the component (reachable with `@ref`).

## Detect file kinds and parse CSV on the server

Goal: use the headless helpers in an API, with no Blazor involved.

```csharp
using Klyroo.Display;

// The MIME type wins over the extension, and parameters are ignored.
var fromMime = FilePreviewService.DetectKind("download", "text/csv; charset=utf-8");   // Csv

// A signed URL still gives its extension: the query string is ignored.
var fromUrl = FilePreviewService.DetectKind("https://cdn.example.com/a/report.PDF?token=abc");   // Pdf

// When neither tells the kind, state it.
var file = new FilePreviewFile { Src = "/api/files/8f2c", Name = "Q3 summary", Kind = FilePreviewKind.Pdf };
var kind = FilePreviewService.DetectKind(file);   // Pdf
```

`DetectKind` tries the explicit `Kind`, then the MIME type, then the extension of the name or URL (ignoring a query string or fragment), and returns `FilePreviewKind.Other` when nothing matches. A MIME type it does not recognize, such as `application/octet-stream`, falls through to the extension.

Parsing an uploaded file in a Minimal API:

```csharp
using Klyroo.Display;

app.MapPost("/preview/csv", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var table = FilePreviewService.ParseCsv(await reader.ReadToEndAsync(), request.Query["name"]);

    return Results.Ok(new
    {
        Delimiter = table.Delimiter.ToString(),
        table.Headers,
        RowCount = table.Rows.Count,
        table.Truncated,
    });
});
```

`ParseCsv` never throws on malformed input: an unterminated quote reads to the end of the text. It keeps at most `maxRows` data rows (1,000 by default) and sets `Truncated` when there were more. Rows are not padded, so a row can be shorter or longer than the header.

## Restyle a block

Goal: change the look of the code block for the whole app, and of one instance.

```csharp
builder.Services.AddKlyrooDisplay(d =>
{
    d.CodeBlock.Slot("label", "tracking-wide");
    d.CodeBlock.Slot("tokenKeyword", "font-semibold text-(--klyroo-primary-text)");
});
```

```razor
<KlyCodeBlock Code="@Snippet" Language="CodeLanguage.CSharp"
              Class="mt-8" Ui="@(new CodeBlockUi { Pre = "rounded-none", TokenString = "text-(--klyroo-warning-text)" })" />
```

The blocks follow the slot model of Klyroo.UI. `KlyrooDisplayOptions.CodeBlock`, `SecretBlock` and `FileList` are themes with named slots (for the code block: `root`, `header`, `label`, `copy`, `pre`, `line`, `lineHighlighted`, `lineNumbered` and a `token*` slot per token category), and the classes you add are merged after the defaults, the last class winning. `CodeBlockUi`, `SecretBlockUi` and `FileListUi` carry the same slots for one instance, and `Class` goes to the root slot last. The themes become read-only when the first component renders, so change them in `AddKlyrooDisplay`. Write every class in full: `klyroo-display.css` only contains the classes the library itself uses, so a new utility needs your own Tailwind build (see the Klyroo.UI documentation) or a CSS rule.

## Translate the built-in texts

Goal: change the labels, counters and accessible names that the components write themselves, in one language.

The components already speak the twelve languages of Klyroo.UI: each shows the `DisplayTexts` set of its current locale (`DisplayLocales.Fr`, `DisplayLocales.De`...), the locale its Klyroo.UI texts come from, so switching language with `KlyLocaleSelect` switches these labels too. English and French are reviewed by a native speaker; the other sets are drafts. To change some texts of a locale, give a function that returns the new set:

```csharp
builder.Services.AddKlyrooDisplay(d => d
    .Texts("fr", t => t with
    {
        ShowMore = n => $"Voir {n} de plus",
        NoPlaysLeft = "Vous avez utilisé toutes vos lectures",
        PlaysLeft = n => n == 1 ? "Encore 1 lecture" : $"Encore {n} lectures",
    })
    .Texts("de", t => t with { ClosePreview = "Schließen" }));
```

`DisplayTexts` is a record, so `with` keeps every text you do not name. A text with a number or a name is a function, which lets a language choose its plural forms and word order. A locale you added to Klyroo.UI ("fr-CA", "tlh") starts from the set `KlyLocales.ResolveCode` picks for it (French for "fr-CA", English for an unknown language), then the changes registered for its own code apply. The sets become read-only once a component has resolved a text, so register the changes in `AddKlyrooDisplay`. Titles, labels and values you pass to the components are shown as given.

## Test your usage

The state and helpers of `Klyroo.Display.Core` need no renderer, so test them with plain xUnit:

```csharp
using Klyroo.Display;
using Xunit;

namespace Snippets.Tests;

public sealed class CsvPreviewTests
{
    [Fact]
    public void Semicolon_delimited_text_is_detected_and_numeric_columns_are_flagged()
    {
        var table = FilePreviewService.ParseCsv("Item;Price\nDesk;299.90\nChair;149.00");

        Assert.Equal(';', table.Delimiter);
        Assert.Equal(["Item", "Price"], table.Headers);
        Assert.Equal(2, table.Rows.Count);
        Assert.False(table.NumericColumns[0]);
        Assert.True(table.NumericColumns[1]);
    }

    [Fact]
    public void Gallery_wraps_around_when_looping()
    {
        var preview = new FilePreviewService();
        preview.OpenGallery(
            [new FilePreviewFile { Src = "/a.png" }, new FilePreviewFile { Src = "/b.pdf" }],
            new FilePreviewOptions { Loop = true });

        preview.Next();
        preview.Next();

        Assert.Equal(0, preview.CurrentIndex);
    }
}
```

For components, use [bUnit](https://bunit.dev). Register `AddKlyrooDisplay()` on `Services` before rendering. A test that renders a player also needs `JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;`, because the players import that module once interactive. Select elements by their `data-slot` attributes, which the components expose:

```csharp
using Bunit;
using Klyroo.Display;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Snippets.Tests;

public sealed class DisplayComponentTests : BunitContext
{
    public DisplayComponentTests()
    {
        Services.AddKlyrooDisplay();
    }

    [Fact]
    public void Emphasized_lines_are_marked_and_the_text_stays_the_code()
    {
        const string code = "SELECT 1\nFROM t\n";
        var cut = Render<KlyCodeBlock>(p => p
            .Add(x => x.Code, code)
            .Add(x => x.Language, CodeLanguage.Sql)
            .Add(x => x.HighlightLines, [2]));

        Assert.Equal("true", cut.FindAll("[data-slot=code-line]")[1].GetAttribute("data-highlighted"));
        Assert.Equal(code, cut.Find("[data-slot=code-value]").TextContent);
    }

    [Fact]
    public void Opening_a_file_shows_the_overlay_and_closing_removes_it()
    {
        var cut = Render<KlyFilePreviewProvider>();
        var preview = Services.GetRequiredService<FilePreviewService>();

        preview.Open(new FilePreviewFile { Src = "/files/notes.txt", Name = "notes.txt", Content = "Hello" });
        cut.Render();

        Assert.Equal("notes.txt", cut.Find("[data-slot=file-preview-title]").TextContent);

        cut.Find("[data-slot=file-preview-close]").Click();

        Assert.Empty(cut.FindAll("[data-slot=file-preview-provider]"));
    }
}
```

Rendering `KlyFilePreviewProvider` first matters: the service reports `IsMounted` from then on, and `Open` needs a mounted provider to show anything. The provider test calls `cut.Render()` after changing the service from outside the component, to be sure the markup is current. Both test classes above are compiled and run against this version of the library.
