# Klyroo.Display

Read-only display blocks, audio and video control bars, and a file preview overlay for Blazor, with a UI-free core for file detection and CSV parsing.

[![CI](https://github.com/klyroo/klyroo-display/actions/workflows/ci.yml/badge.svg)](https://github.com/klyroo/klyroo-display/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/Klyroo.Display.svg)](https://www.nuget.org/packages/Klyroo.Display)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Most screens show information more often than they edit it: an order summary, a usage meter, an audit trail, an attachment list. `KlyDisplay` renders those blocks from typed models through one component, so you pick a `DisplayKind` and fill the parameters it reads instead of hand-writing markup for each one. File preview works the same way: you inject a service, call `Open`, and a provider renders the overlay. The Blazor package targets Blazor Web Apps and declares support for .NET MAUI Blazor Hybrid platforms, and the file logic lives in a separate package with no UI dependency.

## Features

- **One component, 19 block kinds**: `KlyDisplay` renders headings, text, alerts, facts, statistics, breakdowns, meters, charts, tables, diffs, records, people, lists, timelines, dates, images, code values, empty states and accordions.
- **Variants and surfaces**: a `Variant` string selects a layout within a kind (for example `row` for a statistic or `pipeline` for a timeline), and `DisplaySurface` sets the border, background and shadow.
- **Secret values**: the `secret` variant of the code block masks all but the last characters, with a Reveal toggle and a copy button.
- **Progressive reveal**: `Max` and `Step` show long facts, tables, records, lists and timelines a few items at a time behind a Show more button.
- **File preview overlay**: `KlyFilePreviewProvider` shows PDF, images, video, audio, CSV, Markdown and text in a modal, drawer or fullscreen panel, with galleries, keyboard navigation, image zoom and rotation, a details panel and downloads.
- **Headless file helpers**: `Klyroo.Display.Core` detects a file kind from its MIME type or extension, parses CSV and TSV (RFC 4180 quoting, delimiter detection) and formats sizes, and it is NativeAOT compatible.
- **Audio and video players with clip and play-limit rules**: `KlyAudioPlayer` and `KlyVideoPlayer` play a clip window of a file, enforce a play quota and a non-seekable timeline, and report progress and the end of each play through callbacks.

## Packages

| Package | Description |
| :--- | :--- |
| [`Klyroo.Display.Core`](https://www.nuget.org/packages/Klyroo.Display.Core) | `FilePreviewService`, file kind detection, CSV parser and the file and media models. No UI dependency. |
| [`Klyroo.Display`](https://www.nuget.org/packages/Klyroo.Display) | `KlyDisplay`, `KlyAudioPlayer`, `KlyVideoPlayer` and `KlyFilePreviewProvider` for Blazor. Builds on [Klyroo.UI](https://github.com/klyroo/klyroo-ui) and includes the Core package. |

## Installation

```bash
dotnet add package Klyroo.Display --prerelease
```

For code that has no UI, such as an API or a worker, install only the core:

```bash
dotnet add package Klyroo.Display.Core --prerelease
```

Register the services in `Program.cs`, and in the client project too when you use WebAssembly or Auto:

```csharp
using Klyroo.Display;

builder.Services.AddKlyrooDisplay();
```

Import the namespaces in `_Imports.razor`:

```razor
@using Klyroo.UI
@using Klyroo.Display
```

Load the stylesheets in `App.razor`, inside `<head>`. `klyroo-display.css` is the compiled Tailwind classes the blocks use:

```razor
<link rel="stylesheet" href="@Assets["_content/Klyroo.UI/klyroo.preflight.css"]" />
<link rel="stylesheet" href="@Assets["_content/Klyroo.UI/klyroo.css"]" />
<link rel="stylesheet" href="@Assets["_content/Klyroo.Display/klyroo-display.css"]" />
```

## Quick start

Headless, with `Klyroo.Display.Core`:

```csharp
using Klyroo.Display;

var csv = FilePreviewService.ParseCsv("Item;Price\nDesk;299.90\nChair;149.00", "catalog.csv");
Console.WriteLine($"{csv.Rows.Count} rows, delimiter '{csv.Delimiter}', price column numeric: {csv.NumericColumns[1]}");

Console.WriteLine(FilePreviewService.DetectKind("report.pdf"));      // Pdf
Console.WriteLine(FilePreviewService.FormatFileSize(1_572_864));     // 1.5 MB
```

In a Blazor page:

```razor
<KlyDisplay Kind="DisplayKind.Stat" Title="Monthly revenue" Value="148,900" Unit="EUR"
            Delta="+14.2%" Trend="_trend" />

<KlyDisplay Kind="DisplayKind.Code" Variant="secret" Title="API key" Value="sk_test_9876543210" />

@code {
    private readonly double[] _trend = [102, 118, 134, 149];
}
```

To preview files, add `<KlyFilePreviewProvider />` once to an interactive layout, inject `FilePreviewService` where you need it and call `Open`. [Getting started](docs/getting-started.md) builds that end to end.

## Documentation

| | |
| :--- | :--- |
| [Getting started](docs/getting-started.md) | Step-by-step first integration |
| [Guides](docs/guides.md) | Task-oriented recipes |
| [Concepts](docs/concepts.md) | How the library works |
| [Platforms](docs/platforms.md) | Render modes, .NET MAUI, NativeAOT |
| [API reference](docs/api-reference.md) | Every public type and member |
| [FAQ](docs/faq.md) | Troubleshooting and limitations |

## Requirements

- .NET 10 SDK.
- `Klyroo.Display.Core` runs anywhere .NET 10 runs and is marked `IsAotCompatible` and `IsTrimmable`.
- `Klyroo.Display` renders in Interactive Server, WebAssembly, Auto and static server rendering. Copy buttons, toggles, action callbacks, the player controls and the file preview overlay need an interactive render mode.
- `Klyroo.Display` declares `browser`, `ios`, `android`, `maccatalyst` and `windows` as supported platforms for .NET MAUI Blazor Hybrid. It is marked `IsTrimmable` but not `IsAotCompatible`.
- `Klyroo.Display` depends on `Klyroo.UI` for theming and the base components.

## Klyroo ecosystem

Klyroo is a family of independent .NET libraries that compose. Each one has its own repository, versions and releases.

| Library | Description |
| :--- | :--- |
| [Klyroo.UI](https://github.com/klyroo/klyroo-ui) | Accessible Blazor components for the web and .NET MAUI Blazor Hybrid, styled with Tailwind CSS v4 |
| [Klyroo.Query](https://github.com/klyroo/klyroo-query) | Typed query model, URL state sync, and IQueryable / EF Core execution |
| [Klyroo.Table](https://github.com/klyroo/klyroo-table) | Data lists with table and card-grid layouts, filters, selection and actions |
| [Klyroo.Form](https://github.com/klyroo/klyroo-form) | Schema-driven forms, validation and wizards |
| [Klyroo.Dashboard](https://github.com/klyroo/klyroo-dashboard) | Dashboard controller, filters and Blazor blocks: stats, gauges, charts, tables and feeds |
| **[Klyroo.Display](https://github.com/klyroo/klyroo-display)** | Read-only display blocks, media player controls and file preview |
| [Klyroo.Spreadsheet](https://github.com/klyroo/klyroo-spreadsheet) | Import wizard for CSV, TSV and pasted data, with column and value mapping |
| [Klyroo.Agent](https://github.com/klyroo/klyroo-agent) | Bridge that lets AI agents read and act on Klyroo forms, dashboards and imports |
| [Klyroo.Stack](https://github.com/klyroo/klyroo-stack) | Meta-package, MCP documentation server and `dotnet new` templates |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request, and follow the
[code of conduct](CODE_OF_CONDUCT.md). Report security issues privately as described in [SECURITY.md](SECURITY.md).

## License

[MIT](LICENSE)
