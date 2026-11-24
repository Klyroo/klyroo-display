# API Reference — Lux.Display

Complete public API reference extracted directly from the source code of `Lux.Display.Core` and `Lux.Display`.

## Razor Components

### `<LxAudioPlayer>`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/LxAudioPlayer.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Src` | `string` | — |
| `Title` | `string?` | — |
| `Preload` | `string` | — |
| `Autoplay` | `bool` | — |
| `Loop` | `bool` | — |
| `Disabled` | `bool` | — |
| `Compact` | `bool` | — |
| `Keyboard` | `bool` | — |
| `Start` | `double` | Clip start offset in seconds. |
| `End` | `double?` | Optional clip end offset in seconds. |
| `Duration` | `double` | Total duration in seconds (defaults to 60s in headless/SSR preview mode until metadata loads). |
| `Seekable` | `bool` | Whether the user can scrub the timeline. |
| `MaxPlays` | `int` | Maximum number of allowed plays (`0` = unlimited). |
| `Plays` | `int` | Number of plays consumed so far. |
| `PlaysChanged` | `EventCallback<int>` | — |
| `Rate` | `double` | Playback rate (`1.0` by default). |
| `RateChanged` | `EventCallback<double>` | — |
| `Rates` | `IReadOnlyList<double>?` | Available playback rates. |
| `Volume` | `double` | Audio volume between `0` and `1`. |
| `VolumeChanged` | `EventCallback<double>` | — |
| `Muted` | `bool` | Whether audio is muted. |
| `MutedChanged` | `EventCallback<bool>` | — |
| `Download` | `bool` | Enables the download button. |
| `DownloadUrl` | `string?` | — |
| `DownloadFileName` | `string?` | — |
| `Hide` | `IReadOnlyList<MediaPlayerControl>?` | Controls to hide from the bar. |
| `OnPlay` | `EventCallback` | — |
| `OnPause` | `EventCallback` | — |
| `OnEnded` | `EventCallback` | — |
| `OnLimitReached` | `EventCallback` | — |
| `OnTimeUpdate` | `EventCallback<MediaPlayerProgress>` | — |
| `OnDownload` | `EventCallback` | — |

