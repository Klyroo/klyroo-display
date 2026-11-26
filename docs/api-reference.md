# Klyroo.Display API reference

> Generated from the compiled assemblies and their XML documentation. Do not edit by hand.

## Klyroo.Display.Core

Classes (4) · Records (4) · Enums (3)

### Classes

#### `FilePreviewAction`

`Klyroo.Display` · class

A custom button added to the header of the file preview.

| Constructor | Description |
| :--- | :--- |
| `FilePreviewAction()` | Creates an action. Set `FilePreviewAction.Label` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Href` (init) | `string?` | A link target. The current provider does not use it: handle the click in `FilePreviewAction.OnSelect`. |
| `Icon` (init) | `string?` | An icon name. The current provider does not render it. |
| `Label` (init) | `string` | The button text. |
| `OnSelect` (init) | `Func<FilePreviewFile, Task>?` | The callback invoked with the file being previewed when the button is clicked. |

#### `FilePreviewFile`

`Klyroo.Display` · class

A file to show in `KlyFilePreviewProvider`. Pass it to `FilePreviewService.Open` or `FilePreviewService.OpenGallery`.

| Constructor | Description |
| :--- | :--- |
| `FilePreviewFile()` | Creates a file. Set `FilePreviewFile.Src` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Actions` (init) | `IReadOnlyList<FilePreviewAction>?` | Custom buttons added to the header while this file is shown. |
| `Content` (init) | `string?` | The text of the file. CSV, Markdown and text files are rendered from it: the provider does not download `FilePreviewFile.Src` for them. |
| `Details` (init) | `IReadOnlyList<FilePreviewDetail>?` | Extra rows for the details panel. |
| `DownloadUrl` (init) | `string?` | The URL of the download link. When `null`, `FilePreviewFile.Src` is used. |
| `Id` (init) | `string?` | An optional stable identifier of the file. The preview does not use it. |
| `Kind` (init) | `FilePreviewKind?` | The kind to use, skipping detection. Set it when the name and MIME type do not tell the kind, for example a signed URL. |
| `Mime` (init) | `string?` | The MIME type. When set and recognized, it takes precedence over the file extension when the kind is detected. |
| `Name` (init) | `string?` | The display file name. When `null`, the last path segment of `FilePreviewFile.Src` is used. |
| `Poster` (init) | `string?` | The poster image of a video, or the cover image of an audio file. |
| `Rendition` (init) | `FilePreviewFile?` | A file shown instead of this one, for example a PDF conversion of an Office document. The original stays available for download. |
| `Size` (init) | `long?` | The size in bytes, shown in the header and in the details panel. |
| `Src` (init) | `string` | The URL or data URI of the file. Images, video, audio and PDF are loaded from it by the browser. |
| `Tracks` (init) | `IReadOnlyList<VideoPlayerTrack>?` | Caption tracks passed to the video player. |
| `UpdatedAt` (init) | `DateTimeOffset?` | The last-modified time, shown in the header as a date. |

#### `FilePreviewOptions`

`Klyroo.Display` · class

Options for a preview session started with `FilePreviewService.Open` or `FilePreviewService.OpenGallery`.

| Constructor | Description |
| :--- | :--- |
| `FilePreviewOptions()` | Creates options with the defaults. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Index` (init) | `int` | The zero-based index of the file shown first. It is clamped to the gallery. |
| `Loop` (init) | `bool` | Whether Previous and Next wrap around at the ends of the gallery (`false` by default). |
| `Mode` (init) | `FilePreviewContainer?` | The container to use. When `null`, `FilePreviewService.ResolveContainer` picks one from the kinds of the files. |
| `PdfPage` (init) | `int?` | The page a PDF opens on, passed to the browser viewer as `#page=N`. It is ignored when the URL already has a fragment. |

#### `FilePreviewService`

`Klyroo.Display` · class

Holds the state of the file preview (the open gallery, the current file, the options) and notifies `KlyFilePreviewProvider`, which renders it. Inject it, call `FilePreviewService.Open` or `FilePreviewService.OpenGallery`, and the provider shows the overlay. It also exposes static helpers that need no state: file kind detection, CSV parsing and size formatting.

| Constructor | Description |
| :--- | :--- |
| `FilePreviewService()` | Creates a service with no open preview. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `CurrentFile` | `FilePreviewFile?` | The file shown, or `null` when no session is open. |
| `CurrentIndex` | `int` | The zero-based index of the file shown, in `FilePreviewService.Files`. |
| `Files` | `IReadOnlyList<FilePreviewFile>` | The files of the current gallery. It keeps the last gallery after `FilePreviewService.Close`. |
| `IsMounted` | `bool` | Whether a `KlyFilePreviewProvider` is mounted. Without one, opening a file shows nothing. |
| `IsOpen` | `bool` | Whether a preview session is open. |
| `Options` | `FilePreviewOptions` | The options of the current session. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Close()` | `void` | Closes the preview. Does nothing when it is already closed. |
| `static DefaultContainerForKind(FilePreviewKind kind)` | `FilePreviewContainer` | Returns the container suited to a kind: a drawer for PDF, CSV, Markdown and text, a modal for the others. |
| `static DetectKind(FilePreviewFile file)` | `FilePreviewKind` | Detects the kind of a file: its `FilePreviewFile.Kind` if set, else its MIME type, else the extension of its name or URL. |
| `static DetectKind(string? fileNameOrUrl, string? mime = null)` | `FilePreviewKind` | Detects a kind from a MIME type, then from the extension of a file name or URL. A MIME type that is not recognized falls through to the extension. |
| `static ExtensionLabel(string? fileName, string? mime = null)` | `string` | Returns a short uppercase label for a badge: the extension, else the MIME subtype. |
| `static ExtractExtension(string? fileNameOrUrl)` | `string?` | Extracts the lowercase extension, without the dot, from a file name or URL. |
| `static FileNameFromUrl(string? src)` | `string` | Returns the last path segment of a URL, unescaped, to use as a file name. |
| `static FormatFileSize(long bytes, CultureInfo? culture = null)` | `string` | Formats a byte count with binary steps of 1024 and the units `B`, `KB`, `MB` and `GB`. Values under 10 keep one decimal. |
| `GoTo(int index)` | `void` | Jumps to a file by index. The index wraps when `FilePreviewOptions.Loop` is set, and is clamped otherwise. |
| `Next()` | `void` | Moves to the next file. At the last file it wraps to the first when `FilePreviewOptions.Loop` is set, and stays otherwise. |
| `Open(FilePreviewFile file, FilePreviewOptions? options = null)` | `void` | Opens a single file in the preview. |
| `OpenGallery(IReadOnlyList<FilePreviewFile> files, FilePreviewOptions? options = null)` | `void` | Opens a gallery of files in the preview. An empty list does nothing. |
| `static ParseCsv(string? content, string? fileName = null, int maxRows = 1000)` | `ParsedCsvPreview` | Parses CSV or TSV text: quoted fields, doubled quotes and newlines inside quotes follow RFC 4180, and a leading byte order mark is removed. The delimiter is detected among `,`, `;`, tab and `\|` from the first line, and is a tab when `fileName` ends in `.tsv`. |
| `Previous()` | `void` | Moves to the previous file. At the first file it wraps to the last when `FilePreviewOptions.Loop` is set, and stays otherwise. |
| `ResolveContainer()` | `FilePreviewContainer` | Returns `FilePreviewOptions.Mode` when it is set. Otherwise returns the largest container suggested by `FilePreviewService.DefaultContainerForKind` across the gallery (using each file's rendition when it has one), so a gallery with a PDF opens in a drawer. |

### Records

#### `FilePreviewDetail`

`Klyroo.Display` · record

A label and value pair shown in the details panel of the file preview.

| Constructor | Description |
| :--- | :--- |
| `FilePreviewDetail(string Label, string Value)` | A label and value pair shown in the details panel of the file preview. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Label` (init) | `string` | The row label, for example `"Uploaded by"`. |
| `Value` (init) | `string` | The row value. |

