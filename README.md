# Lux.Display (LuxStack)

> **19 read-only display blocks, constrained audio/video players & modal file preview**

19 read-only presentation blocks inside <BxDisplay> (Heading, Alert, Facts, Stat with sparkline, Breakdown, Meter with thresholds, Diff, masked Code/Secret, Timeline, Accordion…), constrained Audio/Video players with clip Start/End bounds and MaxPlays quotas, plus a modal multi-format file previewer.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![NativeAOT](https://img.shields.io/badge/Core-NativeAOT%20Ready-10b981.svg)](./docs/maui-and-aot.md)
[![MAUI Hybrid](https://img.shields.io/badge/.NET%20MAUI-Blazor%20Hybrid-8b5cf6.svg)](./docs/maui-and-aot.md)

---

## Architecture (TanStack Two-Layer Split)

- **`Lux.Display.Core`** — 100% headless C#, zero UI dependencies, `<IsAotCompatible>true</IsAotCompatible>`, `<IsTrimmable>true</IsTrimmable>`. Runs in Minimal APIs, background workers, CLI apps, and native C# .NET MAUI ViewModels.
- **`Lux.Display`** — Multiplatform Blazor Web (Static SSR, Interactive Server, WebAssembly, Auto) & `.NET MAUI Blazor Hybrid` (`browser`, `ios`, `android`, `maccatalyst`, `windows`).

## Highlights

- **19 Read-Only Display Blocks** (`19 Blocks`) — Heading, Text, expandable Alert, Callout, copyable Facts, Stat with sparkline, Breakdown, threshold Meter, Diff before/after, Timeline, and Secret Code.
- **Constrained Audio & Video Players** (`Media Quotas`) — Supports Start/End clip slicing, MaxPlays play quotas (ideal for exams and language assessments), Seekable=false locking, and subtitle tracks.
- **Multi-Format FilePreviewService** (`File Lightbox`) — Headless MIME/extension detection in .Core, built-in RFC 4180 CSV parser, and zoomable modal lightbox for images, PDFs, audio, video, and CSV.
- **Secret Masking & One-Click Reveal** (`Security UX`) — DisplayKind.Code with Variant="secret" automatically masks API keys (••••••••3210) with Reveal/Hide toggle and clipboard copy.

## Installation

```bash
dotnet add package Lux.Display.Core   # Headless C# / NativeAOT / MAUI C#
dotnet add package Lux.Display        # Blazor Web & .NET MAUI Blazor Hybrid
```

Register services in `Program.cs` (Blazor Web App) or `MauiProgram.cs` (.NET MAUI Blazor Hybrid):

```csharp
builder.Services.AddLuxDisplay();
```

## Quick Start

### Headless Core (`Lux.Display.Core`)

```csharp
using Blux.UI.Tools;

// Headless MIME/extension kind detection & modal preview trigger
filePreview.Open(
    new FilePreviewItem("spec.pdf", "/files/spec.pdf"),
    new FilePreviewItem("metrics.csv", "/files/metrics.csv"));

// Built-in RFC 4180 CSV parser for instant preview tables
var csv = FilePreviewService.ParseCsv("Region,MRR\nEurope,148900\nUS,192400");
```

### Blazor Web & .NET MAUI Hybrid (`Lux.Display`)

```razor
<BxDisplay Kind="DisplayKind.Stat" Title="MRR" Value="€148,900"
           Delta="+14.2%" Trend="@([102.0, 118.0, 134.0, 149.0])" />

<BxDisplay Kind="DisplayKind.Code" Variant="secret"
           Value="sk_live_9876543210" VisibleChars="4" Copy="true" />

<BxAudioPlayer Src="/media/listening.mp3" Title="Listening Extract #1"
               Start="15" End="45" MaxPlays="2" Seekable="false" />
```

## Documentation

| Guide | Description |
| :--- | :--- |
| [Getting Started](./docs/getting-started.md) | Installation, DI registration, stylesheets, and first working example |
| [Architecture](./docs/architecture.md) | Headless `.Core` vs UI split, NativeAOT guarantees, and platform matrix |
| [API Reference](./docs/api-reference.md) | Exhaustive reference of every public class, enum, property, method, and Razor parameter |
| [MAUI & NativeAOT](./docs/maui-and-aot.md) | `.NET MAUI Blazor Hybrid` (`iOS`, `Android`, `macOS`, `Windows`) and NativeAOT deployment |
| [Recipes & Testing](./docs/recipes.md) | End-to-end production patterns and `xUnit` / `bUnit` testing recipes |
| [Product Landing Page](./site/index.html) | Standalone TanStack-style interactive landing page |
| [AI Agent Skill](./skills/lux-display/SKILL.md) | Machine-readable guidance for coding agents |

## Key Public Types

| Type | Kind | Package | Summary |
| :--- | :--- | :--- | :--- |
| `FilePreviewKind` | `enum` | `Lux.Display.Core` | Built-in file preview renderer kinds supported by `BxFilePreviewProvider` and `FilePreviewService`. |
| `FilePreviewContainer` | `enum` | `Lux.Display.Core` | Overlay container mode for `BxFilePreviewProvider`. |
| `FilePreviewDetail` | `record` | `Lux.Display.Core` | Metadata key/value detail shown in the file preview details panel. |
| `FilePreviewAction` | `class` | `Lux.Display.Core` | Custom action shown in the file preview header. |
| `FilePreviewFile` | `class` | `Lux.Display.Core` | Describes a file to preview in `BxFilePreviewProvider` via `FilePreviewService`. |
| `FilePreviewOptions` | `class` | `Lux.Display.Core` | Options when opening a file or gallery in `FilePreviewService`. |
| `ParsedCsvPreview` | `record` | `Lux.Display.Core` | Parsed CSV preview table returned by `FilePreviewService.ParseCsv`. |
| `FilePreviewService` | `class` | `Lux.Display.Core` | Scoped service managing active file preview overlays rendered by `BxFilePreviewProvider`. Also provides deterministic file-kind detection, CSV parsing, and size/extension formatting utilities. |
| `MediaPlayerControl` | `enum` | `Lux.Display.Core` | Controls that can be selectively hidden on `BxAudioPlayer` and `BxVideoPlayer`. |
| `VideoPlayerTrack` | `record` | `Lux.Display.Core` | A subtitle or caption track for `BxVideoPlayer`. |
| `MediaPlayerProgress` | `record` | `Lux.Display.Core` | Playback progress snapshot reported by `BxAudioPlayer` and `BxVideoPlayer`. Seconds elapsed relative to the clip start. Total clip duration in seconds. Raw media element time in seconds. |
| `DisplayKind` | `enum` | `Lux.Display` | The 19 read-only display block kinds supported by `BxDisplay`. |
| `DisplaySurface` | `enum` | `Lux.Display` | Container surface treatment of a `BxDisplay` block. |
| `DisplayChip` | `record` | `Lux.Display` | A chip/badge displayed inside a `BxDisplay` block. |
| `DisplayAction` | `class` | `Lux.Display` | An action button or link displayed inside a `BxDisplay` block. |
| `DisplayFact` | `class` | `Lux.Display` | One key/value entry in a `Facts` block. |
| `DisplayFactGroup` | `record` | `Lux.Display` | A named group of `DisplayFact` items in a `Facts` block. |
| `DisplayStatItem` | `record` | `Lux.Display` | One sub-metric in the `row` variant of a `Stat` block. |

## Razor Components

| Component | Package | Parameters | Source |
| :--- | :--- | :--- | :--- |
| `<BxAudioPlayer>` | `Lux.Display` | 32 parameters | `src/Lux.Display/BxAudioPlayer.razor` |
| `<BxDisplay>` | `Lux.Display` | 60 parameters | `src/Lux.Display/BxDisplay.razor` |
| `<BxFilePreviewProvider>` | `Lux.Display` | 2 parameters | `src/Lux.Display/BxFilePreviewProvider.razor` |
| `<BxVideoPlayer>` | `Lux.Display` | 36 parameters | `src/Lux.Display/BxVideoPlayer.razor` |

## Building & Testing

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

## License

MIT — see [LICENSE](./LICENSE). Part of the **[LuxStack](../lux.stack/README.md)** ecosystem.
