# Klyroo.Display.Maui

Native .NET MAUI controls (no WebView, no Blazor) for [Klyroo.Display](https://github.com/klyroo/klyroo-display): the code and secret blocks, the file list, the audio and video players and the file preview. They are drawn with the design tokens of `Klyroo.UI.Maui` (light and dark follow the app theme) over the headless engines of `Klyroo.Display.Core`: the highlighter, the file kind detection, the CSV parser, the preview state and the media models.

Everything is in the namespace `Klyroo.Display.Maui`, with the same type names as the Blazor package (`KlyCodeBlock`), so an app that references both writes the namespace it means.

## Controls

| Control | Built on | Notes |
| :--- | :--- | :--- |
| `KlyCodeBlock` | `Border` + `Label` | `Code`, `Language`, `Label`, `Highlight`, `Copy`, `LineNumbers`, `HighlightLines`. A monospace label with a span per token of `CodeHighlighter`, in the token colors of the web block; a copy button writes the code to the clipboard. Scrolls sideways. |
| `KlySecretBlock` | `Border` + `Label` | `Value`, `Label`, `VisibleChars`, `Copy`, and `IsRevealed` (two-way). Masked until revealed; the whole value is never held by the control while masked; copy copies all of it. |
| `KlyFileList` | `Border` | `Files` (`FileListItem`), `Title`, `Max`, `Step`, `Preview`. Rows with an extension badge or an icon, the name, the description, the size and the date; Show more. A tap raises `FileSelected` and, when `Preview` is set, opens the list as a gallery. |
| `KlyFilePreviewProvider` | `ContentView` + `KlyModal` / `KlySlideover` | Shows what a `FilePreviewService` opens: image (zoom, rotate), video, audio, CSV, Markdown, highlighted text; gallery strip, details, rendition notice, custom actions. Needs a `KlyOverlayHost` above it. |
| `KlyAudioPlayer`, `KlyVideoPlayer` | `Border` | The transport (play and pause, timeline, time, plays left, mute and volume, speed, captions, download) with the clip, plays and lock rules of Klyroo.Display. They play through an `IMediaPlayback`. |

## Playing media

MAUI has no media player control, and this package adds no NuGet dependency, so the players draw the transport and leave the sound and the picture to an `IMediaPlayback` that the app provides:

```csharp
player.Playback = new MyPlayback(mediaElement); // wraps the media player of your app
```

`IMediaPlayback` is small: `OpenAsync`, `PlayAsync`, `PauseAsync`, `SeekAsync`, `Apply(MediaPlaybackOptions)`, the events `TimeChanged`, `DurationChanged`, `Ended`, `PlayingChanged`, `PlayFailed`, a `Surface` view for the picture of a video, and `Capabilities`, so a playback that cannot seek or set the speed hides those controls.

Without a playback, a player uses `LauncherMediaPlayback`: the play button opens the file with the platform (`Launcher`), the browser or the media app of the device plays it, and the bar shows the play button, the plays left and the download button only. That counts as one play for `MaxPlays`.

## Differences from the Blazor controls

- Texts come from `DisplayTexts.Current` (English by default; replace it at startup to translate). The Blazor locale sets are not ported.
- No `Ui` slot overrides, no keyboard shortcuts, no picture-in-picture or fullscreen buttons, no download file name: they are web concepts.
- `EventCallback` parameters are events (`Played`, `Paused`, `Ended`, `LimitReached`, `TimeUpdated`, `DownloadClicked`, `PlaysChanged`...); `Plays`, `Rate`, `Volume` and `Muted` are two-way bindable.
- The preview has no PDF renderer: a PDF, an Office file, an archive or any other file offers to open the file with the platform. `MarkdownRenderer` returns a view, not HTML. A CSV file shows its first 200 rows by default (`CsvMaxRows`), because every cell is a native view. The expand button and the keyboard shortcuts are not ported; the fullscreen container uses a large modal.
- `KlyFileList.FileSelected` is the tap event; setting `Preview` restores the Blazor behavior of opening the gallery.

## Build

Not part of `Klyroo.Display.slnx`: it needs the MAUI workload and the sibling checkout `klyroo-ui` (`..\..\..\klyroo-ui`). It targets `net10.0` (the unit tests run on it) and `net10.0-android`, plus iOS and Mac Catalyst outside Linux and Windows on Windows.

```bash
dotnet build src/Klyroo.Display.Maui -f net10.0-android
dotnet test --project tests/Klyroo.Display.Maui.Tests
```