#### `MediaPlayerProgress`

`Klyroo.Display` · record

The playback position reported by the `OnTimeUpdate` callback of the audio and video players.

| Constructor | Description |
| :--- | :--- |
| `MediaPlayerProgress(double Position, double Length, double CurrentTime)` | The playback position reported by the `OnTimeUpdate` callback of the audio and video players. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `CurrentTime` (init) | `double` | Raw media element time in seconds. |
| `Length` (init) | `double` | Total clip duration in seconds. |
| `Position` (init) | `double` | Seconds elapsed relative to the clip start. |

#### `ParsedCsvPreview`

`Klyroo.Display` · record

A CSV or TSV table parsed by `FilePreviewService.ParseCsv`.

| Constructor | Description |
| :--- | :--- |
| `ParsedCsvPreview(char Delimiter, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows, IReadOnlyList<bool> NumericColumns, bool Truncated)` | A CSV or TSV table parsed by `FilePreviewService.ParseCsv`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Delimiter` (init) | `char` | The field delimiter that was used. |
| `Headers` (init) | `IReadOnlyList<string>` | The fields of the first row. |
| `NumericColumns` (init) | `IReadOnlyList<bool>` | One flag per header: `true` when the column has a value and every non-empty value parses as a number. |
| `Rows` (init) | `IReadOnlyList<IReadOnlyList<string>>` | The data rows after the header, at most the requested maximum. |
| `Truncated` (init) | `bool` | `true` when the content had more rows than were kept. |

#### `VideoPlayerTrack`

`Klyroo.Display` · record

A subtitle or caption track of a video, rendered as a `track` element.

| Constructor | Description |
| :--- | :--- |
| `VideoPlayerTrack(string Src, string SrcLang, string Label, string Kind = "subtitles", bool Default = false)` | A subtitle or caption track of a video, rendered as a `track` element. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Default` (init) | `bool` | Whether the track is selected initially. |
| `Kind` (init) | `string` | The `kind` attribute of the track (`"subtitles"` by default). |
| `Label` (init) | `string` | The label shown in the browser's track list. |
| `Src` (init) | `string` | The URL of the WebVTT file. |
| `SrcLang` (init) | `string` | The BCP 47 language code of the track, for example `"fr"`. |

### Enums

#### `FilePreviewContainer`

`Klyroo.Display` · enum

The layout of the preview overlay rendered by `KlyFilePreviewProvider`.

| Value | Description |
| :--- | :--- |
| `Modal` | A centered dialog, at most 5xl wide. |
| `Drawer` | A panel docked to the end edge of the viewport, at most 4xl wide. |
| `Fullscreen` | The whole viewport. |

#### `FilePreviewKind`

`Klyroo.Display` · enum

The kind of a file as seen by the preview. `FilePreviewService.DetectKind` returns one of these values, and `KlyFilePreviewProvider` picks its renderer from it.

| Value | Description |
| :--- | :--- |
| `Pdf` | A PDF document, shown in the browser's built-in PDF viewer inside an `iframe`. |
| `Image` | A raster or vector image, shown with zoom and rotation controls. |
| `Video` | A video, shown in `KlyVideoPlayer`. |
| `Audio` | An audio file, shown in `KlyAudioPlayer`. |
| `Csv` | A CSV or TSV file, shown as a table parsed from `FilePreviewFile.Content`. |
| `Markdown` | A Markdown file, shown as source, or as HTML when the provider has a `MarkdownRenderer`. |
| `Text` | A plain text, JSON, XML or source code file, shown from `FilePreviewFile.Content`. |
| `Office` | A word-processing, spreadsheet or presentation file. It has no built-in renderer, so provide a `FilePreviewFile.Rendition` to preview it. |
| `Archive` | A compressed archive. It has no built-in renderer and offers a download instead. |
| `Other` | Any file whose kind could not be detected. It offers a download instead of a preview. |

#### `MediaPlayerControl`

`Klyroo.Display` · enum

A control of `KlyAudioPlayer` or `KlyVideoPlayer` that can be hidden through their `Hide` parameter.

| Value | Description |
| :--- | :--- |
| `Time` | The elapsed and total time label. |
| `Volume` | The mute button. |
| `Rate` | The playback speed button. |
| `Captions` | The captions button of the video player. |
| `PictureInPicture` | Reserved: neither player renders a picture-in-picture button yet. |
| `Fullscreen` | Reserved: neither player renders a fullscreen button yet. |
| `Download` | The download link. |

## Klyroo.Display

Components (4) · Static classes and extensions (1) · Classes (4) · Records (12) · Enums (3)

### Components

