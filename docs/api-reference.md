# Klyroo.Display API reference

> Generated from the compiled assemblies and their XML documentation. Do not edit by hand.

## Klyroo.Display.Core

Static classes and extensions (1) · Classes (4) · Records (4) · Structs (2) · Enums (7)

### Static classes and extensions

#### `CodeHighlighter`

`Klyroo.Display` · service

A dependency-free syntax highlighter: it splits source code into `CodeToken` slices of a category, with hand-written scanners (no regular expressions, no reflection), so it is safe to trim and to compile ahead of time.

| Field | Type | Description |
| :--- | :--- | :--- |
| `const MaxLength = 100000` | `int` | The longest code, in characters, that is highlighted (`100000`). A longer code is returned as one plain token. |
| `const MaxLines = 5000` | `int` | The largest number of lines of a code that is highlighted (`5000`). A longer code is returned as one plain token. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Languages` | `IReadOnlyList<CodeLanguage>` | Every supported language, in alphabetical order of `CodeHighlighter.Name`. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static CanHighlight(string? code, CodeLanguage? language)` | `bool` | Whether `CodeHighlighter.Tokenize` would highlight this code: the language is set and the code is within the size caps. |
| `static Highlight(string code, CodeLanguage language)` | `string` | Highlights a code as HTML-safe markup, for a page that is not rendered by Blazor (an e-mail, a static site, a PDF). Text that has a category is wrapped in `<span data-token="keyword">` (`keyword`, `string`, `number`, `comment`, `type`, `function`, `property`, `tag` or `punctuation`), and plain text is bare. Every character of the code is escaped, so the result can be placed inside an element as it is. The spans carry no style: color them with your own CSS on `[data-token]`. |
| `static Name(CodeLanguage language)` | `string` | The canonical lowercase name of a language (`csharp`, `typescript`), as it is written in a `language-` class and a code block label. |
| `static ResolveFileName(string? fileNameOrUrl)` | `CodeLanguage?` | Maps a file name or URL to a `CodeLanguage`, from its extension. |
| `static ResolveLanguage(string? languageOrExtension)` | `CodeLanguage?` | Maps a language name, an alias or a file extension, which come from outside the program (a Markdown fence, a file name), to a `CodeLanguage`. |
| `static Tokenize(string? code, CodeLanguage? language)` | `IReadOnlyList<CodeToken>` | Splits source code into tokens. |
| `static TokenName(CodeTokenKind kind)` | `string` | The value of the `data-token` attribute of a token category (`keyword`, `string`...); `plain` for `CodeTokenKind.Plain`. |

### Classes

#### `FilePreviewAction`

`Klyroo.Display` · class

A custom button added to the header of the file preview.

| Constructor | Description |
| :--- | :--- |
| `FilePreviewAction()` | Creates an action. Set `FilePreviewAction.Label` before use. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Href` (init) | `string?` | A URL. When set, the action renders as a link to it instead of a button, and `FilePreviewAction.OnSelect`, when also set, still runs on click. |
| `Icon` (init) | `string?` | An icon name shown before the label, resolved through the icons of Klyroo.UI. |
| `Label` (init) | `string` | The button text. |
| `OnSelect` (init) | `Func<FilePreviewFile, Task>?` | The callback invoked with the file being previewed when the action is clicked. |

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
| `Id` (init) | `string?` | An optional stable identifier of the file. The provider uses it as the key of the file's thumbnail in the gallery strip, so the strip keeps its elements when a gallery is reordered; it must be unique within a gallery. |
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
| `VideoPlayerTrack(string Src, string SrcLang, string Label, VideoTrackKind Kind = Subtitles, bool Default = false)` | A subtitle or caption track of a video, rendered as a `track` element. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Default` (init) | `bool` | Whether the track is shown initially. With several default tracks, the first one wins. |
| `Kind` (init) | `VideoTrackKind` | The kind of the track, `VideoTrackKind.Subtitles` by default. |
| `Label` (init) | `string` | The label shown in the browser's track list. |
| `Src` (init) | `string` | The URL of the WebVTT file. |
| `SrcLang` (init) | `string` | The BCP 47 language code of the track, for example `"fr"`. |

### Structs

#### `CodeLineRange`

`Klyroo.Display` · struct

A run of lines of a code, counted from 1 and inclusive at both ends, such as the lines a code block emphasizes. A single line converts implicitly from an `Int32`, so `[3, new CodeLineRange(6, 9)]` is lines 3 and 6 to 9.

| Constructor | Description |
| :--- | :--- |
| `CodeLineRange(int Start, int End)` | A run of lines of a code, counted from 1 and inclusive at both ends, such as the lines a code block emphasizes. A single line converts implicitly from an `Int32`, so `[3, new CodeLineRange(6, 9)]` is lines 3 and 6 to 9. |
| `CodeLineRange(int line)` | Creates the range of one line. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `End` (init) | `int` | The last line, not before `Start`. |
| `Start` (init) | `int` | The first line, at least 1. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Contains(int line)` | `bool` | Whether a line (counted from 1) is in the range. A range whose end is before its start contains no line. |
| `static Of(int start, int end)` | `CodeLineRange` | Creates a range from its two ends. |

#### `CodeToken`

`Klyroo.Display` · struct

A slice of source code and its category. The texts of the tokens of a code, in order, concatenate to exactly that code.

| Constructor | Description |
| :--- | :--- |
| `CodeToken(CodeTokenKind Kind, string Text)` | A slice of source code and its category. The texts of the tokens of a code, in order, concatenate to exactly that code. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Kind` (init) | `CodeTokenKind` | The category of the slice. |
| `Text` (init) | `string` | The slice. It is never empty. |

