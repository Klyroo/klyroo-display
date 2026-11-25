# Klyroo.Display

*Part of [Klyroo](https://klyroo.dev): Build better. Compose freely.*

[![CI](https://github.com/klyroo/klyroo-display/actions/workflows/ci.yml/badge.svg)](https://github.com/klyroo/klyroo-display/actions/workflows/ci.yml) [![NuGet](https://img.shields.io/badge/NuGet-0.1.0--preview.1-004880.svg)](https://www.nuget.org/packages/Klyroo.Display) [![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/) [![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-10b981.svg)](https://klyroo.dev/why) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)

> **19 read-only display blocks, constrained audio/video players & modal file preview**

19 read-only presentation blocks inside <KlyDisplay> (Heading, Alert, Facts, Stat with sparkline, Breakdown, Meter with thresholds, Diff, masked Code/Secret, Timeline, Accordion…), constrained Audio/Video players with clip Start/End bounds and MaxPlays quotas, plus a modal multi-format file previewer.

[![MAUI Hybrid](https://img.shields.io/badge/.NET%20MAUI-Blazor%20Hybrid-8b5cf6.svg)](./docs/maui-and-aot.md)

---

## Architecture (Two-Layer Headless Split)

- **`Klyroo.Display.Core`** — 100% headless C#, zero UI dependencies, `<IsAotCompatible>true</IsAotCompatible>`, `<IsTrimmable>true</IsTrimmable>`. Runs in Minimal APIs, background workers, CLI apps, and native C# .NET MAUI ViewModels.
- **`Klyroo.Display`** — Multiplatform Blazor Web (Static SSR, Interactive Server, WebAssembly, Auto) & `.NET MAUI Blazor Hybrid` (`browser`, `ios`, `android`, `maccatalyst`, `windows`).

## Highlights

- **19 Read-Only Display Blocks** (`19 Blocks`) — Heading, Text, expandable Alert, Callout, copyable Facts, Stat with sparkline, Breakdown, threshold Meter, Diff before/after, Timeline, and Secret Code.
- **Constrained Audio & Video Players** (`Media Quotas`) — Supports Start/End clip slicing, MaxPlays play quotas (ideal for exams and language assessments), Seekable=false locking, and subtitle tracks.
- **Multi-Format FilePreviewService** (`File Lightbox`) — Headless MIME/extension detection in .Core, built-in RFC 4180 CSV parser, and zoomable modal lightbox for images, PDFs, audio, video, and CSV.
- **Secret Masking & One-Click Reveal** (`Security UX`) — DisplayKind.Code with Variant="secret" automatically masks API keys (••••••••3210) with Reveal/Hide toggle and clipboard copy.

## Prerequisites

- .NET 10 SDK or later.
- A Blazor Web App (Static SSR, Interactive Server, WebAssembly or Auto) or a .NET MAUI Blazor Hybrid app for the UI
  package; the `.Core` package runs anywhere .NET 10 runs, NativeAOT included.
- For .NET MAUI: the `maui` workload (`dotnet workload install maui`).

## Installation

```bash
dotnet add package Klyroo.Display.Core   # Headless C# / NativeAOT / MAUI C#
dotnet add package Klyroo.Display        # Blazor Web & .NET MAUI Blazor Hybrid
```

Register services in `Program.cs` (Blazor Web App) or `MauiProgram.cs` (.NET MAUI Blazor Hybrid):

```csharp
builder.Services.AddKlyrooDisplay();
```

## Quick Start

### Headless Core (`Klyroo.Display.Core`)

```csharp
using Klyroo.Display;

// Headless MIME/extension kind detection & modal preview trigger
filePreview.Open(
    new FilePreviewItem("spec.pdf", "/files/spec.pdf"),
    new FilePreviewItem("metrics.csv", "/files/metrics.csv"));

// Built-in RFC 4180 CSV parser for instant preview tables
var csv = FilePreviewService.ParseCsv("Region,MRR\nEurope,148900\nUS,192400");
```

### Blazor Web & .NET MAUI Hybrid (`Klyroo.Display`)

```razor
<KlyDisplay Kind="DisplayKind.Stat" Title="MRR" Value="€148,900"
           Delta="+14.2%" Trend="@([102.0, 118.0, 134.0, 149.0])" />

<KlyDisplay Kind="DisplayKind.Code" Variant="secret"
           Value="sk_live_9876543210" VisibleChars="4" Copy="true" />

<KlyAudioPlayer Src="/media/listening.mp3" Title="Listening Extract #1"
               Start="15" End="45" MaxPlays="2" Seekable="false" />
```

## Documentation

| Guide | Description |
| :--- | :--- |
| [Online documentation](https://docs.klyroo.dev/display) | Guides, live examples and API reference |
| [Getting Started](./docs/getting-started.md) | Installation, DI registration, stylesheets, and first working example |
| [Architecture](./docs/architecture.md) | Headless `.Core` vs UI split, NativeAOT guarantees, and platform matrix |
| [API Reference](./docs/api-reference.md) | Exhaustive reference of every public class, enum, property, method, and Razor parameter |
| [MAUI & NativeAOT](./docs/maui-and-aot.md) | `.NET MAUI Blazor Hybrid` (`iOS`, `Android`, `macOS`, `Windows`) and NativeAOT deployment |
| [Recipes & Testing](./docs/recipes.md) | End-to-end production patterns and `xUnit` / `bUnit` testing recipes |
| [AI Agent Skill](./skills/klyroo-display/SKILL.md) | Machine-readable guidance for coding agents |

## Key Public Types

| Type | Kind | Package | Summary |
| :--- | :--- | :--- | :--- |
| `FilePreviewKind` | `enum` | `Klyroo.Display.Core` | Built-in file preview renderer kinds supported by `KlyFilePreviewProvider` and `FilePreviewService`. |
| `FilePreviewContainer` | `enum` | `Klyroo.Display.Core` | Overlay container mode for `KlyFilePreviewProvider`. |
| `FilePreviewDetail` | `record` | `Klyroo.Display.Core` | Metadata key/value detail shown in the file preview details panel. |
| `FilePreviewAction` | `class` | `Klyroo.Display.Core` | Custom action shown in the file preview header. |
| `FilePreviewFile` | `class` | `Klyroo.Display.Core` | Describes a file to preview in `KlyFilePreviewProvider` via `FilePreviewService`. |
| `FilePreviewOptions` | `class` | `Klyroo.Display.Core` | Options when opening a file or gallery in `FilePreviewService`. |
| `ParsedCsvPreview` | `record` | `Klyroo.Display.Core` | Parsed CSV preview table returned by `FilePreviewService.ParseCsv`. |
| `FilePreviewService` | `class` | `Klyroo.Display.Core` | Scoped service managing active file preview overlays rendered by `KlyFilePreviewProvider`. Also provides deterministic file-kind detection, CSV parsing, and size/extension formatting utilities. |
| `MediaPlayerControl` | `enum` | `Klyroo.Display.Core` | Controls that can be selectively hidden on `KlyAudioPlayer` and `KlyVideoPlayer`. |
| `VideoPlayerTrack` | `record` | `Klyroo.Display.Core` | A subtitle or caption track for `KlyVideoPlayer`. |
| `MediaPlayerProgress` | `record` | `Klyroo.Display.Core` | Playback progress snapshot reported by `KlyAudioPlayer` and `KlyVideoPlayer`. Seconds elapsed relative to the clip start. Total clip duration in seconds. Raw media element time in seconds. |
| `DisplayKind` | `enum` | `Klyroo.Display` | The 19 read-only display block kinds supported by `KlyDisplay`. |
| `DisplaySurface` | `enum` | `Klyroo.Display` | Container surface treatment of a `KlyDisplay` block. |
| `DisplayChip` | `record` | `Klyroo.Display` | A chip/badge displayed inside a `KlyDisplay` block. |
| `DisplayAction` | `class` | `Klyroo.Display` | An action button or link displayed inside a `KlyDisplay` block. |
| `DisplayFact` | `class` | `Klyroo.Display` | One key/value entry in a `Facts` block. |
| `DisplayFactGroup` | `record` | `Klyroo.Display` | A named group of `DisplayFact` items in a `Facts` block. |
| `DisplayStatItem` | `record` | `Klyroo.Display` | One sub-metric in the `row` variant of a `Stat` block. |

## Razor Components

| Component | Package | Parameters | Source |
| :--- | :--- | :--- | :--- |
| `<KlyAudioPlayer>` | `Klyroo.Display` | 32 parameters | `src/Klyroo.Display/KlyAudioPlayer.razor` |
| `<KlyDisplay>` | `Klyroo.Display` | 60 parameters | `src/Klyroo.Display/KlyDisplay.razor` |
| `<KlyFilePreviewProvider>` | `Klyroo.Display` | 2 parameters | `src/Klyroo.Display/KlyFilePreviewProvider.razor` |
| `<KlyVideoPlayer>` | `Klyroo.Display` | 36 parameters | `src/Klyroo.Display/KlyVideoPlayer.razor` |

## Building & Testing

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

## Contributing

Issues and pull requests are welcome. Read [CONTRIBUTING.md](./CONTRIBUTING.md) for the workflow (Conventional
Commits, tests, documentation), follow the [Code of Conduct](./CODE_OF_CONDUCT.md), and report vulnerabilities
privately as described in [SECURITY.md](./SECURITY.md).

## Migrating from Lux

This library was previously named `lux-display`. [MIGRATION.md](./MIGRATION.md) lists every renamed package, namespace,
component, CSS token and build setting.

## License

MIT — see [LICENSE](./LICENSE). Part of the **[Klyroo](../klyroo-stack/README.md)** ecosystem.