#### `KlyAudioPlayer`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A compact audio control bar with play and pause, a timeline, a mute button, a speed button and an optional download link, that can restrict playback to a clip (`KlyAudioPlayer.Start` and `KlyAudioPlayer.End`), to a number of plays (`KlyAudioPlayer.MaxPlays`) and to a non-seekable timeline.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Autoplay` | `bool` | Starts playing once the player is interactive. Browsers can refuse to start a player that is not muted before the user has interacted with the page; the player then stays paused. |
| `Compact` | `bool` | Not used in this version. |
| `Disabled` | `bool` | Disables the play, seek and mute controls. |
| `Download` | `bool` | Shows the download link (`false` by default). The link also shows when `KlyAudioPlayer.DownloadUrl` is set. |
| `DownloadFileName` | `string?` | The file name suggested to the browser by the download link. |
| `DownloadUrl` | `string?` | The target of the download link. When `null`, `KlyAudioPlayer.Src` is used. |
| `Duration` | `double` | The total duration in seconds, used until the media element reports its own (`60` by default). Set `KlyAudioPlayer.End` to stop before the end of the file. |
| `End` | `double?` | The clip end, in seconds. It is used when it is greater than `KlyAudioPlayer.Start`, otherwise `KlyAudioPlayer.Duration` gives the end. |
| `Hide` | `IReadOnlyList<MediaPlayerControl>?` | The controls to remove from the control bar. |
| `Keyboard` | `bool` | Whether the focused player reacts to keys: Space or K toggles playback, M toggles mute (`true` by default). |
| `Loop` | `bool` | Sets `loop` on the media element. It is ignored when the player is restricted by `KlyAudioPlayer.MaxPlays` or by `KlyAudioPlayer.Seekable` being `false`. |
| `MaxPlays` | `int` | The number of times the clip can be started (`0`, the default, means unlimited). Once `KlyAudioPlayer.Plays` reaches it and the current play ends, the player locks and shows that no plays are left. |
| `Muted` | `bool` | Whether the player is muted. The mute button toggles it and raises `KlyAudioPlayer.MutedChanged`. |
| `MutedChanged` | `EventCallback<bool>` | Raised with the new value when the mute button is clicked. |
| `OnDownload` | `EventCallback` | Raised when the download link is clicked. |
| `OnEnded` | `EventCallback` | Raised when the position reaches the end of the clip. |
| `OnLimitReached` | `EventCallback` | Raised when no plays are left: when a play ends with `KlyAudioPlayer.Plays` at `KlyAudioPlayer.MaxPlays`, or when a new play is requested after that. |
| `OnPause` | `EventCallback` | Raised when the user pauses. |
| `OnPlay` | `EventCallback` | Raised when playback starts or resumes. |
| `OnTimeUpdate` | `EventCallback<MediaPlayerProgress>` | Raised with the position during playback, about four times a second, and when the user seeks. |
| `Plays` | `int` | The number of plays already used. The component increments it when a play starts and raises `KlyAudioPlayer.PlaysChanged`: keep the value in your own state and pass it back, so a re-render does not lose it. |
| `PlaysChanged` | `EventCallback<int>` | Raised with the new count when a play starts. |
| `Preload` | `string` | The `preload` attribute of the media element (`"metadata"` by default). |
| `Rate` | `double` | The playback rate shown on the speed button (`1.0` by default). |
| `RateChanged` | `EventCallback<double>` | Raised with the new rate when the speed button is clicked. |
| `Rates` | `IReadOnlyList<double>?` | The rates the speed button cycles through. The button is hidden when there are fewer than two (0.5, 0.75, 1, 1.25, 1.5 and 2 by default). |
| `Seekable` | `bool` | Whether the user can move the position with the timeline. When `false` the timeline is a read-only progress bar (`true` by default). |
| `Src` | `string` | The URL of the media file. |
| `Start` | `double` | The clip start, in seconds. Positions and durations are reported relative to it. |
| `Title` | `string?` | The accessible name of the player region. |
| `Volume` | `double` | The volume of the media element, from 0 to 1 (`1.0` by default). The player has no volume slider, so set it from your own control. |
| `VolumeChanged` | `EventCallback<double>` | Not raised in this version: the player has no volume slider. |

Also accepts the parameters of `KlyComponentBase`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `ClipEnd` | `double` | The clip end in seconds: `KlyAudioPlayer.End` when it is greater than `KlyAudioPlayer.Start`, otherwise `KlyAudioPlayer.Duration`. |
| `ClipLength` | `double` | The clip length in seconds, `KlyAudioPlayer.ClipEnd` minus `KlyAudioPlayer.Start`. |
| `ClipPosition` | `double` | The position inside the clip in seconds, between zero and `KlyAudioPlayer.ClipLength`. |
| `IsLocked` | `bool` | Whether the player is locked: `KlyAudioPlayer.MaxPlays` is set, no plays are left and no play is in progress. |
| `RemainingPlays` | `int` | The number of plays left, or `Int32.MaxValue` when `KlyAudioPlayer.MaxPlays` is zero. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `CompletePlayAsync()` | `Task` | Ends the current play: stops, moves the position to the clip end, and raises `KlyAudioPlayer.OnEnded` and, when no plays are left, `KlyAudioPlayer.OnLimitReached`. The player calls it when the media reaches the clip end. |
| `CycleRateAsync()` | `Task` | Selects the next entry of `KlyAudioPlayer.Rates` (wrapping around), and raises `KlyAudioPlayer.RateChanged`. It does nothing with fewer than two rates. |
| `SeekAsync(double clipOffsetSeconds)` | `Task` | Moves the position to an offset inside the clip, raises `KlyAudioPlayer.OnTimeUpdate`, and ends the play when the offset reaches the clip end. It does nothing unless `KlyAudioPlayer.Seekable`, and when `KlyAudioPlayer.Disabled` or `KlyAudioPlayer.IsLocked`. |
| `ToggleMuteAsync()` | `Task` | Toggles `KlyAudioPlayer.Muted` and raises `KlyAudioPlayer.MutedChanged`. |
| `TogglePlayAsync()` | `Task` | Starts, resumes or pauses the player, as the play button does. Starting a new play consumes one of `KlyAudioPlayer.MaxPlays`. It does nothing when `KlyAudioPlayer.Disabled` or `KlyAudioPlayer.IsLocked`. |

#### `KlyDisplay`

`Klyroo.Display` · component · inherits `KlyComponentBase`

