# Getting Started with Lux.Display

19 read-only presentation blocks inside <BxDisplay> (Heading, Alert, Facts, Stat with sparkline, Breakdown, Meter with thresholds, Diff, masked Code/Secret, Timeline, Accordion…), constrained Audio/Video players with clip Start/End bounds and MaxPlays quotas, plus a modal multi-format file previewer.

## 1. Choose Your Package

| Use Case | Package to Install |
| :--- | :--- |
| Headless C#, Minimal APIs, Workers, CLI, or native C# .NET MAUI ViewModels | `Lux.Display.Core` |
| Blazor Web App (Server, WebAssembly, Auto, Static SSR) or `.NET MAUI Blazor Hybrid` | `Lux.Display` |

```bash
dotnet add package Lux.Display.Core
dotnet add package Lux.Display
```

## 2. Register Services

Call `builder.Services.AddLuxDisplay();` in `Program.cs` (both Server and Client projects when using Blazor Interactive Auto) or `MauiProgram.cs` (after `AddMauiBlazorWebView()`):

```csharp
builder.Services.AddLuxDisplay();
```

## 3. Add Namespace Imports

In `_Imports.razor`:

```razor
@using Lux.UI
@using Lux.Display

```

## 4. First Example

### Headless C# (`Lux.Display.Core`)

```csharp
using Lux.Display;

// Headless MIME/extension kind detection & modal preview trigger
filePreview.Open(
    new FilePreviewItem("spec.pdf", "/files/spec.pdf"),
    new FilePreviewItem("metrics.csv", "/files/metrics.csv"));

// Built-in RFC 4180 CSV parser for instant preview tables
var csv = FilePreviewService.ParseCsv("Region,MRR\nEurope,148900\nUS,192400");
```

### Blazor / MAUI Hybrid (`Lux.Display`)

```razor
<BxDisplay Kind="DisplayKind.Stat" Title="MRR" Value="€148,900"
           Delta="+14.2%" Trend="@([102.0, 118.0, 134.0, 149.0])" />

<BxDisplay Kind="DisplayKind.Code" Variant="secret"
           Value="sk_live_9876543210" VisibleChars="4" Copy="true" />

<BxAudioPlayer Src="/media/listening.mp3" Title="Listening Extract #1"
               Start="15" End="45" MaxPlays="2" Seekable="false" />
```

## Next Steps

- [Architecture & Two-Layer Design](./architecture.md)
- [Complete API Reference](./api-reference.md)
- [.NET MAUI & NativeAOT Guide](./maui-and-aot.md)
- [Recipes & Testing](./recipes.md)