### `<LxDisplay>`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/LxDisplay.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Kind` | `DisplayKind` | The display block kind to render. |
| `Variant` | `string?` | Visual variant within `Kind` (for example `"hero"`, `"trend"`, `"row"` for `Stat`; `"bar"`, `"segments"`, `"ring"`, `"scale"` for `Meter`; `"block"`, `"value"`, `"secret"` for `Code`). |
| `Surface` | `DisplaySurface?` | Surface treatment (`Plain`, `Muted`, `Bordered` or `Elevated`). When `null`, uses the default surface of `Kind`. |
| `Tone` | `LxColor` | Primary accent tone of the block. |
| `Loading` | `bool` | Shows a layout-matched placeholder skeleton instead of the block content. |
| `Title` | `string?` | Title or heading text of the block. |
| `Value` | `string?` | Primary text, message or value of the block. |
| `Description` | `string?` | Secondary description or caption text. |
| `Unit` | `string?` | Unit or aside text (for `Stat` or `Chart`). |
| `Icon` | `string?` | Leading icon name. |
| `HeadingLevel` | `int` | Heading level (`2` to `6`, `3` by default) for `Heading`. |
| `Count` | `string?` | Count badge text for `Heading` or `Record`. |
| `Chip` | `DisplayChip?` | Chip displayed beside the title or value. |
| `Action` | `DisplayAction?` | Primary action of the block. |
| `Actions` | `IReadOnlyList<DisplayAction>?` | Action buttons of the block. |
| `Paragraphs` | `IReadOnlyList<string>?` | Paragraphs for `Text`. |
| `Author` | `string?` | Quote author for `Text` (`Variant="quote"`). |
| `Meta` | `string?` | Quote or record metadata text. |
| `MaxLines` | `int?` | Maximum number of text lines before clamping with a Read more toggle (`Text`). |
| `Details` | `string?` | Collapsible details text (`Alert`) or footnote (`Breakdown`). |
| `DefaultOpen` | `bool` | Whether collapsible details or breakdown lines start open (`false` by default). |
| `Facts` | `IReadOnlyList<DisplayFact>?` | Fact items for `Facts`. |
| `FactGroups` | `IReadOnlyList<DisplayFactGroup>?` | Grouped facts for `Facts`. |
| `Delta` | `string?` | Delta text (for example `"+12.4%"`) for `Stat`. |
| `DeltaDirection` | `string` | Delta direction (`"up"`, `"down"` or `"flat"`) for `Stat`. |
| `Trend` | `IReadOnlyList<double>?` | Sparkline numeric series (2 or more points) for `Stat`. |
| `StatItems` | `IReadOnlyList<DisplayStatItem>?` | Row metrics for `Stat` (`Variant="row"`). |
| `BreakdownLines` | `IReadOnlyList<DisplayBreakdownLine>?` | Line items for `Breakdown`. |
| `BreakdownTotal` | `DisplayBreakdownLine?` | Total line for `Breakdown`. |
| `CompareColumns` | `(string First, string Second)?` | Comparison column headings for `Breakdown` (`Variant="compare"`). |
| `MeterValue` | `double` | Numeric value for `Meter`. |
| `MeterMax` | `double` | Maximum numeric value for `Meter` (`100` by default). |
| `WarningThreshold` | `double?` | Warning percentage threshold (0..100) for `Meter`. |
| `ErrorThreshold` | `double?` | Error percentage threshold (0..100) for `Meter`. |
| `MeterParts` | `IReadOnlyList<DisplayMeterPart>?` | Segments or parts for `Meter`. |
| `MeterLevels` | `IReadOnlyList<string>?` | Discrete level labels for `Meter` (`Variant="scale"`). |
| `CurrentLevel` | `int?` | Zero-based current level index for `Meter` (`Variant="scale"`). |
| `ChartData` | `IReadOnlyList<DisplayChartPoint>?` | Data points for `Chart`. |
| `TableColumns` | `IReadOnlyList<DisplayTableColumn>?` | Columns for `Table`. |
| `TableRows` | `IReadOnlyList<IReadOnlyDictionary<string, string?>>?` | Rows (keyed by column key) for `Table`. |
| `TableFooter` | `IReadOnlyDictionary<string, string?>?` | Optional footer cell values (keyed by column key) for `Table`. |
| `DiffItems` | `IReadOnlyList<DisplayDiffItem>?` | Diff entries for `Diff`. |
| `ShowUnchanged` | `bool` | Shows unchanged diff rows when `true` (`false` by default). |
| `Records` | `IReadOnlyList<DisplayRecordItem>?` | Record items for `Record`. |
| `People` | `IReadOnlyList<DisplayPersonItem>?` | People for `Person`. |
| `Contacts` | `IReadOnlyList<DisplayContactItem>?` | Contact entries for `Person` (`Variant="contact"`). |
| `ListItems` | `IReadOnlyList<DisplayListItem>?` | List items for `List`. |
| `TimelineItems` | `IReadOnlyList<DisplayTimelineItem>?` | Timeline items for `Timeline`. |
| `DateValue` | `DateTimeOffset?` | Date value for `Date`. |
| `EndDateValue` | `DateTimeOffset?` | End date for `Date` (`Variant="range"`). |
| `Now` | `DateTimeOffset?` | Reference "now" timestamp for `Date` (`Variant="countdown"`) or `Timeline`. |
| `Language` | `string?` | Code language (`"json"` enables token highlighting) for `Code`. |
| `VisibleChars` | `int` | Number of trailing characters left visible when `Code` is in `Variant="secret"` (`4` by default). |
| `Copy` | `bool` | Whether to show a copy button on `Code` (`true` by default). |
| `AccordionItems` | `IReadOnlyList<DisplayAccordionItem>?` | Accordion items for `Accordion`. |
| `Multiple` | `bool` | Allows multiple accordion items to be open at once (`false` by default). |
| `Src` | `string?` | Media source URL for `Media`. |
| `Images` | `IReadOnlyList<string>?` | Media gallery image URLs for `Media` (`Variant="gallery"`). |
| `Max` | `int?` | Maximum number of items initially revealed before a "Show more" button (`null` shows all). |
| `Step` | `int?` | Number of additional items revealed per click on "Show more" (when `null`, reveals all remaining items). |

### `<LxFilePreviewProvider>`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/LxFilePreviewProvider.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ChildContent` | `RenderFragment?` | Optional child content wrapped by the provider. |
| `MarkdownRenderer` | `Func<string, string>?` | Optional Markdown-to-HTML callback for `Markdown` files. |