### Enums

#### `CodeLanguage`

`Klyroo.Display` · enum

A language `CodeHighlighter` can highlight. A code in no language (`null` where a `CodeLanguage`? is taken) is plain text.

| Value | Description |
| :--- | :--- |
| `Bash` | Bash and other POSIX shells (`sh`, `zsh`, `shell`). |
| `CSharp` | C# (`cs`, `csx`). |
| `Css` | CSS. |
| `Html` | HTML and Razor (`htm`, `razor`, `cshtml`, `svg`): `script` and `style` contents are highlighted as JavaScript and CSS. |
| `JavaScript` | JavaScript (`js`, `jsx`, `mjs`, `cjs`). |
| `Json` | JSON, with comments (`jsonc`). |
| `Less` | Less. |
| `Python` | Python (`py`). |
| `Scss` | SCSS. |
| `Sql` | SQL. |
| `TypeScript` | TypeScript (`ts`, `tsx`, `mts`, `cts`). |
| `Xml` | XML and its dialects (`xaml`, `csproj`, `props`, `xsd`). |
| `Yaml` | YAML (`yml`). |

#### `CodeTokenKind`

`Klyroo.Display` · enum

The category of a `CodeToken`. A theme gives each category a color.

| Value | Description |
| :--- | :--- |
| `Plain` | Text with no special meaning: identifiers, whitespace, and everything in a language that is not highlighted. |
| `Keyword` | A reserved word, a literal such as `true` or `null`, a preprocessor directive, an at-rule or a document declaration. |
| `String` | A string or character literal, a regular expression literal, a heredoc body or an attribute value. |
| `Number` | A numeric literal, with its suffix or unit, and a hexadecimal color. |
| `Comment` | A line or block comment. |
| `Type` | The name of a type or class, or a class or identifier selector. |
| `Function` | The name of a function or method, at its call or declaration. |
| `Property` | A member, attribute, object key, decorator, CSS property or shell variable. |
| `Tag` | A markup tag name or a CSS element selector. |
| `Punctuation` | An operator or a punctuation mark, grouped in runs. |

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
| `Volume` | The mute button and the volume slider. |
| `Rate` | The playback speed button. |
| `Captions` | The captions button of the video player. |
| `PictureInPicture` | The picture-in-picture button of the video player. It shows only once the browser reports picture-in-picture support, and not when the player is not seekable, since the video element then disables picture-in-picture. |
| `Fullscreen` | The fullscreen button of the video player, which shows the whole player fullscreen. It shows only once the browser reports fullscreen support. |
| `Download` | The download link. |

#### `MediaPreload`

`Klyroo.Display` · enum

How much of a clip the browser loads before the user plays it.

| Value | Description |
| :--- | :--- |
| `None` | Nothing. |
| `Metadata` | The metadata only (the duration and the size of a video). |
| `Auto` | Whatever the browser decides is good, up to the whole file. |

#### `VideoTrackKind`

`Klyroo.Display` · enum

The `kind` of a `VideoPlayerTrack`.

| Value | Description |
| :--- | :--- |
| `Subtitles` | A translation of the dialogue. |
| `Captions` | A transcription of the dialogue and the sounds, for viewers who cannot hear. |
| `Descriptions` | A description of the video for viewers who cannot see it. |
| `Chapters` | The titles of the chapters. |
| `Metadata` | Data for scripts, not shown. |

## Klyroo.Display

Components (6) · Static classes and extensions (2) · Classes (1) · Records (5)

### Components

