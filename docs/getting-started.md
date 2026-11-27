# Getting started

This tutorial builds a small order page: first the headless pieces in plain C#, then a Blazor page that shows a code sample, a masked key and the attachments of an order, and opens an invoice in the file preview overlay. It takes about ten minutes.

Check the [prerequisites](../README.md#prerequisites) first: the .NET 10 SDK, and for the components a Blazor app with Klyroo.UI set up.

## 1. Install the packages

```bash
dotnet add package Klyroo.Display --prerelease
```

`Klyroo.Display` brings `Klyroo.Display.Core` and `Klyroo.UI` with it. If you only need the headless helpers, for example in an API or a worker, install the core alone:

```bash
dotnet add package Klyroo.Display.Core --prerelease
```

## 2. Try the headless core

`FilePreviewService` has static helpers that need no setup, and an instance that holds the state of a preview. `CodeHighlighter` colors code without any UI. All of them live in `Klyroo.Display.Core`, so this code runs in a console app as well as in Blazor.

```csharp
using Klyroo.Display;

var csv = FilePreviewService.ParseCsv("Item;Price\nDesk;299.90\nChair;149.00", "catalog.csv");
Console.WriteLine($"{csv.Rows.Count} rows, delimiter '{csv.Delimiter}', price column numeric: {csv.NumericColumns[1]}");

Console.WriteLine(FilePreviewService.DetectKind("report.pdf"));      // Pdf
Console.WriteLine(FilePreviewService.FormatFileSize(1_572_864));     // 1.5 MB
```

`ParseCsv` detected the semicolon from the first line and flagged the `Price` column as numeric, because every value in it parses as a number. `DetectKind` looks at the extension here, and also accepts a MIME type, which takes precedence.

The highlighter takes a `CodeLanguage`. A name, an alias or a file extension that comes from outside, such as a Markdown fence or a file name, resolves to one:

```csharp
using Klyroo.Display;

var language = CodeHighlighter.ResolveFileName("Program.cs");       // CodeLanguage.CSharp
if (language is { } known)
{
    Console.WriteLine(CodeHighlighter.Highlight("var n = 42;", known));
    // <span data-token="keyword">var</span> n <span data-token="punctuation">=</span> <span data-token="number">42</span><span data-token="punctuation">;</span>
}
```

`Highlight` returns escaped markup whose spans carry a `data-token` attribute and no style, so you color them with your own CSS. `CodeHighlighter.Tokenize` returns the same slices as typed `CodeToken` values.

The instance side is a small state machine. Opening a gallery, moving through it and closing it each raise `Changed`, which is how the Blazor provider knows to re-render:

```csharp
using Klyroo.Display;

var preview = new FilePreviewService();
preview.Changed += () =>
    Console.WriteLine($"{preview.CurrentFile?.Name} ({preview.CurrentIndex + 1} of {preview.Files.Count})");

preview.OpenGallery(
    [
        new FilePreviewFile { Src = "/files/spec.pdf", Name = "spec.pdf" },
        new FilePreviewFile { Src = "/files/metrics.csv", Name = "metrics.csv" },
    ],
    new FilePreviewOptions { Loop = true });

preview.Next();
preview.Next();   // wraps around to the first file
preview.Close();
```

## 3. Register the services

`AddKlyrooDisplay` registers `FilePreviewService` as a scoped service and calls `AddKlyrooUi()` for you.

```csharp
using Klyroo.Display;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKlyrooDisplay();
```

In an Interactive WebAssembly or Auto app, make the same call in the client project as well, because components can render on either side and each side has its own container.

## 4. Add imports and stylesheets

In `_Imports.razor`:

```razor
@using Klyroo.UI
@using Klyroo.Display
```

In `App.razor`, inside `<head>`:

```razor
<link rel="stylesheet" href="@Assets["_content/Klyroo.UI/klyroo.preflight.css"]" />
<link rel="stylesheet" href="@Assets["_content/Klyroo.UI/klyroo.css"]" />
<link rel="stylesheet" href="@Assets["_content/Klyroo.Display/klyroo-display.css"]" />
```

The first two come from Klyroo.UI and define the theme tokens (`--klyroo-*`) and the base styles. `klyroo-display.css` holds the utility classes the components use, so without it they render unstyled. If your app compiles its own Tailwind, follow the Klyroo.UI documentation for importing the theme instead, see [Klyroo.UI](https://github.com/klyroo/klyroo-ui).

## 5. Show the order

Create `Components/Pages/Order.razor`, a page that shows a highlighted query with two emphasized lines, a masked API key, the attachments of the order and a button that opens the invoice:

```razor
@page "/orders/1042"
@inject FilePreviewService Preview

<KlyCodeBlock Code="@Query" Language="CodeLanguage.Sql" Label="Query behind this order" LineNumbers
              HighlightLines="@([2, new CodeLineRange(4, 5)])" />

<KlySecretBlock Label="Webhook key" Value="whsec_9f2c41d7a0b8" />

<KlyFileList Title="Attachments" Files="_attachments" />

<KlyButton Label="Preview invoice" OnClick="OpenInvoice" />

@code {
    private const string Query =
        "SELECT id, total\nFROM orders\nWHERE id = 1042\n  AND status = 'paid'\n  AND total > 100;\n";

    private readonly IReadOnlyList<FilePreviewFile> _attachments =
    [
        new() { Src = "/files/invoice-1042.pdf", Size = 183_204 },
        new() { Src = "/files/packing-list.csv", Size = 2_048 },
    ];

    private void OpenInvoice() =>
        Preview.Open(new FilePreviewFile { Src = "/files/invoice-1042.pdf", Name = "invoice-1042.pdf", Size = 183_204 });
}
```

A few things to notice:

- `Language` is a `CodeLanguage`, so a misspelled language does not compile. Without it the block shows plain text. The label defaults to the language name.
- `HighlightLines` takes line numbers counted from 1: a single line converts to a `CodeLineRange` by itself, and `new CodeLineRange(4, 5)` is a run. The emphasized lines get a neutral fill and an accent marker in the gutter, and the text you select and copy is still the code as given.
- `KlySecretBlock` writes only the bullets and the last four characters to the page while the value is masked. Reveal and the copy buttons need an interactive page (step 6 sets that up).
- Each file in the list shows an extension badge and its size. A click opens the whole list as a gallery at that file, once a `KlyFilePreviewProvider` is mounted. Until then the name is a plain link to the file.
- `Preview.Open` only changes state. Nothing appears until a `KlyFilePreviewProvider` is mounted, which is the next step.

## 6. Mount the file preview provider

Make the app interactive in `App.razor`:

```razor
<Routes @rendermode="InteractiveServer" />
```

Then add the provider once, in `Components/Layout/MainLayout.razor`:

```razor
@inherits LayoutComponentBase

<main>
    @Body
</main>

<KlyFilePreviewProvider />
```

The provider and the page must share the same `FilePreviewService` instance, and they only do when both render in the same interactive scope. With interactivity set globally on `<Routes />`, the layout is interactive and this works as written. If you set a render mode per page instead, place `<KlyFilePreviewProvider />` inside that page.

Run the app and open `/orders/1042`. The query is colored with two lines emphasized, the key is masked until you click Reveal, a click on an attachment opens the gallery, and the button opens a drawer with the invoice in the browser's PDF viewer. The files do not need to exist for you to see the overlay and its header.

## Next steps

- [Guides](guides.md) for code samples, secrets, file lists, galleries, CSV preview, play limits and testing.
- [Concepts](concepts.md) for how the components, the highlighter, the service and the provider fit together.
- [Platforms](platforms.md) for render modes, .NET MAUI and NativeAOT.
- [API reference](api-reference.md) for every parameter and its default.