### `<LxVideoPlayer>`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/LxVideoPlayer.razor`

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Src` | `string` | — |
| `Poster` | `string?` | — |
| `Title` | `string?` | — |
| `AspectRatio` | `string` | — |
| `Preload` | `string` | — |
| `Autoplay` | `bool` | — |
| `Loop` | `bool` | — |
| `PlaysInline` | `bool` | — |
| `Disabled` | `bool` | — |
| `Compact` | `bool` | — |
| `Keyboard` | `bool` | — |
| `Start` | `double` | — |
| `End` | `double?` | — |
| `Duration` | `double` | — |
| `Seekable` | `bool` | — |
| `MaxPlays` | `int` | — |
| `Plays` | `int` | — |
| `PlaysChanged` | `EventCallback<int>` | — |
| `Rate` | `double` | — |
| `RateChanged` | `EventCallback<double>` | — |
| `Rates` | `IReadOnlyList<double>?` | — |
| `Volume` | `double` | — |
| `VolumeChanged` | `EventCallback<double>` | — |
| `Muted` | `bool` | — |
| `MutedChanged` | `EventCallback<bool>` | — |
| `Download` | `bool` | — |
| `DownloadUrl` | `string?` | — |
| `DownloadFileName` | `string?` | — |
| `Tracks` | `IReadOnlyList<VideoPlayerTrack>?` | — |
| `Hide` | `IReadOnlyList<MediaPlayerControl>?` | — |
| `OnPlay` | `EventCallback` | — |
| `OnPause` | `EventCallback` | — |
| `OnEnded` | `EventCallback` | — |
| `OnLimitReached` | `EventCallback` | — |
| `OnTimeUpdate` | `EventCallback<MediaPlayerProgress>` | — |
| `OnDownload` | `EventCallback` | — |

## C# Types, Controllers, Models & Enums

### `enum FilePreviewKind`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewModels.cs`

Built-in file preview renderer kinds supported by `LxFilePreviewProvider` and `FilePreviewService`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Pdf` | `enum member` | — |
| `Image` | `enum member` | — |
| `Video` | `enum member` | — |
| `Audio` | `enum member` | — |
| `Csv` | `enum member` | — |
| `Markdown` | `enum member` | — |
| `Text` | `enum member` | — |
| `Office` | `enum member` | — |
| `Archive` | `enum member` | — |
| `Other` | `enum member` | — |

### `enum FilePreviewContainer`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewModels.cs`

Overlay container mode for `LxFilePreviewProvider`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Modal` | `enum member` | — |
| `Drawer` | `enum member` | — |
| `Fullscreen` | `enum member` | — |

### `record FilePreviewDetail`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewModels.cs`

Metadata key/value detail shown in the file preview details panel.

### `class FilePreviewAction`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewModels.cs`

Custom action shown in the file preview header.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string` | — |
| `Icon` | `string?` | — |
| `Href` | `string?` | — |
| `OnSelect` | `Func<FilePreviewFile, Task>?` | — |

### `class FilePreviewFile`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewModels.cs`

Describes a file to preview in `LxFilePreviewProvider` via `FilePreviewService`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Src` | `string` | URL or data URI of the file. |
| `Id` | `string?` | Optional stable identifier in a gallery. |
| `Name` | `string?` | Display file name. Defaults to the last path segment of `Src`. |
| `Mime` | `string?` | Optional MIME type (takes precedence over the file extension when detecting the renderer kind). |
| `Kind` | `FilePreviewKind?` | Optional explicit renderer kind overriding automatic detection. |
| `Size` | `long?` | Optional file size in bytes. |
| `UpdatedAt` | `DateTimeOffset?` | Optional last-modified timestamp. |
| `Poster` | `string?` | Optional poster image URL for video or audio files. |
| `Content` | `string?` | Optional inline text content (for CSV, Markdown, JSON or code files when already loaded in memory). |
| `Rendition` | `FilePreviewFile?` | Optional rendition file (for example a PDF preview of an Office document) used for the stage while keeping the original file for download. |
| `Details` | `IReadOnlyList<FilePreviewDetail>?` | Optional metadata details displayed in the file details drawer. |
| `DownloadUrl` | `string?` | Optional custom download URL (defaults to `Src`). |
| `Actions` | `IReadOnlyList<FilePreviewAction>?` | Optional custom actions in the preview header. |
| `Tracks` | `IReadOnlyList<VideoPlayerTrack>?` | Optional video caption tracks. |

### `class FilePreviewOptions`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewModels.cs`

Options when opening a file or gallery in `FilePreviewService`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Index` | `int` | Initial zero-based index in the file list. |
| `Mode` | `FilePreviewContainer?` | Optional container override (`FilePreviewContainer.Modal`, `FilePreviewContainer.Drawer` or `FilePreviewContainer.Fullscreen`). |
| `Loop` | `bool` | Wraps around at the first and last file when navigating with Previous / Next (`false` by default). |
| `PdfPage` | `int?` | Initial PDF page number (`#page=N`). |