#### `KlyAudioPlayer`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A compact audio control bar with play and pause, a timeline, a mute button and a volume slider, a speed button and an optional download link, that can restrict playback to a clip (`KlyAudioPlayer.Start` and `KlyAudioPlayer.End`), to a number of plays (`KlyAudioPlayer.MaxPlays`) and to a non-seekable timeline.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Autoplay` | `bool` | Starts playing once the player is interactive. Browsers can refuse to start a player that is not muted before the user has interacted with the page; the player then stays paused. |
| `Compact` | `bool` | Renders a denser control bar: less padding and spacing, smaller buttons and a shorter volume slider. |
| `Disabled` | `bool` | Disables the play, seek, mute and volume controls, and the methods that drive them. |
| `Download` | `bool` | Shows the download link (`false` by default). The link also shows when `KlyAudioPlayer.DownloadUrl` is set. |
| `DownloadFileName` | `string?` | The file name suggested to the browser by the download link. When `null`, the last segment of the link URL is used, or the browser picks a name for a `data:` or `blob:` URL. |
| `DownloadUrl` | `string?` | The target of the download link. When `null`, `KlyAudioPlayer.Src` is used. |
| `Duration` | `double` | The total duration in seconds, used until the media element reports its own (`60` by default). Set `KlyAudioPlayer.End` to stop before the end of the file. |
| `End` | `double?` | The clip end, in seconds. It is used when it is greater than `KlyAudioPlayer.Start`, otherwise `KlyAudioPlayer.Duration` gives the end. |
| `Hide` | `IReadOnlyList<MediaPlayerControl>?` | The controls to remove from the control bar. `MediaPlayerControl.Volume` removes the mute button and the volume slider. `MediaPlayerControl.Captions`, `MediaPlayerControl.PictureInPicture` and `MediaPlayerControl.Fullscreen` are video controls that this player does not render. |
| `Keyboard` | `bool` | Whether the focused player reacts to keys: Space or K toggles playback, M toggles mute (`true` by default). Only keys pressed while the player itself has the focus count: a key on one of its buttons or sliders is left to that control. |
| `Loop` | `bool` | Sets `loop` on the media element. It is ignored when the player is restricted by `KlyAudioPlayer.MaxPlays` or by `KlyAudioPlayer.Seekable` being `false`. |
| `MaxPlays` | `int` | The number of times the clip can be started (`0`, the default, means unlimited). Once `KlyAudioPlayer.Plays` reaches it and the current play ends, the player locks and shows that no plays are left. |
| `Muted` | `bool` | Whether the player is muted. The mute button toggles it and raises `KlyAudioPlayer.MutedChanged`. |
| `MutedChanged` | `EventCallback<bool>` | Raised with the new value when the mute button is clicked. |
| `OnDownload` | `EventCallback` | Raised when the download link is clicked. |
| `OnEnded` | `EventCallback` | Raised when the position reaches the end of the clip. |
| `OnLimitReached` | `EventCallback` | Raised when no plays are left: when a play ends with `KlyAudioPlayer.Plays` at `KlyAudioPlayer.MaxPlays`, and each time a play is requested while the player is locked (from the keyboard, a media key or `KlyAudioPlayer.TogglePlayAsync`). |
| `OnPause` | `EventCallback` | Raised when the user pauses. |
| `OnPlay` | `EventCallback` | Raised when playback starts or resumes. |
| `OnTimeUpdate` | `EventCallback<MediaPlayerProgress>` | Raised with the position during playback, about four times a second, and when the user seeks. |
| `Plays` | `int` | The number of plays already used. The component increments it when a play starts and raises `KlyAudioPlayer.PlaysChanged`: keep the value in your own state and pass it back, so a re-render does not lose it. |
| `PlaysChanged` | `EventCallback<int>` | Raised with the new count when a play starts. |
| `Preload` | `MediaPreload` | How much of the clip the browser loads before the user plays it: `MediaPreload.Metadata` by default. |
| `Rate` | `double` | The playback rate shown on the speed button (`1.0` by default). |
| `RateChanged` | `EventCallback<double>` | Raised with the new rate when the speed button is clicked. |
| `Rates` | `IReadOnlyList<double>?` | The rates the speed button cycles through. The button is hidden when there are fewer than two (0.5, 0.75, 1, 1.25, 1.5 and 2 by default). |
| `Seekable` | `bool` | Whether the user can move the position with the timeline. When `false` the timeline is a read-only progress bar (`true` by default). |
| `Src` | `string` | The URL of the media file. |
| `Start` | `double` | The clip start, in seconds. Positions and durations are reported relative to it. |
| `Title` | `string?` | The accessible name of the player region. |
| `Volume` | `double` | The volume of the media element, from 0 to 1 (`1.0` by default). The volume slider changes it and raises `KlyAudioPlayer.VolumeChanged`. |
| `VolumeChanged` | `EventCallback<double>` | Raised with the new volume, from 0 to 1, when the volume slider moves or `KlyAudioPlayer.SetVolumeAsync` is called. |

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
| `SetVolumeAsync(double volume)` | `Task` | Sets `KlyAudioPlayer.Volume`, as the volume slider does, and raises `KlyAudioPlayer.VolumeChanged`. |
| `ToggleMuteAsync()` | `Task` | Toggles `KlyAudioPlayer.Muted` and raises `KlyAudioPlayer.MutedChanged`. It does nothing when `KlyAudioPlayer.Disabled`. |
| `TogglePlayAsync()` | `Task` | Starts, resumes or pauses the player, as the play button does. Starting a new play consumes one of `KlyAudioPlayer.MaxPlays`. It does nothing when `KlyAudioPlayer.Disabled`, and only raises `KlyAudioPlayer.OnLimitReached` when `KlyAudioPlayer.IsLocked`. |

