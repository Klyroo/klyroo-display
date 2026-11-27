# Klyroo.Display

Rich content for Blazor: a syntax-highlighted code block, a masked secret block, a file list with a file preview overlay, and audio and video players, with a UI-free core for code highlighting, file detection and CSV parsing.

[![CI](https://github.com/klyroo/klyroo-display/actions/workflows/ci.yml/badge.svg)](https://github.com/klyroo/klyroo-display/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/Klyroo.Display.svg)](https://www.nuget.org/packages/Klyroo.Display)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/klyroo/klyroo-display/blob/main/LICENSE)

Some content needs more than markup: code that has to be colored, copied and annotated, a value that must stay masked until the user asks, a list of attachments that opens in a viewer, a clip with a play limit. Klyroo.Display ships those as distinct typed components, each with only the parameters it reads, and keeps the logic behind them in a core package with no UI dependency, so the highlighter and the file helpers also run in an API or a worker. File preview works through a service: you inject it, call `Open`, and a provider renders the overlay. The Blazor package targets Blazor Web Apps and declares support for .NET MAUI Blazor Hybrid platforms. The plain read-only blocks (alerts, statistics, tables, timelines and the like) are components of [Klyroo.UI](https://github.com/klyroo/klyroo-ui).

## Features

- **Syntax highlighting**: `KlyCodeBlock` colors C#, JavaScript, TypeScript, JSON, HTML, XML, CSS, SCSS, Less, SQL, Python, Bash and YAML from a typed `CodeLanguage`, with a label, a copy button, optional line numbers and emphasized lines (`HighlightLines`), and the file preview does the same from the file extension. The scanner is `CodeHighlighter` in `Klyroo.Display.Core`: dependency-free, trim and AOT safe, it never throws, and `CodeHighlighter.Highlight(code, language)` returns escaped markup for pages that Blazor does not render.
- **Secret values**: `KlySecretBlock` masks all but the last characters of a value, with a Reveal toggle and a copy button, and never highlights it.
- **File list and preview overlay**: `KlyFileList` lists files with an extension badge, a size and a date, and a click opens them as a gallery in the overlay. `KlyFilePreviewProvider` shows PDF, images, video, audio, CSV, Markdown and text in a modal, drawer or fullscreen panel, with keyboard navigation, image zoom and rotation, a details panel and downloads.
- **Headless file helpers**: `Klyroo.Display.Core` detects a file kind from its MIME type or extension, parses CSV and TSV (RFC 4180 quoting, delimiter detection) and formats sizes, and it is NativeAOT compatible.
- **Audio and video players with clip and play-limit rules**: `KlyAudioPlayer` and `KlyVideoPlayer` play a clip window of a file, enforce a play quota and a non-seekable timeline, and report progress and the end of each play through callbacks. Their control bar has volume, speed, captions, picture-in-picture and fullscreen controls, and a compact size.
- **Themeable like Klyroo.UI**: each block has slots with default classes (`KlyrooDisplayOptions.CodeBlock`, `SecretBlock` and `FileList`) that you extend in `AddKlyrooDisplay`, and a typed `Ui` parameter (`CodeBlockUi`, `SecretBlockUi`, `FileListUi`) for one instance.
- **Translated texts**: the built-in labels follow the Klyroo.UI locale, with a `DisplayTexts` set for each of its twelve languages (English and French reviewed), and `AddKlyrooDisplay(d => d.Texts("fr", t => t with { ... }))` changes any of them.

## Packages

| Package | Description |
| :--- | :--- |
| [`Klyroo.Display.Core`](https://www.nuget.org/packages/Klyroo.Display.Core) | `CodeHighlighter` and `CodeLanguage`, `FilePreviewService`, file kind detection, CSV parser and the file and media models. No UI dependency. |
| [`Klyroo.Display`](https://www.nuget.org/packages/Klyroo.Display) | `KlyCodeBlock`, `KlySecretBlock`, `KlyFileList`, `KlyAudioPlayer`, `KlyVideoPlayer` and `KlyFilePreviewProvider` for Blazor. Builds on [Klyroo.UI](https://github.com/klyroo/klyroo-ui) and includes the Core package. |

## Prerequisites

To use the library in an app:

- **The .NET 10 SDK**, and an app that targets `net10.0`. The packages are prereleases, so every `dotnet add package` command needs `--prerelease`.
- **`Klyroo.Display.Core`** alone (code highlighting, file kinds, CSV parsing, sizes) runs in any .NET 10 app.
- **A Blazor app** for `Klyroo.Display`: a Blazor Web App or a .NET MAUI Blazor Hybrid app. The code, secret and file list blocks render in static server rendering; the media players, copy buttons, the Reveal toggle, Show more and the file preview need an interactive render mode. In a WebAssembly or Auto app, register the services in the server project and in the `.Client` project.
- **Klyroo.UI set up in the app.** `Klyroo.Display` brings the Klyroo.UI package, and `AddKlyrooDisplay()` registers its services, but the layout still needs the Klyroo.UI stylesheets in `<head>` and `<KlyApp>` around the body, as described in the [Klyroo.UI installation](https://github.com/klyroo/klyroo-ui#installation).
- **`<KlyFilePreviewProvider>` in the layout** to open file previews, and a secure context (HTTPS or `localhost`) for the copy buttons, which use the clipboard API.
- **A browser with the native popover API**, which Klyroo.UI uses: Chrome and Edge 114, Safari 17 or Firefox 125, or later. In a .NET MAUI app, that means iOS or Mac Catalyst 17, or an Android System WebView 114 or later.
- **Network access on the first build** only if your app self-hosts web fonts (`KlyrooFont`), uses icons outside the built-in set, or compiles Tailwind (`KlyrooTailwind`): the build downloads them once and caches them under `obj/klyroo`. Node.js is not needed.

To build this repository you need the .NET 10 SDK (a newer SDK works too, see `global.json`) and a clone of `klyroo-ui` in the same parent folder: some projects reference it by relative path (`../klyroo-ui/...`). See [Contributing](https://github.com/klyroo/klyroo-display/blob/main/CONTRIBUTING.md).

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

Load the stylesheets in `App.razor`, inside `<head>`. `klyroo-display.css` is the compiled Tailwind classes the components use:

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

var language = CodeHighlighter.ResolveFileName("Program.cs");        // CodeLanguage.CSharp
Console.WriteLine(CodeHighlighter.Highlight("var n = 42;", language!.Value));
// <span data-token="keyword">var</span> n <span data-token="punctuation">=</span> <span data-token="number">42</span><span data-token="punctuation">;</span>
```

In a Blazor page:

```razor
<KlyCodeBlock Code="@Snippet" Language="CodeLanguage.Sql" Label="Query" LineNumbers
              HighlightLines="@([2, new CodeLineRange(4, 5)])" />

<KlySecretBlock Label="API key" Value="sk_test_9876543210" />

<KlyFileList Title="Attachments" Files="_files" />

@code {
    private const string Snippet = "SELECT id, total\nFROM orders\nWHERE status = 'paid'\n  AND total > 100\nORDER BY total DESC;\n";

    private readonly IReadOnlyList<FilePreviewFile> _files =
    [
        new() { Src = "/files/spec.pdf", Size = 1_572_864 },
        new() { Src = "/files/metrics.csv", Size = 18_400 },
    ];
}
```

To preview files, add `<KlyFilePreviewProvider />` once to an interactive layout, inject `FilePreviewService` where you need it and call `Open`. [Getting started](https://github.com/klyroo/klyroo-display/blob/main/docs/getting-started.md) builds that end to end.

## Documentation

| | |
| :--- | :--- |
| [Getting started](https://github.com/klyroo/klyroo-display/blob/main/docs/getting-started.md) | Step-by-step first integration |
| [Guides](https://github.com/klyroo/klyroo-display/blob/main/docs/guides.md) | Task-oriented recipes |
| [Concepts](https://github.com/klyroo/klyroo-display/blob/main/docs/concepts.md) | How the library works |
| [Platforms](https://github.com/klyroo/klyroo-display/blob/main/docs/platforms.md) | Render modes, .NET MAUI, NativeAOT |
| [API reference](https://github.com/klyroo/klyroo-display/blob/main/docs/api-reference.md) | Every public type and member |
| [FAQ](https://github.com/klyroo/klyroo-display/blob/main/docs/faq.md) | Troubleshooting and limitations |

## Compatibility

- .NET 10 SDK.
- `Klyroo.Display.Core` runs anywhere .NET 10 runs and is marked `IsAotCompatible` and `IsTrimmable`.
- `Klyroo.Display` renders in Interactive Server, WebAssembly, Auto and static server rendering. Copy buttons, the Reveal toggle, Show more, the player controls and the file preview overlay need an interactive render mode.
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
| **[Klyroo.Display](https://github.com/klyroo/klyroo-display)** | Code highlighting, file preview and audio and video players |
| [Klyroo.Spreadsheet](https://github.com/klyroo/klyroo-spreadsheet) | Import wizard for CSV, TSV and pasted data, with column and value mapping |
| [Klyroo.Agent](https://github.com/klyroo/klyroo-agent) | Bridge that lets AI agents read and act on Klyroo forms, dashboards and imports |
| [Klyroo.Stack](https://github.com/klyroo/klyroo-stack) | Meta-package, MCP documentation server and `dotnet new` templates |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](https://github.com/klyroo/klyroo-display/blob/main/CONTRIBUTING.md) before opening a pull request, and follow the
[code of conduct](https://github.com/klyroo/klyroo-display/blob/main/CODE_OF_CONDUCT.md). Report security issues privately as described in [SECURITY.md](https://github.com/klyroo/klyroo-display/blob/main/SECURITY.md).

## License

[MIT](https://github.com/klyroo/klyroo-display/blob/main/LICENSE)
