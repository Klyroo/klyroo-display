# Getting started

This tutorial builds a small order page: first the headless pieces in plain C#, then a Blazor page that shows the order with `KlyDisplay` and opens an invoice in the file preview overlay. It takes about ten minutes and assumes a Blazor Web App on .NET 10 with interactive rendering enabled.

## 1. Install the packages

```bash
dotnet add package Klyroo.Display --prerelease
```

`Klyroo.Display` brings `Klyroo.Display.Core` and `Klyroo.UI` with it. If you only need the headless helpers, for example in an API or a worker, install the core alone:

```bash
dotnet add package Klyroo.Display.Core --prerelease
```

## 2. Try the headless core

`FilePreviewService` has static helpers that need no setup, and an instance that holds the state of a preview. Both live in `Klyroo.Display.Core`, so this code runs in a console app as well as in Blazor.

```csharp
using Klyroo.Display;

var csv = FilePreviewService.ParseCsv("Item;Price\nDesk;299.90\nChair;149.00", "catalog.csv");
Console.WriteLine($"{csv.Rows.Count} rows, delimiter '{csv.Delimiter}', price column numeric: {csv.NumericColumns[1]}");

Console.WriteLine(FilePreviewService.DetectKind("report.pdf"));      // Pdf
Console.WriteLine(FilePreviewService.FormatFileSize(1_572_864));     // 1.5 MB
```

`ParseCsv` detected the semicolon from the first line and flagged the `Price` column as numeric, because every value in it parses as a number. `DetectKind` looks at the extension here, and also accepts a MIME type, which takes precedence.

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

The first two come from Klyroo.UI and define the theme tokens (`--klyroo-*`) and the base styles. `klyroo-display.css` holds the utility classes the blocks use, so without it they render unstyled. If your app compiles its own Tailwind, follow the Klyroo.UI documentation for importing the theme instead, see [Klyroo.UI](https://github.com/klyroo/klyroo-ui).

## 5. Show the order

Create `Components/Pages/Order.razor`, a page that renders a heading and a list of facts, and a button that opens the invoice:

```razor
@page "/orders/1042"
@inject FilePreviewService Preview

<KlyDisplay Kind="DisplayKind.Heading" Title="Order 1042" Count="3 items" Description="Placed on 24 November 2026" />

<KlyDisplay Kind="DisplayKind.Facts" Facts="_facts" />

<KlyButton Label="Preview invoice" OnClick="OpenInvoice" />

@code {
    private readonly IReadOnlyList<DisplayFact> _facts =
    [
        new DisplayFact { Label = "Customer", Value = "Acme Corp" },
        new DisplayFact { Label = "Reference", Value = "ORD-1042", Mono = true, Copy = true },
        new DisplayFact { Label = "Status", Chip = new DisplayChip("Paid", KlyColor.Success) },
        new DisplayFact { Label = "Shipping address", Value = null },
    ];

    private void OpenInvoice() =>
        Preview.Open(new FilePreviewFile { Src = "/files/invoice-1042.pdf", Name = "invoice-1042.pdf", Size = 183_204 });
}
```

A few things to notice:

- `Kind` chooses the block, and each kind reads its own parameters: `Count` and `Description` for the heading, `Facts` for the facts block.
- A fact with no value, like `Shipping address`, renders a dash instead of an empty row.
- `Copy = true` adds a copy button. It copies through the browser clipboard, so the page must render interactively (step 6 sets that up).
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

Run the app and open `/orders/1042`. The facts render, the reference has a copy button, and the button opens a drawer with the invoice in the browser's PDF viewer. The file does not need to exist for you to see the overlay and its header.

## Next steps

- [Guides](guides.md) for metrics, secrets, galleries, CSV preview, play limits and testing.
- [Concepts](concepts.md) for how the blocks, the service and the provider fit together.
- [Platforms](platforms.md) for render modes, .NET MAUI and NativeAOT.
- [API reference](api-reference.md) for every parameter and its default.