#### `KlyCodeBlock`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A block of source code with a label, syntax highlighting from `CodeHighlighter`, a copy button, optional line numbers and emphasized lines. Show a value that must stay hidden with `KlySecretBlock` instead.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Code` | `string?` | The source code to show. It is plain text: markup in it is escaped. |
| `Copy` | `bool` | Whether the block shows a copy button, which copies the code as given (`true` by default). The button needs an interactive render mode. |
| `Highlight` | `bool` | Whether the code is highlighted according to `KlyCodeBlock.Language` (`true` by default). A code longer than `CodeHighlighter.MaxLength` characters or `CodeHighlighter.MaxLines` lines is always plain text. |
| `HighlightLines` | `IReadOnlyList<CodeLineRange>?` | The lines to emphasize, counted from 1: a neutral fill and an accent marker in the gutter. Single lines and runs mix in one list: `HighlightLines="@([3, new CodeLineRange(6, 9)])"`. Lines past the end of the code are ignored. |
| `Label` | `string?` | The label above the code. Without it, the name of `KlyCodeBlock.Language`, or the localized "Code". |
| `Language` | `CodeLanguage?` | The language of the code: it selects the syntax highlighting, and it is the label of the block when `KlyCodeBlock.Label` is not set. `null` (the default) shows plain text. Get it from a name, an alias or a file extension with `CodeHighlighter.ResolveLanguage` or `CodeHighlighter.ResolveFileName`. |
| `LineNumbers` | `bool` | Shows the number of each line in a gutter. The numbers are drawn by CSS, so they are not part of the text that is selected or copied. |
| `Ui` | `CodeBlockUi?` | Per-instance class overrides of the slots of the block, see `CodeBlockUi`. They are merged after the theme, and `Class` wins on the root slot. |

Also accepts the parameters of `KlyComponentBase`.

#### `KlyFileList`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A list of files, each with an extension badge, its name, its size and its date. Clicking a file opens the whole list as a gallery in the file preview, at that file, when a `KlyFilePreviewProvider` is mounted and the page is interactive. Otherwise the name is a plain link to the file.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Files` | `IReadOnlyList<FileListItem>?` | The files to list, in order. A `FilePreviewFile` converts to an item by itself. A row shows the badge (the extension, or the item `Icon`), the name (or the last segment of `Src`), the description and the size, and the item `Meta` or the date of the file. |
| `Max` | `int?` | The number of files shown before a Show more button (all files when `null`). |
| `Step` | `int?` | The number of files each click on Show more reveals (all remaining files when `null`). |
| `Title` | `string?` | A title above the list. |
| `Ui` | `FileListUi?` | Per-instance class overrides of the slots of the list, see `FileListUi`. They are merged after the theme, and `Class` wins on the root slot. |

Also accepts the parameters of `KlyComponentBase`.

#### `KlyFilePreviewProvider`

`Klyroo.Display` · component · inherits `KlyComponentBase`

Renders the file preview overlay that `FilePreviewService` opens: a dialog, drawer or fullscreen panel with a header (name, kind, size, zoom and rotate for images, custom actions, details, download, close), a stage that renders the file, a rendition notice, and a strip of thumbnails for galleries. Place one provider in your layout, once.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ChildContent` | `RenderFragment?` | Content rendered before the overlay. Use it to wrap your layout, or leave it empty and place the provider next to it. |
| `MarkdownRenderer` | `Func<string, string>?` | Converts the Markdown of a file to HTML. The result is rendered as markup without sanitizing, so use a renderer that escapes untrusted input. When `null`, Markdown files are shown as source. |

Also accepts the parameters of `KlyComponentBase`.

#### `KlySecretBlock`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A value that is masked until the user reveals it, such as an API key or a token, with a Reveal and Conceal toggle and a copy button. It is never highlighted.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Copy` | `bool` | Whether the block shows a copy button, which copies the whole secret even while it is masked (`true` by default). The Reveal and copy buttons need an interactive render mode. |
| `Label` | `string?` | The label above the value. Without it, the localized "Secret". |
| `Ui` | `SecretBlockUi?` | Per-instance class overrides of the slots of the block, see `SecretBlockUi`. They are merged after the theme, and `Class` wins on the root slot. |
| `Value` | `string?` | The secret, such as an API key or a token. While it is masked it is not written to the page at all, only its last `KlySecretBlock.VisibleChars` characters are. |
| `VisibleChars` | `int` | The number of trailing characters left visible while the value is masked (`4` by default). A longer value shows eight bullets followed by those characters. A value that is not longer than this number is fully masked, so a short secret is never shown whole. |

Also accepts the parameters of `KlyComponentBase`.

#### `KlyVideoPlayer`

`Klyroo.Display` · component · inherits `KlyComponentBase`