Renders a read-only block of information: a heading, a text, an alert, a list of facts, a statistic, a breakdown, a meter, a chart, a table, a diff, records, people, a list, a timeline, a date, an image, a code value, an empty state or an accordion. Set `KlyDisplay.Kind` to choose the block, and fill the parameters that kind reads.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AccordionItems` | `IReadOnlyList<DisplayAccordionItem>?` | The sections of an Accordion block. |
| `Action` | `DisplayAction?` | The button of a Heading block, shown at the end of the row. |
| `Actions` | `IReadOnlyList<DisplayAction>?` | The buttons of an Alert or Empty block. |
| `Author` | `string?` | The author shown in the footer of a Text block, before `KlyDisplay.Meta`. |
| `BreakdownLines` | `IReadOnlyList<DisplayBreakdownLine>?` | The lines of a Breakdown block. |
| `BreakdownTotal` | `DisplayBreakdownLine?` | The total row of a Breakdown block, shown below the lines. |
| `ChartData` | `IReadOnlyList<DisplayChartPoint>?` | The bars of a Chart block, or the steps of a `funnel` Chart. |
| `Chip` | `DisplayChip?` | The badge of the single person shown by a Person block when `KlyDisplay.People` is empty. |
| `CompareColumns` | `(string, string)?` | Not used in this version: a Breakdown has no comparison layout. |
| `Contacts` | `IReadOnlyList<DisplayContactItem>?` | The contact lines of a Person block. They are shown under the person and are ignored by the `group` variant. |
| `Copy` | `bool` | Whether a Code block shows a copy button (`true` by default). |
| `Count` | `string?` | Count text shown beside the title of a Heading or Record block. |
| `CurrentLevel` | `int?` | The zero-based index of the current level of a `scale` Meter. The levels up to it are highlighted. |
| `DateValue` | `DateTimeOffset?` | The date of a Date block, or the start of a `range`. When `null`, the current date is used. |
| `DefaultOpen` | `bool` | Whether the Alert details, or the lines of a `collapsible` Breakdown, start open. It is read once, when the component first receives its parameters. |
| `Delta` | `string?` | The change shown as a badge beside a Stat value, for example `"+12.4%"`. It is displayed as given. |
| `DeltaDirection` | `string` | The direction of `KlyDisplay.Delta`: `"up"` (green, the default), `"down"` (red) or `"flat"` (neutral). |
| `Description` | `string?` | A secondary line: under the title of a Heading, Stat, Meter, Date, Media (caption) or Empty block, under the name of a single Person, or the Alert message when `KlyDisplay.Value` is not set. |
| `Details` | `string?` | The collapsible details of an Alert (opened with Show details), or the footnote of a Breakdown. |
| `DiffItems` | `IReadOnlyList<DisplayDiffItem>?` | The fields of a Diff block. |
| `EndDateValue` | `DateTimeOffset?` | The end of a `range` Date block, inclusive in the day count. |
| `ErrorThreshold` | `double?` | The percentage of `KlyDisplay.MeterMax` from which the Meter bar turns to the error color. It wins over `KlyDisplay.WarningThreshold`. |
| `FactGroups` | `IReadOnlyList<DisplayFactGroup>?` | The titled groups of a Facts block. When set, they replace `KlyDisplay.Facts`, and `KlyDisplay.Max` does not apply. |
| `Facts` | `IReadOnlyList<DisplayFact>?` | The rows of a Facts block. They are ignored when `KlyDisplay.FactGroups` is set. |
| `HeadingLevel` | `int` | Not used in this version: a Heading always renders an `h3`, and the `eyebrow` variant renders no heading element. |
| `Icon` | `string?` | The name of the leading icon of a Heading, Alert or Empty block. For an Alert, an empty string hides the icon. |
| `Images` | `IReadOnlyList<string>?` | The image URLs of a Media block with `Variant="gallery"`. |
| `Kind` | `DisplayKind` | The kind of block to render (`DisplayKind.Facts` by default). |
| `Language` | `string?` | The label of a Code block when `KlyDisplay.Title` is not set. It does not enable syntax highlighting. |
| `ListItems` | `IReadOnlyList<DisplayListItem>?` | The items of a List block. |
| `Loading` | `bool` | Shows a skeleton placeholder instead of the block. |
| `Max` | `int?` | The number of items shown before a Show more button, for the flat Facts, Table, Record, List and Timeline kinds (all items when `null`). In a `group` Person block it is the number of avatars (4 by default), and in a `gallery` Media block the number of tiles (3 by default). |
| `MaxLines` | `int?` | When greater than zero, clamps `KlyDisplay.Value` of a Text block to two lines and adds a Read more toggle. The number itself is not used, and `KlyDisplay.Paragraphs` are not clamped. |
| `Meta` | `string?` | Extra text shown in the footer of a Text block, after `KlyDisplay.Author`. |
| `MeterLevels` | `IReadOnlyList<string>?` | The level labels of a Meter block with `Variant="scale"`. |
| `MeterMax` | `double` | The maximum of a Meter block (`100` by default). When it is zero or less, the sum of `KlyDisplay.MeterParts` is used. |
| `MeterParts` | `IReadOnlyList<DisplayMeterPart>?` | The segments of a Meter block. When set, they replace the single bar and the thresholds do not apply. |
| `MeterValue` | `double` | The current value of a Meter block, clamped between 0 and `KlyDisplay.MeterMax`. |
| `Multiple` | `bool` | Allows several Accordion sections to be open at once (`false` by default). |
| `Now` | `DateTimeOffset?` | The reference time of a `countdown` Date block. When `null`, `DateTimeOffset.UtcNow` is used. Pass it for tests and for stable server rendering. |
| `Paragraphs` | `IReadOnlyList<string>?` | The paragraphs of a Text block. When set, they replace `KlyDisplay.Value`. |
| `People` | `IReadOnlyList<DisplayPersonItem>?` | The people of a Person block. Without the `group` variant, only the first person is shown. |
| `Records` | `IReadOnlyList<DisplayRecordItem>?` | The rows of a Record block. |
| `ShowUnchanged` | `bool` | Shows the fields whose `DisplayDiffItem.Changed` is `false` (`false` by default). |
| `Src` | `string?` | The image URL of a Media block. |
| `StatItems` | `IReadOnlyList<DisplayStatItem>?` | The metrics of a Stat block with `Variant="row"`. |
| `Step` | `int?` | The number of items each click on Show more reveals (all remaining items when `null`). |
| `Surface` | `DisplaySurface?` | The surface treatment (`DisplaySurface.Plain`, `DisplaySurface.Muted`, `DisplaySurface.Bordered` or `DisplaySurface.Elevated`). When `null`, a default is chosen from `KlyDisplay.Kind`: bordered for Facts, Breakdown, Table, Record and Code, muted for Alert and Stat (Stat is bordered for its `row` variant), and plain for the others. |
| `TableColumns` | `IReadOnlyList<DisplayTableColumn>?` | The columns of a Table block. |
| `TableFooter` | `IReadOnlyDictionary<string, string?>?` | The footer row of a Table block, as a map from column key to text. |
| `TableRows` | `IReadOnlyList<IReadOnlyDictionary<string, string?>>?` | The rows of a Table block. Each row maps a column key to its text, and a missing key renders a dash. |
| `TimelineItems` | `IReadOnlyList<DisplayTimelineItem>?` | The events of a Timeline block. |
| `Title` | `string?` | The title of the block: the heading text, the bold first line of a Text, the Alert title, the label of a Stat, Meter or Code block, and the title of the other kinds that show one. For a Person block it is the name of the single person, or the caption of a `group`. For a Date block it replaces the formatted date, and for a Media block it is the alt text. |
| `Tone` | `KlyColor` | The tone of the block. Only `DisplayKind.Alert` reads it: it selects the default icon and sets `role="status"` for `KlyColor.Warning` and `KlyColor.Error`. |
| `Trend` | `IReadOnlyList<double>?` | The series of a Stat sparkline. It is drawn from two points or more. |
| `Unit` | `string?` | The unit shown beside a Stat value, or at the end of a Chart title. |
| `Value` | `string?` | The main text of the block: the Text body, the Alert message, the Stat value, the Meter value label, or the Code content. |
| `Variant` | `string?` | Selects a variant of the kind. Matching is case-insensitive and an unknown value renders the default layout. Heading: `eyebrow`. Text: `quote`. Stat: `row`. Breakdown: `collapsible`. Meter: `scale`. Chart: `funnel`. Person: `group`. List: `steps`, `checks`, `files`. Timeline: `pipeline`. Date: `countdown`, `range`. Media: `gallery`. Code: `secret`. |
| `VisibleChars` | `int` | The number of trailing characters left visible by a `secret` Code block (`4` by default). A longer value shows eight bullets followed by those characters. A value that is not longer than this number is fully masked. |
| `WarningThreshold` | `double?` | The percentage of `KlyDisplay.MeterMax` from which the Meter bar turns to the warning color. |

Also accepts the parameters of `KlyComponentBase`.

#### `KlyFilePreviewProvider`

`Klyroo.Display` · component · inherits `KlyComponentBase`

Renders the file preview overlay that `FilePreviewService` opens: a dialog, drawer or fullscreen panel with a header (name, kind, size, zoom and rotate for images, custom actions, details, download, close), a stage that renders the file, a rendition notice, and a strip of thumbnails for galleries. Place one provider in your layout, once.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ChildContent` | `RenderFragment?` | Content rendered before the overlay. Use it to wrap your layout, or leave it empty and place the provider next to it. |
| `MarkdownRenderer` | `Func<string, string>?` | Converts the Markdown of a file to HTML. The result is rendered as markup without sanitizing, so use a renderer that escapes untrusted input. When `null`, Markdown files are shown as source. |

