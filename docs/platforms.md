# Platforms

What runs where, what each environment needs, and the limitations of this version. The facts below come from the project files (`IsAotCompatible`, `IsTrimmable`, `SupportedPlatform`) and from the component code.

## Packages at a glance

| | `Klyroo.Display.Core` | `Klyroo.Display` |
| :--- | :--- | :--- |
| Target framework | `net10.0` | `net10.0` |
| Depends on | nothing | `Klyroo.Display.Core`, `Klyroo.UI` |
| `IsTrimmable` | yes | yes |
| `IsAotCompatible` | yes | not declared |
| Declared platforms | any | `browser`, `ios`, `android`, `maccatalyst`, `windows` |
| Static web assets | none | `klyroo-display.css` |

## Render modes

| Render mode | Blocks | Interactive features |
| :--- | :--- | :--- |
| Static server rendering | Render as markup. Links (`Href` on facts, records and list items) work. | Copy, Show more, Alert details, accordion, secret reveal, action callbacks, player controls and the file preview overlay do nothing, because they need event handlers. |
| Interactive Server | Everything works. | `FilePreviewService` is one instance per circuit. |
| Interactive WebAssembly | Everything works. | One instance for the app. Call `AddKlyrooDisplay()` in the client project. |
| Interactive Auto | Everything works. | Call `AddKlyrooDisplay()` in both projects. Each side has its own `FilePreviewService`, so an open preview does not carry from the server render to the client. |

Notes that apply to all modes:

- **Copy buttons** call `navigator.clipboard.writeText` through JS interop and do nothing before the page is interactive. Browsers expose the Clipboard API only in secure contexts, so over plain HTTP, other than on localhost, copying can fail. A JavaScript error from the call is swallowed.
- **The file preview provider and the code that opens a file must share a scope.** Put `<KlyFilePreviewProvider />` in a layout that renders interactively (for example with global interactivity on `<Routes />`), or inside the interactive page that opens files.
- **Package stylesheets** are loaded with ordinary `<link>` elements, in any mode, see [Getting started](getting-started.md#4-add-imports-and-stylesheets).
- **No JavaScript module of its own.** `Klyroo.Display` ships no script, and uses JS interop only for the clipboard.

## .NET MAUI Blazor Hybrid

`Klyroo.Display` declares `browser`, `ios`, `android`, `maccatalyst` and `windows` with `SupportedPlatform`, which tells the platform compatibility analyzer that a MAUI app targeting those platforms can use it. In a Blazor Hybrid app:

1. Call `builder.Services.AddKlyrooDisplay()` in `MauiProgram.cs`, after `AddMauiBlazorWebView()`.
2. Add the stylesheets to the `<head>` of `wwwroot/index.html`:

```html
<link rel="stylesheet" href="_content/Klyroo.UI/klyroo.preflight.css" />
<link rel="stylesheet" href="_content/Klyroo.UI/klyroo.css" />
<link rel="stylesheet" href="_content/Klyroo.Display/klyroo-display.css" />
```

A `BlazorWebView` renders interactively, so no extra render mode is needed. The repository has no MAUI sample project and the test suite runs on bUnit, so behavior on a device or emulator is not covered here, in particular PDF display in an `iframe` and the Clipboard API inside each platform's web view. Test those on your target platforms.

For a native MAUI UI with XAML and no `BlazorWebView`, install `Klyroo.Display.Core` only. It has no ASP.NET Core or Razor dependency, so a view model can use `FilePreviewService`, `ParseCsv` and the models directly.

## NativeAOT and trimming

`Klyroo.Display.Core` sets `IsAotCompatible` and `IsTrimmable`, which turns on the trim, AOT and single-file analyzers at build, and the repository builds with warnings as errors. The sample in `examples/FilePreviewCoreExample` was published with NativeAOT for `linux-x64` and runs:

```bash
dotnet publish examples/FilePreviewCoreExample -c Release -r linux-x64 -p:PublishAot=true
```

`Klyroo.Display` sets `IsTrimmable`, so it is safe to reference from a trimmed app, but it does not declare `IsAotCompatible` and its AOT behavior is not verified. Blazor WebAssembly ahead-of-time compilation of an app that uses it has not been tested here.

## Known limitations

These are limits of the current code, not configuration errors.

- **The media players do not play media.** `KlyAudioPlayer` and `KlyVideoPlayer` render a hidden or visible `audio` or `video` element and a control bar, and track the clip, the play count and the position in component state. They do not call `play()`, so pressing Play changes the button and counts a play but starts no sound, and the position does not advance on its own. `Autoplay`, `Compact` and `Volume` have no effect, and `OnTimeUpdate` is raised only by seeking.
- **A non-seekable video cannot end a play from code.** `SeekAsync` returns immediately when `Seekable` is false, and `KlyVideoPlayer` has no `CompletePlayAsync`. `KlyAudioPlayer` does have it.
- **The CC button of the video player does not change the active track.** The `track` elements are rendered, and the button only toggles its pressed state.
- **`MediaPlayerControl.PictureInPicture` and `Fullscreen` are reserved.** Neither player renders those buttons.
- **Some model properties are not rendered.** For example `DisplayChip.Icon`, `DisplayRecordItem.ImageSrc`, `DisplayRecordItem.Badges`, `DisplayPersonItem.AvatarSrc`, `DisplayTimelineItem.Date` and `Icon`, `DisplayBreakdownLine.CompareValue` and `Detail`, and `KlyDisplay.HeadingLevel` (a heading is always an `h3`). Each is marked as unused in its summary in the [API reference](api-reference.md).
- **Layouts that are not implemented**: there is no comparison layout for a breakdown, no line chart or ring meter, no sticky table header, no log or schedule timeline, and no syntax highlighting in the code block. Charts are bars and a funnel only.
- **The file preview has no renderer for Office files, archives and unknown files.** They show a download card, or their `Rendition` when you provide one.
- **No localization of the built-in texts.** Labels such as "Show more", "Read more", "No plays left" and the preview toolbar are English strings in the components.
