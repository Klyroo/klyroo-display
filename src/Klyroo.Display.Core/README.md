# Klyroo.Display.Core

The UI-free half of [Klyroo.Display](https://github.com/klyroo/klyroo-display): file kind detection, a CSV and TSV parser, file size formatting, and the state and models behind the file preview. It has no dependencies, targets .NET 10, and is marked `IsAotCompatible` and `IsTrimmable`, so it runs in APIs, workers, console tools and NativeAOT apps. For the Blazor components, install `Klyroo.Display`, which includes this package.

## Install

```bash
dotnet add package Klyroo.Display.Core --prerelease
```

## Use

```csharp
using Klyroo.Display;

var csv = FilePreviewService.ParseCsv("Item;Price\nDesk;299.90\nChair;149.00", "catalog.csv");
Console.WriteLine($"{csv.Rows.Count} rows, delimiter '{csv.Delimiter}', price column numeric: {csv.NumericColumns[1]}");

Console.WriteLine(FilePreviewService.DetectKind("report.pdf"));      // Pdf
Console.WriteLine(FilePreviewService.FormatFileSize(1_572_864));     // 1.5 MB
```

## Documentation

See the [repository README](https://github.com/klyroo/klyroo-display#readme) and its [guides](https://github.com/klyroo/klyroo-display/blob/main/docs/guides.md).