Also accepts the parameters of `KlyComponentBase`.

#### `KlyVideoPlayer`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A video frame with an overlay play button and a control bar (play and pause, timeline, mute, speed, captions and an optional download link), that can restrict playback to a clip (`KlyVideoPlayer.Start` and `KlyVideoPlayer.End`), to a number of plays (`KlyVideoPlayer.MaxPlays`) and to a non-seekable timeline.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AspectRatio` | `string` | The CSS `aspect-ratio` of the player (`"16 / 9"` by default). |
| `Autoplay` | `bool` | Starts playing once the player is interactive. Browsers can refuse to start a player that is not muted before the user has interacted with the page; the player then stays paused. |
| `Compact` | `bool` | Not used in this version. |
| `Disabled` | `bool` | Disables the play, seek and mute controls. |
| `Download` | `bool` | Shows the download link (`false` by default). The link also shows when `KlyVideoPlayer.DownloadUrl` is set. |
| `DownloadFileName` | `string?` | The file name suggested to the browser by the download link. |
| `DownloadUrl` | `string?` | The target of the download link. When `null`, `KlyVideoPlayer.Src` is used. |
| `Duration` | `double` | The total duration in seconds, used until the media element reports its own (`60` by default). Set `KlyVideoPlayer.End` to stop before the end of the file. |
| `End` | `double?` | The clip end, in seconds. It is used when it is greater than `KlyVideoPlayer.Start`, otherwise `KlyVideoPlayer.Duration` gives the end. |
| `Hide` | `IReadOnlyList<MediaPlayerControl>?` | The controls to remove from the control bar. |
| `Keyboard` | `bool` | Whether the focused player reacts to keys: Space or K toggles playback, M toggles mute (`true` by default). |
| `Loop` | `bool` | Sets `loop` on the media element. It is ignored when the player is restricted by `KlyVideoPlayer.MaxPlays` or by `KlyVideoPlayer.Seekable` being `false`. |
| `MaxPlays` | `int` | The number of times the clip can be started (`0`, the default, means unlimited). Once `KlyVideoPlayer.Plays` reaches it and the current play ends, the player locks and shows that no plays are left. |
| `Muted` | `bool` | Whether the player is muted. The mute button toggles it and raises `KlyVideoPlayer.MutedChanged`. |
| `MutedChanged` | `EventCallback<bool>` | Raised with the new value when the mute button is clicked. |
| `OnDownload` | `EventCallback` | Raised when the download link is clicked. |
| `OnEnded` | `EventCallback` | Raised when the position reaches the end of the clip. |
| `OnLimitReached` | `EventCallback` | Raised when no plays are left: when a play ends with `KlyVideoPlayer.Plays` at `KlyVideoPlayer.MaxPlays`, or when a new play is requested after that. |
| `OnPause` | `EventCallback` | Raised when the user pauses. |
| `OnPlay` | `EventCallback` | Raised when playback starts or resumes. |
| `OnTimeUpdate` | `EventCallback<MediaPlayerProgress>` | Raised with the position during playback, about four times a second, and when the user seeks. |
| `Plays` | `int` | The number of plays already used. The component increments it when a play starts and raises `KlyVideoPlayer.PlaysChanged`: keep the value in your own state and pass it back, so a re-render does not lose it. |
| `PlaysChanged` | `EventCallback<int>` | Raised with the new count when a play starts. |
| `PlaysInline` | `bool` | Sets `playsinline` on the video element (`true` by default). |
| `Poster` | `string?` | The poster image shown before the video plays. |
| `Preload` | `string` | The `preload` attribute of the media element (`"metadata"` by default). |
| `Rate` | `double` | The playback rate shown on the speed button (`1.0` by default). |
| `RateChanged` | `EventCallback<double>` | Raised with the new rate when the speed button is clicked. |
| `Rates` | `IReadOnlyList<double>?` | The rates the speed button cycles through. The button is hidden when there are fewer than two (0.5, 0.75, 1, 1.25, 1.5 and 2 by default). |
| `Seekable` | `bool` | Whether the user can move the position with the timeline. When `false` the timeline is a read-only progress bar (`true` by default). |
| `Src` | `string` | The URL of the media file. |
| `Start` | `double` | The clip start, in seconds. Positions and durations are reported relative to it. |
| `Title` | `string?` | The accessible name of the player region. |
| `Tracks` | `IReadOnlyList<VideoPlayerTrack>?` | The caption tracks, rendered as `track` elements. The CC button cycles an internal selection and does not change the browser track mode in this version. |
| `Volume` | `double` | The volume of the media element, from 0 to 1 (`1.0` by default). The player has no volume slider, so set it from your own control. |
| `VolumeChanged` | `EventCallback<double>` | Not raised in this version: the player has no volume slider. |