A video frame with an overlay play button and a control bar (play and pause, timeline, mute and volume, speed, captions, picture-in-picture, fullscreen and an optional download link), that can restrict playback to a clip (`KlyVideoPlayer.Start` and `KlyVideoPlayer.End`), to a number of plays (`KlyVideoPlayer.MaxPlays`) and to a non-seekable timeline.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `AspectRatio` | `string` | The CSS `aspect-ratio` of the player (`"16 / 9"` by default). |
| `Autoplay` | `bool` | Starts playing once the player is interactive. Browsers can refuse to start a player that is not muted before the user has interacted with the page; the player then stays paused. |
| `Compact` | `bool` | Renders a denser control bar: less padding and spacing, smaller buttons and a shorter volume slider. |
| `Disabled` | `bool` | Disables the play, seek, mute and volume controls, and the methods that drive them. |
| `Download` | `bool` | Shows the download link (`false` by default). The link also shows when `KlyVideoPlayer.DownloadUrl` is set. |
| `DownloadFileName` | `string?` | The file name suggested to the browser by the download link. When `null`, the last segment of the link URL is used, or the browser picks a name for a `data:` or `blob:` URL. |
| `DownloadUrl` | `string?` | The target of the download link. When `null`, `KlyVideoPlayer.Src` is used. |
| `Duration` | `double` | The total duration in seconds, used until the media element reports its own (`60` by default). Set `KlyVideoPlayer.End` to stop before the end of the file. |
| `End` | `double?` | The clip end, in seconds. It is used when it is greater than `KlyVideoPlayer.Start`, otherwise `KlyVideoPlayer.Duration` gives the end. |
| `Hide` | `IReadOnlyList<MediaPlayerControl>?` | The controls to remove from the control bar. `MediaPlayerControl.Volume` removes the mute button and the volume slider. |
| `Keyboard` | `bool` | Whether the focused player reacts to keys: Space or K toggles playback, M toggles mute (`true` by default). Only keys pressed while the player itself has the focus count: a key on one of its buttons or sliders is left to that control. |
| `Loop` | `bool` | Sets `loop` on the media element. It is ignored when the player is restricted by `KlyVideoPlayer.MaxPlays` or by `KlyVideoPlayer.Seekable` being `false`. |
| `MaxPlays` | `int` | The number of times the clip can be started (`0`, the default, means unlimited). Once `KlyVideoPlayer.Plays` reaches it and the current play ends, the player locks and shows that no plays are left. |
| `Muted` | `bool` | Whether the player is muted. The mute button toggles it and raises `KlyVideoPlayer.MutedChanged`. |
| `MutedChanged` | `EventCallback<bool>` | Raised with the new value when the mute button is clicked. |
| `OnDownload` | `EventCallback` | Raised when the download link is clicked. |
| `OnEnded` | `EventCallback` | Raised when the position reaches the end of the clip. |
| `OnLimitReached` | `EventCallback` | Raised when no plays are left: when a play ends with `KlyVideoPlayer.Plays` at `KlyVideoPlayer.MaxPlays`, and each time a play is requested while the player is locked (from the keyboard, a media key or `KlyVideoPlayer.TogglePlayAsync`). |
| `OnPause` | `EventCallback` | Raised when the user pauses. |
| `OnPlay` | `EventCallback` | Raised when playback starts or resumes. |
| `OnTimeUpdate` | `EventCallback<MediaPlayerProgress>` | Raised with the position during playback, about four times a second, and when the user seeks. |
| `Plays` | `int` | The number of plays already used. The component increments it when a play starts and raises `KlyVideoPlayer.PlaysChanged`: keep the value in your own state and pass it back, so a re-render does not lose it. |
| `PlaysChanged` | `EventCallback<int>` | Raised with the new count when a play starts. |
| `PlaysInline` | `bool` | Sets `playsinline` on the video element (`true` by default). |
| `Poster` | `string?` | The poster image shown before the video plays. |
| `Preload` | `MediaPreload` | How much of the clip the browser loads before the user plays it: `MediaPreload.Metadata` by default. |
| `Rate` | `double` | The playback rate shown on the speed button (`1.0` by default). |
| `RateChanged` | `EventCallback<double>` | Raised with the new rate when the speed button is clicked. |
| `Rates` | `IReadOnlyList<double>?` | The rates the speed button cycles through. The button is hidden when there are fewer than two (0.5, 0.75, 1, 1.25, 1.5 and 2 by default). |
| `Seekable` | `bool` | Whether the user can move the position with the timeline. When `false` the timeline is a read-only progress bar (`true` by default). |
| `Src` | `string` | The URL of the media file. |
| `Start` | `double` | The clip start, in seconds. Positions and durations are reported relative to it. |
| `Title` | `string?` | The accessible name of the player region. |
| `Tracks` | `IReadOnlyList<VideoPlayerTrack>?` | The caption tracks, rendered as `track` elements. The track marked `VideoPlayerTrack.Default` is shown first; the CC button then cycles through the tracks and off, showing the selected track and disabling the others. The choice is kept across renders and starts over when `KlyVideoPlayer.Src` changes. |
| `Volume` | `double` | The volume of the media element, from 0 to 1 (`1.0` by default). The volume slider changes it and raises `KlyVideoPlayer.VolumeChanged`. |
| `VolumeChanged` | `EventCallback<double>` | Raised with the new volume, from 0 to 1, when the volume slider moves or `KlyVideoPlayer.SetVolumeAsync` is called. |

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
| `SetVolumeAsync(double volume)` | `Task` | Sets `KlyVideoPlayer.Volume`, as the volume slider does, and raises `KlyVideoPlayer.VolumeChanged`. |
| `ToggleFullscreenAsync()` | `Task` | Shows the player fullscreen, or leaves fullscreen, as the fullscreen button does. The browser refuses it without a recent user gesture. |
| `ToggleMuteAsync()` | `Task` | Toggles `KlyVideoPlayer.Muted` and raises `KlyVideoPlayer.MutedChanged`. It does nothing when `KlyVideoPlayer.Disabled`. |
| `TogglePictureInPictureAsync()` | `Task` | Opens the video in the browser's picture-in-picture window, or closes that window, as the picture-in-picture button does. The browser refuses it without a recent user gesture, when it does not support picture-in-picture, and when `KlyVideoPlayer.Seekable` is `false` (the element then has `disablepictureinpicture`). |
| `TogglePlayAsync()` | `Task` | Starts, resumes or pauses the player, as the play button does. Starting a new play consumes one of `KlyVideoPlayer.MaxPlays`. It does nothing when `KlyVideoPlayer.Disabled`, and only raises `KlyVideoPlayer.OnLimitReached` when `KlyVideoPlayer.IsLocked`. |