### `record ParsedCsvPreview`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewService.cs`

Parsed CSV preview table returned by `FilePreviewService.ParseCsv`.

### `class FilePreviewService`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/FilePreviewService.cs`

Scoped service managing active file preview overlays rendered by `LxFilePreviewProvider`. Also provides deterministic file-kind detection, CSV parsing, and size/extension formatting utilities.

| Member | Type | Description |
| :--- | :--- | :--- |
| `IsMounted` | `bool` | Whether a `LxFilePreviewProvider` is currently mounted in the component tree. |
| `IsOpen` | `bool` | Whether the file preview overlay is currently open. |
| `Files` | `IReadOnlyList<FilePreviewFile>` | Files in the active gallery. |
| `CurrentIndex` | `int` | Zero-based index of the currently shown file in `Files`. |
| `Options` | `FilePreviewOptions` | Current options for the active preview session. |
| `CurrentFile` | `FilePreviewFile?` | Currently active file, or `null` when closed. |
| `Open(FilePreviewFile file, FilePreviewOptions? options = null)` | `void` | Opens a single file in `LxFilePreviewProvider`. |
| `OpenGallery(IReadOnlyList<FilePreviewFile> files, FilePreviewOptions? options = null)` | `void` | Opens a gallery of files in `LxFilePreviewProvider`. |
| `Close()` | `void` | Closes the active file preview overlay. |
| `Next()` | `void` | Moves to the next file in the gallery. |
| `Previous()` | `void` | Moves to the previous file in the gallery. |
| `GoTo(int index)` | `void` | Jumps to a specific zero-based index in the gallery. |
| `ResolveContainer()` | `FilePreviewContainer` | Resolves the largest container across all files in the current gallery unless overridden in `Options`. |
| `DefaultContainerForKind(FilePreviewKind kind)` | `FilePreviewContainer` | Returns the default container mode for a `FilePreviewKind`. |
| `DetectKind(FilePreviewFile file)` | `FilePreviewKind` | Detects the `FilePreviewKind` of a `FilePreviewFile` (checking explicit kind, MIME type, then file extension). |
| `DetectKind(string? fileNameOrUrl, string? mime = null)` | `FilePreviewKind` | Detects the `FilePreviewKind` from a file name and optional MIME type. |
| `ExtractExtension(string? fileNameOrUrl)` | `string?` | Extracts the lowercase extension (without leading dot) from a file name or URL. |
| `FileNameFromUrl(string? src)` | `string` | Extracts a display file name from a URL or returns `"Untitled"`. |
| `ExtensionLabel(string? fileName, string? mime = null)` | `string` | Returns a short uppercase badge label for a file's extension or MIME subtype. |
| `FormatFileSize(long bytes, CultureInfo? culture = null)` | `string` | Formats a byte count into a human-readable size string (`B`, `KB`, `MB`, `GB`). |
| `ParseCsv(string? content, string? fileName = null, int maxRows = 1000)` | `ParsedCsvPreview` | Parses CSV or TSV content according to RFC 4180 (quoted fields, escaped quotes, embedded newlines) and auto-detects the delimiter among `,`, `;`, `\t`, and `|`. |

### `enum MediaPlayerControl`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/MediaModels.cs`

Controls that can be selectively hidden on `LxAudioPlayer` and `LxVideoPlayer`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Time` | `enum member` | — |
| `Volume` | `enum member` | — |
| `Rate` | `enum member` | — |
| `Captions` | `enum member` | — |
| `PictureInPicture` | `enum member` | — |
| `Fullscreen` | `enum member` | — |
| `Download` | `enum member` | — |

### `record VideoPlayerTrack`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/MediaModels.cs`

A subtitle or caption track for `LxVideoPlayer`.

### `record MediaPlayerProgress`
*Package*: `Lux.Display.Core` · *Source*: `src/Lux.Display.Core/MediaModels.cs`

Playback progress snapshot reported by `LxAudioPlayer` and `LxVideoPlayer`. Seconds elapsed relative to the clip start. Total clip duration in seconds. Raw media element time in seconds.

### `enum DisplayKind`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