Also accepts the parameters of `KlyComponentBase`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `ClipEnd` | `double` | The clip end in seconds: `KlyVideoPlayer.End` when it is greater than `KlyVideoPlayer.Start`, otherwise `KlyVideoPlayer.Duration`. |
| `ClipLength` | `double` | The clip length in seconds, `KlyVideoPlayer.ClipEnd` minus `KlyVideoPlayer.Start`. |
| `ClipPosition` | `double` | The position inside the clip in seconds, between zero and `KlyVideoPlayer.ClipLength`. |
| `IsLocked` | `bool` | Whether the player is locked: `KlyVideoPlayer.MaxPlays` is set, no plays are left and no play is in progress. |
| `RemainingPlays` | `int` | The number of plays left, or `Int32.MaxValue` when `KlyVideoPlayer.MaxPlays` is zero. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `CompletePlayAsync()` | `Task` | Ends the current play: stops, moves the position to the clip end, and raises `KlyVideoPlayer.OnEnded` and, when no plays are left, `KlyVideoPlayer.OnLimitReached`. The player calls it when the media reaches the clip end. |
| `CycleRateAsync()` | `Task` | Selects the next entry of `KlyVideoPlayer.Rates` (wrapping around), and raises `KlyVideoPlayer.RateChanged`. It does nothing with fewer than two rates. |
| `SeekAsync(double clipOffsetSeconds)` | `Task` | Moves the position to an offset inside the clip, raises `KlyVideoPlayer.OnTimeUpdate`, and ends the play when the offset reaches the clip end. It does nothing unless `KlyVideoPlayer.Seekable`, and when `KlyVideoPlayer.Disabled` or `KlyVideoPlayer.IsLocked`. |
| `ToggleMuteAsync()` | `Task` | Toggles `KlyVideoPlayer.Muted` and raises `KlyVideoPlayer.MutedChanged`. |
| `TogglePlayAsync()` | `Task` | Starts, resumes or pauses the player, as the play button does. Starting a new play consumes one of `KlyVideoPlayer.MaxPlays`. It does nothing when `KlyVideoPlayer.Disabled` or `KlyVideoPlayer.IsLocked`. |

### Static classes and extensions

#### `KlyrooDisplayServiceCollectionExtensions`

`Klyroo.Display` · service

Service registration for Klyroo.Display.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static AddKlyrooDisplay(IServiceCollection services)` | `IServiceCollection` | Registers `FilePreviewService` as a scoped service and calls `AddKlyrooUi()`, which Klyroo.Display depends on. Call it once in `Program.cs`, and also in the client project of an Interactive WebAssembly or Auto app. |

### Classes

#### `DisplayAction`

`Klyroo.Display` · class

A button or link shown in a block, rendered as a small `KlyButton`.

| Constructor | Description |
| :--- | :--- |
| `DisplayAction()` | Creates an action. Set `DisplayAction.Label` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Color` (init) | `KlyColor` | The button color (neutral by default). |
| `Href` (init) | `string?` | The link target. When set, the button navigates instead of only raising `DisplayAction.OnClick`. |
| `Icon` (init) | `string?` | An icon name shown before the label. |
| `Label` (init) | `string` | The button text. |
| `OnClick` (init) | `EventCallback` | The callback invoked when the button is clicked. It only runs once the page is interactive. |
| `Target` (init) | `string?` | The link target window. The current renderer does not apply it. |
| `Variant` (init) | `ButtonVariant` | The button style (outline by default). |

#### `DisplayFact`

`Klyroo.Display` · class

One label and value row of a `DisplayKind.Facts` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayFact()` | Creates a row. Set `DisplayFact.Label` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Chip` (init) | `DisplayChip?` | A badge shown instead of the value. |
| `Copy` (init) | `bool` | Shows a copy button that copies `DisplayFact.CopyText`, or `DisplayFact.Value` when that is not set. |
| `CopyText` (init) | `string?` | The text copied by the copy button. Setting it also shows the button. |
| `Emphasis` (init) | `bool` | Renders the value in bold. |
| `Hint` (init) | `string?` | A hint. The current renderer does not show it. |
| `Href` (init) | `string?` | Renders the value as a link to this URL. |
| `Icon` (init) | `string?` | An icon name shown before the label. |
| `Label` (init) | `string` | The row label. |
| `Loading` (init) | `bool` | Shows a placeholder instead of the value. |
| `Mono` (init) | `bool` | Renders the value in a monospace font. |
| `Tone` (init) | `KlyColor?` | A tone for the row. The current renderer does not use it. |
| `Value` (init) | `string?` | The row value. An empty value with no `DisplayFact.Chip` renders a dash. |

#### `DisplayListItem`

`Klyroo.Display` · class

One item of a `DisplayKind.List` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayListItem()` | Creates an item. Set `DisplayListItem.Text` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Current` (init) | `bool` | Marks the item as the current step in the `steps` and `checks` variants. |
| `Description` (init) | `string?` | A line under the text. When `DisplayListItem.FileSize` is set, the formatted size is shown instead. |
| `Done` (init) | `bool` | Marks the item as done in the `steps` and `checks` variants. |
| `FileName` (init) | `string?` | The file name of a file item, used for the extension badge and the preview. It defaults to `DisplayListItem.Text`. |
| `FileSize` (init) | `long?` | The size of a file item in bytes. |
| `Href` (init) | `string?` | The URL of the item. In the `files` variant, a click opens the item in the file preview when a `KlyFilePreviewProvider` is mounted. |
| `Icon` (init) | `string?` | An icon name. The current renderer does not show it. |
| `Meta` (init) | `string?` | Text shown at the end of the row. |
| `MimeType` (init) | `string?` | The MIME type of a file item, used for the extension badge and to detect the preview kind. |
| `Text` (init) | `string` | The item text. It is a link when `DisplayListItem.Href` is set. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |

