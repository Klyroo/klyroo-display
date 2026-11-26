# Guides

Task-oriented recipes. Each one is complete and compiled against the current code. They assume the setup from [Getting started](getting-started.md): `AddKlyrooDisplay()` is registered, the namespaces are imported and the stylesheets are loaded.

- [Show a metric with a trend and a threshold meter](#show-a-metric-with-a-trend-and-a-threshold-meter)
- [Mask a secret and let the user reveal it](#mask-a-secret-and-let-the-user-reveal-it)
- [Show progress and a deadline](#show-progress-and-a-deadline)
- [Compare two periods in a breakdown](#compare-two-periods-in-a-breakdown)
- [Reveal a long list a few items at a time](#reveal-a-long-list-a-few-items-at-a-time)
- [Preview a gallery with a converted Office document](#preview-a-gallery-with-a-converted-office-document)
- [Preview CSV, Markdown and text that you already hold](#preview-csv-markdown-and-text-that-you-already-hold)
- [Open files from an attachment list](#open-files-from-an-attachment-list)
- [Limit how many times a clip can be started](#limit-how-many-times-a-clip-can-be-started)
- [Detect file kinds and parse CSV on the server](#detect-file-kinds-and-parse-csv-on-the-server)
- [Translate the built-in texts](#translate-the-built-in-texts)
- [Test your usage](#test-your-usage)

## Show a metric with a trend and a threshold meter

Goal: a revenue figure with a change badge and a sparkline, and a storage meter that changes color as it fills.

```razor
<KlyDisplay Kind="DisplayKind.Stat" Title="Monthly revenue" Value="148,900" Unit="EUR"
            Delta="+14.2%" DeltaDirection="up" Trend="_trend" Description="Compared to October" />

<KlyDisplay Kind="DisplayKind.Meter" Title="Storage" MeterValue="46" MeterMax="50"
            WarningThreshold="75" ErrorThreshold="90" Description="46 of 50 GB used" />

@code {
    private readonly double[] _trend = [102, 118, 134, 149];
}
```

`Delta` is shown as given, so format the number yourself. `DeltaDirection` only chooses the color (`up` is green, `down` is red, `flat` is neutral) and does not compare anything. The meter thresholds are percentages of `MeterMax`: here 46 of 50 is 92 percent, past the 90 percent error threshold, so the bar is red. The error threshold wins when both are crossed.

## Mask a secret and let the user reveal it

Goal: show an API key as `••••••••3210`, with a Reveal button and a copy button.

```razor
<KlyDisplay Kind="DisplayKind.Code" Variant="secret" Title="API key" Value="@ApiKey" VisibleChars="4" />

@code {
    [Parameter, EditorRequired] public string ApiKey { get; set; } = "";
}
```

The block shows eight bullets followed by the last `VisibleChars` characters (4 by default). A value that is not longer than `VisibleChars` is fully masked. Reveal and copy need an interactive render mode.

The mask is a display feature and does not protect the value. In Interactive Server the value stays in server memory, but in WebAssembly it is in the browser's memory, and the copy button copies the real value. Do not send a secret to a client that must not hold it.

## Highlight a code sample

Goal: show a snippet with syntax colors and a copy button.

```razor
<KlyDisplay Kind="DisplayKind.Code" Language="csharp" Value="@Snippet" />

@code {
    private const string Snippet = "public record Order(int Id) { public string Label => $\"Order {Id}\"; }";
}
```

`Language` takes a name, an alias or a file extension: `csharp`, `javascript`, `typescript`, `json`, `html`, `xml`, `css`, `scss`, `less`, `sql`, `python`, `bash` and `yaml` (`cs`, `ts`, `yml`, `sh`... also work), and `CodeHighlighter.Languages` lists the canonical names. The text of the block stays plain text: selecting it and the copy button give the code as it is. An unsupported language, `Highlight="false"`, the `secret` variant, and a code over 100,000 characters or 5,000 lines render without colors. The file preview does the same for text files, from the extension of the file name.

You can call the scanner yourself, without any UI, with `CodeHighlighter.Tokenize(code, "sql")`: it returns tokens with a `Kind` and a `Text`, and the texts concatenate to exactly the code.

## Show an agenda

Goal: list the events of a few days, with their time range, length and the current one marked.

```razor
<KlyDisplay Kind="DisplayKind.Timeline" Variant="schedule" TimelineItems="_events" Now="_now" />

@code {
    private static readonly TimeSpan Paris = TimeSpan.FromHours(1);
    private readonly DateTimeOffset _now = new(2026, 3, 12, 9, 30, 0, Paris);

    private readonly IReadOnlyList<DisplayTimelineItem> _events =
    [
        new("Standup", Date: new(2026, 3, 12, 9, 0, 0, Paris), EndDate: new(2026, 3, 12, 9, 30, 0, Paris), State: DisplayTimelineState.Current),
        new("Design review", "Bring the mockups", Date: new(2026, 3, 12, 14, 0, 0, Paris), EndDate: new(2026, 3, 12, 15, 30, 0, Paris), ActorName: "Alice"),
        new("Conference", Date: new(2026, 3, 14, 0, 0, 0, Paris), EndDate: new(2026, 3, 16, 0, 0, 0, Paris)),
        new("Book the venue"),
    ];
}
```

The events are sorted by `Date` and grouped by day; the first day reads "Today" because `Now` is that day (pass `Now` for a stable output, it defaults to the current UTC time). The standup shows `9:00 AM – 9:30 AM` and `30 min` in the format of the current locale, the review `1 h 30 min`, and the conference, which starts at midnight and ends at midnight three days later, says "All day" and "3 days": for an all-day event the end day counts, so the 14th to the 16th is three days. The last event has no date, so it goes under "Unscheduled".

`EndDate` is optional: without it only the start time shows. An end that is not after the start is ignored. An event that ends on another day stays under its start day and shows the end date in its range. Each event is shown in the offset of its own `Date`, and `EndDate` in the offset of `Date`. `Time` replaces the computed range with your own text. In this variant an event without a `State` is a normal event, and only `Done` is muted and `Current` is emphasized and gets `aria-current`. `Max` and `Step` apply to the sorted list.

## Show progress and a deadline

Goal: a row of order stages with the current one marked, and a countdown to a due date.

```razor
<KlyDisplay Kind="DisplayKind.Timeline" Variant="pipeline" TimelineItems="_steps" />
<KlyDisplay Kind="DisplayKind.Date" Variant="countdown" Title="Invoice due" DateValue="_due" Now="_now" />

@code {
    private readonly DateTimeOffset _now = new(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
    private readonly DateTimeOffset _due = new(2026, 11, 28, 0, 0, 0, TimeSpan.Zero);

    private readonly IReadOnlyList<DisplayTimelineItem> _steps =
    [
        new("Ordered", Time: "24 Nov"),
        new("Packed", Time: "25 Nov", State: DisplayTimelineState.Current),
        new("Delivered", State: DisplayTimelineState.Upcoming),
    ];
}
```

A timeline item without a `State` counts as `Done`, so set the current and upcoming ones explicitly. `Time` is displayed as given. The countdown compares calendar days between `DateValue` and `Now`, says "In 3 days" here, and shows the title "Invoice due" as a caption under it. `Now` defaults to the current UTC time, so pass it when the output must be stable, for example in a test or when a server renders a page that is cached.

## Compare two periods in a breakdown

Goal: this year's costs next to last year's, with a note on one line and a discount in green.

```razor
<KlyDisplay Kind="DisplayKind.Breakdown" Title="Costs" CompareColumns="@(("2026", "2025"))"
            BreakdownLines="_lines" BreakdownTotal="_total" />

@code {
    private readonly IReadOnlyList<DisplayBreakdownLine> _lines =
    [
        new("Licenses", "€9,600", CompareValue: "€8,400", Detail: "40 seats"),
        new("Support", "€1,200", CompareValue: "€1,200"),
        new("Loyalty discount", "-€500", Tone: KlyColor.Success, Chip: new DisplayChip("New")),
    ];

    private readonly DisplayBreakdownLine _total = new("Total", "€10,300", CompareValue: "€9,600");
}
```

`CompareColumns` names the two amount columns: `Value` goes under the first and `CompareValue` under the second, for the lines and the total. The discount has no previous amount, so its second column shows a dash. A `CompareValue` on any line also switches the block to two columns, just without the header row. `Detail` is a smaller line under the label, `Chip` a badge after it, and `Tone` colors the amount with a theme color.

## Reveal a long list a few items at a time

Goal: show three orders and let the user load three more per click.

```razor
<KlyDisplay Kind="DisplayKind.Record" Title="Recent orders" Count="@_orders.Count.ToString()"
            Records="_orders" Max="3" Step="3" />

@code {
    private readonly IReadOnlyList<DisplayRecordItem> _orders =
    [
        new() { Reference = "ORD-1042", Title = "Acme Corp", Trailing = "EUR 1,240.00", Chip = new DisplayChip("Paid", KlyColor.Success) },
        new() { Reference = "ORD-1041", Title = "Globex", Trailing = "EUR 310.50", Chip = new DisplayChip("Pending", KlyColor.Warning) },
        new() { Reference = "ORD-1040", Title = "Initech", Trailing = "EUR 87.00" },
        new() { Reference = "ORD-1039", Title = "Umbrella", Trailing = "EUR 2,450.00" },
        new() { Reference = "ORD-1038", Title = "Hooli", Trailing = "EUR 640.00" },
    ];
}
```

`Max` sets how many items show first and `Step` how many each click adds. Without `Step`, one click shows everything. `Max` applies to a flat facts list, tables, records, lists and timelines. Reveal state lives in the component, so it resets when the component is recreated.

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

## Open files from an attachment list

Goal: a list of attachments that opens in the preview when clicked.

```razor
<KlyDisplay Kind="DisplayKind.List" Variant="files" Title="Attachments" ListItems="_attachments" />

@code {
    private readonly IReadOnlyList<DisplayListItem> _attachments =
    [
        new() { Text = "Specification", Href = "/files/spec.pdf", FileName = "spec.pdf", FileSize = 1_572_864 },
        new() { Text = "Metrics", Href = "/files/metrics.csv", FileName = "metrics.csv", FileSize = 18_400, MimeType = "text/csv" },
    ];
}
```

In the `files` variant each item gets an extension badge and its size, and a click opens all the items that have an `Href` as a gallery, starting at the clicked one, when a provider is mounted and the page is interactive. Without a provider, or before the page is interactive, the link navigates to the `Href` as usual. Give every item an `Href`: items without one are left out of the gallery, which would shift the index of the ones after them.

`MimeType` helps when the name has no extension. `FileName` is the name used for the badge and for the preview, and it defaults to `Text`.

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

For components, use [bUnit](https://bunit.dev). Register `AddKlyrooDisplay()` on `Services` before rendering. A test that renders a player also needs `JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;`, because the players import that module once interactive. Select elements by their `data-slot` attributes, which the blocks and controls expose:

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
    public void Meter_turns_to_the_error_tone_past_its_threshold()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Meter)
            .Add(x => x.MeterValue, 46)
            .Add(x => x.MeterMax, 50)
            .Add(x => x.WarningThreshold, 75)
            .Add(x => x.ErrorThreshold, 90));

        Assert.Equal("error", cut.Find("[data-slot=display-meter]").GetAttribute("data-tone"));
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