### Static classes and extensions

#### `DisplayLocales`

`Klyroo.Display` · service

The built-in texts of Klyroo.Display, one set per locale of `KlyLocales.All` and keyed by the same codes. A component picks the set of its current `KlyLocale.Code` with `KlyLocales.ResolveCode`, so "pt-PT" gets "pt" and an unknown language gets English. Change a set with `KlyrooDisplayOptions.Texts`.

| Property | Type | Description |
| :--- | :--- | :--- |
| `static Ar` | `DisplayTexts` | العربية (ar). Needs review. |
| `static De` | `DisplayTexts` | Deutsch (de). Needs review. |
| `static En` | `DisplayTexts` | English. |
| `static Es` | `DisplayTexts` | Español (es). Needs review. |
| `static Fr` | `DisplayTexts` | Français (fr). Reviewed. |
| `static It` | `DisplayTexts` | Italiano (it). Needs review. |
| `static Ja` | `DisplayTexts` | 日本語 (ja). Needs review. |
| `static Nl` | `DisplayTexts` | Nederlands (nl). Needs review. |
| `static Pt` | `DisplayTexts` | Português (pt). Needs review. |
| `static PtBr` | `DisplayTexts` | Português (Brasil) (pt-BR). Needs review. |
| `static ReviewedCodes` | `IReadOnlyList<string>` | The codes whose texts were reviewed by a native speaker. The other sets are drafts waiting for review. |
| `static Ru` | `DisplayTexts` | Русский (ru). Needs review. |
| `static Tables` | `IReadOnlyDictionary<string, DisplayTexts>` | Every set by locale code: the codes of `KlyLocales.All`. |
| `static ZhCn` | `DisplayTexts` | 简体中文 (zh-Hans). Needs review. |

#### `KlyrooDisplayServiceCollectionExtensions`

`Klyroo.Display` · service

Service registration for Klyroo.Display.

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static AddKlyrooDisplay(IServiceCollection services, Action<KlyrooDisplayOptions>? configure = null)` | `IServiceCollection` | Registers `FilePreviewService` as a scoped service and calls `AddKlyrooUi()`, which Klyroo.Display depends on. Call it once in `Program.cs`, and also in the client project of an Interactive WebAssembly or Auto app. |

### Classes

#### `KlyrooDisplayOptions`

`Klyroo.Display` · class

Configuration of Klyroo.Display: the built-in texts of each locale. It is an extension of `KlyrooOptions` (`KlyrooOptions.Extension``). Configure it with `services.AddKlyrooDisplay(d => ...)`, in code shared by the server and the WebAssembly client.

| Constructor | Description |
| :--- | :--- |
| `KlyrooDisplayOptions()` | Creates the options with the built-in texts of `DisplayLocales`. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `CodeBlock` | `ComponentTheme` | The look of `KlyCodeBlock`: its slots (`root`, `pre`, `line`, the `token*` colors...) with their default classes. Add classes in `AddKlyrooDisplay`. |
| `FileList` | `ComponentTheme` | The look of `KlyFileList`. Add classes in `AddKlyrooDisplay`. |
| `SecretBlock` | `ComponentTheme` | The look of `KlySecretBlock`. Add classes in `AddKlyrooDisplay`. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `Texts(string localeCode, Func<DisplayTexts, DisplayTexts> change)` | `KlyrooDisplayOptions` | Changes the texts of a locale, like `KlyLocales.Extend` does for Klyroo.UI: `change` receives the set the locale resolves to and returns the one to use, usually `t => t with { ... }`. `localeCode` is a code of `KlyLocales.All` ("fr"), or the code of a locale you added ("en-AU", "tlh"): a locale without a set of its own starts from the set `KlyLocales.ResolveCode` picks for it, then its changes apply. The texts become read-only the first time a component resolves one. |

### Records

#### `CodeBlockUi`

`Klyroo.Display` · record