#### `DisplayRecordItem`

`Klyroo.Display` · class

One row of a `DisplayKind.Record` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayRecordItem()` | Creates a row. Set `DisplayRecordItem.Title` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `AvatarText` (init) | `string?` | Short text, such as initials, shown in the leading tile. |
| `Badges` (init) | `IReadOnlyList<DisplayChip>?` | Several badges. The current renderer does not show them: use `DisplayRecordItem.Chip`. |
| `Chip` (init) | `DisplayChip?` | A badge shown at the end of the row. |
| `Description` (init) | `string?` | The secondary line when `DisplayRecordItem.Meta` is not set. |
| `Href` (init) | `string?` | The URL the title links to. |
| `Icon` (init) | `string?` | An icon name shown in the leading tile. It takes precedence over `DisplayRecordItem.AvatarText`. |
| `ImageSrc` (init) | `string?` | An image for the leading tile. The current renderer does not show it. |
| `Meta` (init) | `string?` | The secondary line. It takes precedence over `DisplayRecordItem.Description`. |
| `MetaIcon` (init) | `string?` | An icon for the meta line. The current renderer does not show it. |
| `OnClick` (init) | `EventCallback` | A click callback. The current renderer does not invoke it. |
| `Reference` (init) | `string?` | A reference such as an order number, shown above the title in a monospace font. |
| `Title` (init) | `string` | The row title. It is a link when `DisplayRecordItem.Href` is set. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |
| `Trailing` (init) | `string?` | A value shown at the end of the row, such as an amount. |
| `TrailingCaption` (init) | `string?` | A caption under the trailing value. The current renderer does not show it. |

### Records

#### `DisplayAccordionItem`

`Klyroo.Display` · record

One section of a `DisplayKind.Accordion` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayAccordionItem(string Label, string? Content = null, string? Icon = null, bool DefaultOpen = false, RenderFragment? Template = null)` | One section of a `DisplayKind.Accordion` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Content` (init) | `string?` | The section text, used when `Template` is not set. |
| `DefaultOpen` (init) | `bool` | Whether the section starts open. |
| `Icon` (init) | `string?` | An icon name shown before the label. |
| `Label` (init) | `string` | The section header. |
| `Template` (init) | `RenderFragment?` | Markup for the section body. It takes precedence over `Content`. |

#### `DisplayBreakdownLine`

`Klyroo.Display` · record

One line of a `DisplayKind.Breakdown` block, also used for its total.

| Constructor | Description |
| :--- | :--- |
| `DisplayBreakdownLine(string Label, string? Value = null, string? CompareValue = null, string? Detail = null, bool IsSubtotal = false, bool IsMuted = false, KlyColor? Tone = null, DisplayChip? Chip = null)` | One line of a `DisplayKind.Breakdown` block, also used for its total. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Chip` (init) | `DisplayChip?` | A badge. The current renderer does not show it. |
| `CompareValue` (init) | `string?` | A second amount. The current renderer does not show it. |
| `Detail` (init) | `string?` | A detail text. The current renderer does not show it. |
| `IsMuted` (init) | `bool` | Renders the line in a dimmed color. |
| `IsSubtotal` (init) | `bool` | Renders the line as a subtotal, with a top border and bold text. |
| `Label` (init) | `string` | The line label. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |
| `Value` (init) | `string?` | The amount, already formatted. A missing value renders a dash. |

#### `DisplayChartPoint`

`Klyroo.Display` · record

One bar or step of a `DisplayKind.Chart` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayChartPoint(string Label, double Value, KlyColor? Tone = null)` | One bar or step of a `DisplayKind.Chart` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Label` (init) | `string` | The label under the bar. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |
| `Value` (init) | `double` | The value. Bar heights are relative to the largest value. |

#### `DisplayChip`

`Klyroo.Display` · record

A small badge shown in a block.

| Constructor | Description |
| :--- | :--- |
| `DisplayChip(string Label, KlyColor? Color = null, string? Icon = null)` | A small badge shown in a block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Color` (init) | `KlyColor?` | The badge color, or `null` for neutral. |
| `Icon` (init) | `string?` | An icon name. The current renderer does not show it. |
| `Label` (init) | `string` | The badge text. |

#### `DisplayContactItem`

`Klyroo.Display` · record

One contact line of a `DisplayKind.Person` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayContactItem(string Value, string? Icon = null, string? Href = null, bool Copy = true)` | One contact line of a `DisplayKind.Person` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Copy` (init) | `bool` | Whether to show a copy button. |
| `Href` (init) | `string?` | A link. The current renderer does not use it. |
| `Icon` (init) | `string?` | An icon name. The current renderer does not show it. |
| `Value` (init) | `string` | The text shown and copied, such as an email address. |

#### `DisplayDiffItem`

`Klyroo.Display` · record

One field of a `DisplayKind.Diff` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayDiffItem(string Label, string? Before, string? After, bool Changed = true, DisplayChip? Note = null)` | One field of a `DisplayKind.Diff` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `After` (init) | `string?` | The new value. |
| `Before` (init) | `string?` | The previous value, shown struck through. |
| `Changed` (init) | `bool` | Whether the field changed. Unchanged fields are hidden unless `KlyDisplay.ShowUnchanged` is set. |
| `Label` (init) | `string` | The field label. |
| `Note` (init) | `DisplayChip?` | A note. The current renderer does not show it. |

#### `DisplayFactGroup`

`Klyroo.Display` · record

A titled group of rows in a `DisplayKind.Facts` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayFactGroup(string? Title, IReadOnlyList<DisplayFact> Items, string? Aside = null)` | A titled group of rows in a `DisplayKind.Facts` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Aside` (init) | `string?` | Text shown opposite the title. |
| `Items` (init) | `IReadOnlyList<DisplayFact>` | The rows of the group. |
| `Title` (init) | `string?` | The group title. |

#### `DisplayMeterPart`

`Klyroo.Display` · record

One segment of a `DisplayKind.Meter` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayMeterPart(string Label, double Value, string? ValueLabel = null, KlyColor? Tone = null, string? Icon = null, double? Max = null)` | One segment of a `DisplayKind.Meter` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Icon` (init) | `string?` | An icon name. The current renderer does not show it. |
| `Label` (init) | `string` | The segment label, shown as a tooltip. |
| `Max` (init) | `double?` | A maximum for this segment. The current renderer does not use it. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |
| `Value` (init) | `double` | The segment size, in the same unit as `KlyDisplay.MeterMax`. |
| `ValueLabel` (init) | `string?` | A formatted value. The current renderer does not show it. |

