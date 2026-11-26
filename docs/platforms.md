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
- **One JavaScript module, for the players.** The audio and video players import `_content/Klyroo.Display/klyroo-display-media.js` once interactive. It drives the media element and reports back the position, the end of the clip, the player keys, and picture-in-picture and fullscreen support. The blocks use JS interop only for the clipboard, and the preview provider only to move the focus into its dialog.

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

- **The media players need an interactive render mode.** They drive their element from JavaScript once interactive, so under static server rendering the control bar shows but does not play.
- **`Autoplay` depends on the browser.** Most browsers refuse to start a player that is not muted before the user has interacted with the page; the player then stays paused.
- **Picture-in-picture and fullscreen depend on the browser.** The video player shows those buttons only after the media module reports that the browser supports them, and hides picture-in-picture when `Seekable` is `false`, because the element disables it then. Browsers accept these requests only shortly after a user gesture, and under Interactive Server the click makes a round trip to the server first, so a slow connection can make the browser refuse it. The audio player has neither button.
- **Layouts that are not implemented**: there is no schedule timeline. Charts are bars, a line and a funnel, without axes.
- **Syntax highlighting is lexical.** `CodeHighlighter` recognizes comments, strings, numbers, keywords and a few naming heuristics (a name followed by `(` is a function, a capitalized name is a type); it does not parse, so a contextual keyword used as a name can be colored as a keyword. It covers C#, JavaScript, TypeScript, JSON, HTML (with Razor expressions, and `script` and `style` contents), XML, CSS, SCSS, Less, SQL, Python, Bash and YAML. JSX, indented Sass, TOML, INI and shell and SQL dialect details are not highlighted. A code over 100,000 characters or 5,000 lines is shown without colors, so a large file cannot slow the rendering.
- **The file preview has no renderer for Office files, archives and unknown files.** They show a download card, or their `Rendition` when you provide one.
- **Built-in texts per locale.** Labels such as "Show more", "No plays left" and the preview toolbar come from the `DisplayTexts` set of the current Klyroo.UI locale, on every platform. Change a set in `AddKlyrooDisplay(d => d.Texts("fr", t => t with { ... }))`.