Per-instance class overrides of `KlyCodeBlock` slots.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Actions` (init) | `string?` | Classes merged into the `actions` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `Copy` (init) | `string?` | Classes merged into the `copy` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `CopyIcon` (init) | `string?` | Classes merged into the `copyIcon` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `Header` (init) | `string?` | Classes merged into the `header` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `Label` (init) | `string?` | Classes merged into the `label` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `Line` (init) | `string?` | Classes merged into the `line` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `LineHighlighted` (init) | `string?` | Classes merged into the `lineHighlighted` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `LineIdle` (init) | `string?` | Classes merged into the `lineIdle` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `LineNumbered` (init) | `string?` | Classes merged into the `lineNumbered` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `Pre` (init) | `string?` | Classes merged into the `pre` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `Root` (init) | `string?` | Classes merged into the `root` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenComment` (init) | `string?` | Classes merged into the `tokenComment` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenFunction` (init) | `string?` | Classes merged into the `tokenFunction` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenKeyword` (init) | `string?` | Classes merged into the `tokenKeyword` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenNumber` (init) | `string?` | Classes merged into the `tokenNumber` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenProperty` (init) | `string?` | Classes merged into the `tokenProperty` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenPunctuation` (init) | `string?` | Classes merged into the `tokenPunctuation` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenString` (init) | `string?` | Classes merged into the `tokenString` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenTag` (init) | `string?` | Classes merged into the `tokenTag` slot of `KlyCodeBlock`, after the theme and the app configuration. |
| `TokenType` (init) | `string?` | Classes merged into the `tokenType` slot of `KlyCodeBlock`, after the theme and the app configuration. |

#### `DisplayTexts`

`Klyroo.Display` · record

The built-in texts of `KlyCodeBlock`, `KlySecretBlock`, `KlyFileList`, `KlyAudioPlayer`, `KlyVideoPlayer` and `KlyFilePreviewProvider`: button labels, accessible names and counters. There is one set per locale of Klyroo.UI (`DisplayLocales`), and a component shows the set of its current locale, the one its `Messages` come from. Change a set with `KlyrooDisplayOptions.Texts` in `AddKlyrooDisplay(d => ...)`.

| Constructor | Description |
| :--- | :--- |
| `DisplayTexts()` | Creates the texts with their English defaults. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `AudioPlayer` (init) | `string` | The accessible name of an audio player without a title (`"Audio player"`). |
| `Captions` (init) | `string` | The captions button of the video player (`"Subtitles"`). |
| `ClosePreview` (init) | `string` | The close button of the preview (`"Close preview"`). |
| `Code` (init) | `string` | The label of a code block without a label or language (`"Code"`). |
| `Conceal` (init) | `string` | The button that masks a revealed secret block again (`"Conceal"`). |
| `ConvertedPreview` (init) | `Func<string, string>` | The notice above a rendition, given the kind of the original file (`"Converted preview · The original is a Office file."`). |
| `CopyCode` (init) | `string` | The accessible name of the copy button of a code block and of a secret block (`"Copy code"`). |
| `CsvSummary` (init) | `Func<int, int, string, string>` | The footer of a CSV table, given the number of rows, the number of columns and the delimiter (`"12 rows · 4 columns · delimiter ','"`, with `"tab"` for a tab). |
| `CsvTruncated` (init) | `Func<int, string>` | The notice of a CSV table cut to its first rows, given the number of rows shown (`"Showing first 1,000 rows"`). |
| `DetailName` (init) | `string` | The file name row of the details panel (`"Name"`). |
| `Details` (init) | `string` | The details button and panel title of the file preview (`"Details"`). |
| `DetailSize` (init) | `string` | The file size row of the details panel (`"Size"`). |
| `DetailType` (init) | `string` | The file type row of the details panel (`"Type"`). |
| `Download` (init) | `string` | The download link of the players and the file preview (`"Download"`). |
| `DownloadOriginal` (init) | `string` | The download link of the rendition notice (`"Download original"`). |
| `ExitFullscreen` (init) | `string` | The fullscreen button of the video player while it is fullscreen (`"Exit fullscreen"`). |
| `ExpandPreview` (init) | `string` | The button that expands a modal or drawer to the whole viewport (`"Expand preview"`). |
| `Fullscreen` (init) | `string` | The fullscreen button of the video player (`"Fullscreen"`). |
| `LineCount` (init) | `Func<int, string>` | The line count of a text file (`"1 line"`, `"12 lines"`). |
| `MarkdownPreview` (init) | `string` | The tab that shows rendered Markdown (`"Preview"`). |
| `MarkdownSource` (init) | `string` | The tab that shows Markdown source (`"Source"`). |
| `Mute` (init) | `string` | The mute button of an unmuted player (`"Mute"`). |
| `NextFile` (init) | `string` | The next file button of a gallery (`"Next file"`). |
| `NoPlaysLeft` (init) | `string` | The message of a locked player (`"No plays left"`). |
| `NoPreview` (init) | `string` | The title of the card shown for a file without a renderer (`"No preview available"`). |
| `NoPreviewHint` (init) | `string` | The text of the card shown for a file without a renderer (`"Download this file to open it in another application."`). |
| `Pause` (init) | `string` | The pause button (`"Pause"`). |
| `PictureInPicture` (init) | `string` | The picture-in-picture button of the video player (`"Picture in picture"`). |
| `Play` (init) | `string` | The play button (`"Play"`). |
| `PlaybackProgress` (init) | `string` | The read-only timeline of a non-seekable player (`"Playback progress"`). |
| `PlaybackSpeed` (init) | `string` | The playback speed button (`"Playback speed"`). |
| `PlaysLeft` (init) | `Func<int, string>` | The remaining plays of a player with a quota, given their number (`"2 plays left"`). |
| `PlayVideo` (init) | `string` | The large play button over a paused video (`"Play the video"`). |
| `PreviewPosition` (init) | `Func<int, int, string, string>` | The announcement of gallery navigation for screen readers, given the one-based position, the number of files and the file name (`"2 of 5: report.pdf"`). |
| `PreviousFile` (init) | `string` | The previous file button of a gallery (`"Previous file"`). |
| `ResetZoom` (init) | `string` | The button that resets the zoom of an image (`"Reset zoom"`). |
| `RestorePreview` (init) | `string` | The button that returns an expanded preview to its container (`"Restore preview size"`). |
| `Reveal` (init) | `string` | The button that unmasks a secret block (`"Reveal"`). |
| `Rotate` (init) | `string` | The rotate button of an image (`"Rotate 90 degrees"`). |
| `Secret` (init) | `string` | The label of a secret block without a label (`"Secret"`). |
| `Seek` (init) | `string` | The timeline slider (`"Seek"`). |
| `SeekValueText` (init) | `Func<string, string, string>` | The spoken value of the timeline slider and of the progress bar of a non-seekable player, given the position and the length of the clip (`"0:37 of 1:00"`). |
| `ShowMore` (init) | `Func<int, string>` | The Show more button of a file list, given the number of files the next click reveals (`"Show more · 3"`). |
| `Unmute` (init) | `string` | The mute button of a muted player (`"Unmute"`). |
| `VideoPlayer` (init) | `string` | The accessible name of a video player without a title (`"Video player"`). |
| `Volume` (init) | `string` | The volume slider (`"Volume"`). |
| `WrapLines` (init) | `string` | The toggle that wraps long lines of a text file (`"Wrap lines"`). |
| `ZoomIn` (init) | `string` | The zoom in button of an image (`"Zoom in"`). |
| `ZoomOut` (init) | `string` | The zoom out button of an image (`"Zoom out"`). |

#### `FileListItem`

`Klyroo.Display` · record

A file of a `KlyFileList`: the file the preview opens, and what the row shows besides it.

| Constructor | Description |
| :--- | :--- |
| `FileListItem(FilePreviewFile File, string? Description = null, string? Meta = null, string? Icon = null, KlyColor? Tone = null)` | A file of a `KlyFileList`: the file the preview opens, and what the row shows besides it. |

| Property | Type | Description |
| :--- | :--- | :--- |
| `Description` (init) | `string?` | A line under the name, before the size. |
| `File` (init) | `FilePreviewFile` | The file. Its name, size, MIME type and date feed the row, and the whole item list opens as a gallery in the file preview. |
| `Icon` (init) | `string?` | An icon name shown in the badge instead of the extension. |
| `Meta` (init) | `string?` | Text at the end of the row. Without it, the last-modified date of the file, when it has one. |
| `Tone` (init) | `KlyColor?` | A theme color for the badge, custom colors included. |

| Method | Returns | Description |
| :--- | :--- | :--- |
| `static FromFile(FilePreviewFile file)` | `FileListItem` | Lists a file with nothing but its own data. |

#### `FileListUi`

`Klyroo.Display` · record

Per-instance class overrides of `KlyFileList` slots.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Badge` (init) | `string?` | Classes merged into the `badge` slot of `KlyFileList`, after the theme and the app configuration. |
| `BadgeIcon` (init) | `string?` | Classes merged into the `badgeIcon` slot of `KlyFileList`, after the theme and the app configuration. |
| `BadgeIdle` (init) | `string?` | Classes merged into the `badgeIdle` slot of `KlyFileList`, after the theme and the app configuration. |
| `BadgeTone` (init) | `string?` | Classes merged into the `badgeTone` slot of `KlyFileList`, after the theme and the app configuration. |
| `Body` (init) | `string?` | Classes merged into the `body` slot of `KlyFileList`, after the theme and the app configuration. |
| `Description` (init) | `string?` | Classes merged into the `description` slot of `KlyFileList`, after the theme and the app configuration. |
| `Item` (init) | `string?` | Classes merged into the `item` slot of `KlyFileList`, after the theme and the app configuration. |
| `List` (init) | `string?` | Classes merged into the `list` slot of `KlyFileList`, after the theme and the app configuration. |
| `Main` (init) | `string?` | Classes merged into the `main` slot of `KlyFileList`, after the theme and the app configuration. |
| `Meta` (init) | `string?` | Classes merged into the `meta` slot of `KlyFileList`, after the theme and the app configuration. |
| `More` (init) | `string?` | Classes merged into the `more` slot of `KlyFileList`, after the theme and the app configuration. |
| `MoreIcon` (init) | `string?` | Classes merged into the `moreIcon` slot of `KlyFileList`, after the theme and the app configuration. |
| `Name` (init) | `string?` | Classes merged into the `name` slot of `KlyFileList`, after the theme and the app configuration. |
| `Root` (init) | `string?` | Classes merged into the `root` slot of `KlyFileList`, after the theme and the app configuration. |
| `Title` (init) | `string?` | Classes merged into the `title` slot of `KlyFileList`, after the theme and the app configuration. |

#### `SecretBlockUi`

`Klyroo.Display` · record

Per-instance class overrides of `KlySecretBlock` slots.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Actions` (init) | `string?` | Classes merged into the `actions` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `Copy` (init) | `string?` | Classes merged into the `copy` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `CopyIcon` (init) | `string?` | Classes merged into the `copyIcon` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `Header` (init) | `string?` | Classes merged into the `header` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `Label` (init) | `string?` | Classes merged into the `label` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `Pre` (init) | `string?` | Classes merged into the `pre` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `Root` (init) | `string?` | Classes merged into the `root` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `Toggle` (init) | `string?` | Classes merged into the `toggle` slot of `KlySecretBlock`, after the theme and the app configuration. |
| `ToggleIcon` (init) | `string?` | Classes merged into the `toggleIcon` slot of `KlySecretBlock`, after the theme and the app configuration. |