#### `DisplayPersonItem`

`Klyroo.Display` · record

One person of a `DisplayKind.Person` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayPersonItem(string Name, string? Description = null, string? AvatarSrc = null, KlyColor? Tone = null, DisplayChip? Chip = null)` | One person of a `DisplayKind.Person` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `AvatarSrc` (init) | `string?` | An avatar image. The current renderer does not show it. |
| `Chip` (init) | `DisplayChip?` | A badge shown at the end of the row. |
| `Description` (init) | `string?` | A line under the name, such as a role. |
| `Name` (init) | `string` | The person's name. The avatar shows its initials. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |

#### `DisplayStatItem`

`Klyroo.Display` · record

One metric of the `row` variant of a `DisplayKind.Stat` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayStatItem(string Label, string Value, string? Caption = null, KlyColor? Tone = null)` | One metric of the `row` variant of a `DisplayKind.Stat` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Caption` (init) | `string?` | A line of text below the value. |
| `Label` (init) | `string` | The metric label. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |
| `Value` (init) | `string` | The metric value. |

#### `DisplayTableColumn`

`Klyroo.Display` · record

One column of a `DisplayKind.Table` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayTableColumn(string Key, string Label, bool Numeric = false, string? Align = null, bool Mono = false)` | One column of a `DisplayKind.Table` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Align` (init) | `string?` | Set to `"end"` to align the column to the end. |
| `Key` (init) | `string` | The key that selects this column's cell in each row. |
| `Label` (init) | `string` | The header text. |
| `Mono` (init) | `bool` | Renders the cells in a monospace font. |
| `Numeric` (init) | `bool` | Aligns the column to the end and uses tabular figures. |

#### `DisplayTimelineItem`

`Klyroo.Display` · record

One event of a `DisplayKind.Timeline` block.

| Constructor | Description |
| :--- | :--- |
| `DisplayTimelineItem(string Title, string? Description = null, string? Time = null, DateTimeOffset? Date = null, string? Icon = null, string? ActorName = null, DisplayTimelineState? State = null, KlyColor? Tone = null)` | One event of a `DisplayKind.Timeline` block. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `ActorName` (init) | `string?` | The name of the person behind the event. |
| `Date` (init) | `DateTimeOffset?` | A date. The current renderer does not use it. |
| `Description` (init) | `string?` | A line under the title. |
| `Icon` (init) | `string?` | An icon name. The current renderer does not show it. |
| `State` (init) | `DisplayTimelineState?` | The event state. When `null`, `DisplayTimelineState.Done` is used. |
| `Time` (init) | `string?` | The time text shown beside the title. It is displayed as given. |
| `Title` (init) | `string` | The event title. |
| `Tone` (init) | `KlyColor?` | A tone. The current renderer does not use it. |

### Enums

#### `DisplayKind`

`Klyroo.Display` · enum

The kind of block rendered by `KlyDisplay`. Each kind reads its own set of parameters and a few of them accept a `KlyDisplay.Variant`. The summaries list what the current renderer supports.

| Value | Description |
| :--- | :--- |
| `Heading` | A section heading with an optional icon, count pill, description and action. The `eyebrow` variant renders small uppercase text instead of a heading element. |
| `Text` | One or more paragraphs, or a single value. The `quote` variant adds a start border and a footer with the author and meta text. `KlyDisplay.MaxLines` clamps the value and adds a Read more toggle. |
| `Alert` | A message with an icon chosen from `KlyDisplay.Tone`, an optional collapsible details text and action buttons. |
| `Facts` | A list of label and value rows, flat from `KlyDisplay.Facts` or grouped from `KlyDisplay.FactGroups`. Rows can show a chip, a link, a copy button and a loading placeholder. |
| `Stat` | A highlighted value with a title, unit, delta badge, sparkline and caption. The `row` variant shows several small metrics side by side from `KlyDisplay.StatItems`. |
| `Breakdown` | A list of labelled amounts with subtotal rows and a total row. The `collapsible` variant hides the lines behind a toggle. |
| `Meter` | A horizontal meter driven by `KlyDisplay.MeterValue` and `KlyDisplay.MeterMax`, with warning and error thresholds, or stacked parts. The `scale` variant shows discrete levels instead. |
| `Chart` | A small bar chart from `KlyDisplay.ChartData`. The `funnel` variant draws one bar per step with the conversion rate from the previous step. |
| `Table` | A plain table of text cells, defined by columns and rows keyed by column, with an optional footer row. |
| `Diff` | Before and after values per field, with unchanged fields hidden unless `KlyDisplay.ShowUnchanged` is set. |
| `Record` | A list of entity rows, each with an avatar or icon, a reference, a title, a secondary line, a chip and a trailing value. |
| `Person` | A person with initials, name, description and chip and optional copyable contacts, or with the `group` variant a stack of initials with a +N overflow. |
| `List` | A list of items. The `steps` and `checks` variants number the items and mark done ones, and the `files` variant shows an extension badge and file size and can open the items in the file preview. |
| `Timeline` | A numbered list of events with a done, current or upcoming state. The `pipeline` variant lays the events out in a row. |
| `Date` | A calendar tile with a title. The `countdown` variant shows the days until or since the date, and the `range` variant shows a start and end date with the number of days. |
| `Media` | A single image, or with the `gallery` variant a grid of images with a +N overflow tile. |
| `Code` | A preformatted value with a label and a copy button. The `secret` variant masks the value and adds a Reveal toggle. |
| `Empty` | An empty state with an icon, a title, a description and action buttons. |
| `Accordion` | Collapsible sections, one open at a time unless `KlyDisplay.Multiple` is set. |

#### `DisplaySurface`

`Klyroo.Display` · enum

The surface treatment of a `KlyDisplay` block: border, background and padding.

| Value | Description |
| :--- | :--- |
| `Plain` | No border, background or padding. |
| `Muted` | A muted background with padding. |
| `Bordered` | A border and padding. |
| `Elevated` | A border, an elevated background, padding and a shadow. |

#### `DisplayTimelineState`

`Klyroo.Display` · enum

The state of an event of a `DisplayKind.Timeline` block.

| Value | Description |
| :--- | :--- |
| `Done` | The event happened. This is the state of an event whose state is not set. |
| `Current` | The event is happening now. |
| `Upcoming` | The event has not happened yet. |