The 19 read-only display block kinds supported by `LxDisplay`.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Heading` | `enum member` | — |
| `Text` | `enum member` | — |
| `Alert` | `enum member` | — |
| `Facts` | `enum member` | — |
| `Stat` | `enum member` | — |
| `Breakdown` | `enum member` | — |
| `Meter` | `enum member` | — |
| `Chart` | `enum member` | — |
| `Table` | `enum member` | — |
| `Diff` | `enum member` | — |
| `Record` | `enum member` | — |
| `Person` | `enum member` | — |
| `List` | `enum member` | — |
| `Timeline` | `enum member` | — |
| `Date` | `enum member` | — |
| `Media` | `enum member` | — |
| `Code` | `enum member` | — |
| `Empty` | `enum member` | — |
| `Accordion` | `enum member` | — |

### `enum DisplaySurface`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

Container surface treatment of a `LxDisplay` block.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Plain` | `enum member` | — |
| `Muted` | `enum member` | — |
| `Bordered` | `enum member` | — |
| `Elevated` | `enum member` | — |

### `record DisplayChip`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

A chip/badge displayed inside a `LxDisplay` block.

### `class DisplayAction`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

An action button or link displayed inside a `LxDisplay` block.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string` | — |
| `Icon` | `string?` | — |
| `Href` | `string?` | — |
| `Target` | `string?` | — |
| `OnClick` | `EventCallback` | — |
| `Color` | `LxColor` | — |
| `Variant` | `ButtonVariant` | — |

### `class DisplayFact`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One key/value entry in a `Facts` block.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Label` | `string` | — |
| `Value` | `string?` | — |
| `Hint` | `string?` | — |
| `Chip` | `DisplayChip?` | — |
| `Tone` | `LxColor?` | — |
| `Copy` | `bool` | — |
| `CopyText` | `string?` | — |
| `Mono` | `bool` | — |
| `Emphasis` | `bool` | — |
| `Icon` | `string?` | — |
| `Href` | `string?` | — |
| `Loading` | `bool` | — |

### `record DisplayFactGroup`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

A named group of `DisplayFact` items in a `Facts` block.

### `record DisplayStatItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One sub-metric in the `row` variant of a `Stat` block.

### `record DisplayBreakdownLine`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One line item in a `Breakdown` block.

### `record DisplayMeterPart`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One segment or part in a `Meter` block.

### `record DisplayChartPoint`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One data point in a `Chart` block.

### `record DisplayTableColumn`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One column definition in a `Table` block.

### `record DisplayDiffItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One field difference in a `Diff` block.

### `class DisplayRecordItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One entity record in a `Record` block.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Reference` | `string?` | — |
| `Title` | `string` | — |
| `Meta` | `string?` | — |
| `MetaIcon` | `string?` | — |
| `Description` | `string?` | — |
| `Icon` | `string?` | — |
| `AvatarText` | `string?` | — |
| `ImageSrc` | `string?` | — |
| `Tone` | `LxColor?` | — |
| `Badges` | `IReadOnlyList<DisplayChip>?` | — |
| `Chip` | `DisplayChip?` | — |
| `Trailing` | `string?` | — |
| `TrailingCaption` | `string?` | — |
| `Href` | `string?` | — |
| `OnClick` | `EventCallback` | — |

### `record DisplayPersonItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One person in a `Person` block.

### `record DisplayContactItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One contact channel in the `contact` variant of a `Person` block.

### `class DisplayListItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One entry in a `List` block.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Text` | `string` | — |
| `Description` | `string?` | — |
| `Icon` | `string?` | — |
| `Tone` | `LxColor?` | — |
| `Done` | `bool` | — |
| `Current` | `bool` | — |
| `Meta` | `string?` | — |
| `Href` | `string?` | — |
| `FileName` | `string?` | — |
| `FileSize` | `long?` | — |
| `MimeType` | `string?` | — |

### `enum DisplayTimelineState`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

State of a step in a `Timeline` block.

| Member | Type | Description |
| :--- | :--- | :--- |
| `Done` | `enum member` | — |
| `Current` | `enum member` | — |
| `Upcoming` | `enum member` | — |

### `record DisplayTimelineItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One event in a `Timeline` block.

### `record DisplayAccordionItem`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/DisplayModels.cs`

One collapsible section in a `Accordion` block.

### `class LuxDisplayServiceCollectionExtensions`
*Package*: `Lux.Display` · *Source*: `src/Lux.Display/LuxDisplayServiceCollectionExtensions.cs`

| Member | Type | Description |
| :--- | :--- | :--- |
| `AddLuxDisplay(this IServiceCollection services)` | `IServiceCollection` | — |
