# Concepts

This page explains how the pieces of Klyroo.Display fit together and why they are shaped the way they are. For task recipes see [Guides](guides.md), and for every member see the [API reference](api-reference.md).

## Two packages

```text
Klyroo.Display.Core                          Klyroo.Display
(no UI dependency, NativeAOT compatible)     (Blazor, builds on Klyroo.UI)

CodeHighlighter, CodeLanguage,               KlyCodeBlock             code, label, copy, line numbers
CodeToken, CodeLineRange   ─── tokens ─────►   │                      and emphasized lines
                                             KlySecretBlock           masked value, Reveal, copy
FilePreviewService   ─── Changed ──────────► KlyFilePreviewProvider   overlay for CurrentFile
  state: Files, CurrentIndex, Options          ▲
  Open / OpenGallery / Next / Previous /       │ injects the same instance
  GoTo / Close                                 │
  static: DetectKind, ParseCsv,              KlyFileList              opens its files in the service
          FormatFileSize, ...                KlyAudioPlayer, KlyVideoPlayer
FilePreviewFile, FilePreviewOptions,           clip, play-limit and seek rules
ParsedCsvPreview, VideoPlayerTrack, ...
```

`Klyroo.Display.Core` holds everything that can be expressed without a renderer: the syntax highlighter, the file and media models, the preview state, kind detection, the CSV parser and size formatting. Because nothing in it touches Blazor, you can unit test it with plain xUnit, call it from an API, and compile it with NativeAOT. `Klyroo.Display` adds the components and the service registration, and references Klyroo.UI for the theme tokens, the slot model, `KlyButton` and the icons.

The plain read-only blocks (alerts, statistics, tables, timelines and the like) are not here: they are components of Klyroo.UI. This package keeps what needs logic of its own, which is highlighting, masking, file preview and media rules.

## The code, secret and file list blocks

Each block is its own component with only the parameters it reads, so the compiler tells you when a parameter does not belong, and a misspelled name is caught by Razor instead of being forwarded to the root element as an unknown attribute.

**`KlyCodeBlock`** colors its `Code` according to a `CodeLanguage`. The scanning is done by `CodeHighlighter` in `Klyroo.Display.Core`, a hand-written, dependency-free scanner that splits the code into tokens (keyword, string, number, comment, type, function, property, tag, punctuation) whose texts concatenate to exactly the input, so selecting and copying behave as for plain text. `CodeLanguage` is an enum because the set of languages is closed. A name, an alias or a file extension that comes from outside the program goes through `CodeHighlighter.ResolveLanguage` or `ResolveFileName` once, and gives a `CodeLanguage?` where `null` means plain text. Each colored token is a `span` with a `data-token` attribute and a class from a `token*` slot of the theme, which defaults to a Klyroo.UI semantic text color (`text-(--klyroo-success-text)` for strings and so on), so light and dark mode and custom palettes apply without extra CSS. A missing language, `Highlight="false"` and a code longer than 100,000 characters or 5,000 lines all render plain text.

`LineNumbers` and `HighlightLines` (a list of `CodeLineRange`, counted from 1 and inclusive) make the block write one element per line instead of a flat run of tokens. A line keeps its line break inside its element, so the text content, the selection and the copy button still give the code exactly. The numbers are drawn by CSS from `data-line`, and an emphasized line has a neutral fill and an accent border on its start side. A token that spans several lines, such as a block comment, is cut at each line break and keeps its color on every line.

**`KlySecretBlock`** is the same frame with a masked value. While it is masked, the page holds the bullets and the last `VisibleChars` characters only, never the value. It is never highlighted, so a secret does not get colors that would hint at its shape, and its copy button copies the whole value.

**`KlyFileList`** takes `FilePreviewFile` values, the same type the preview service takes. A click calls `OpenGallery` on the injected `FilePreviewService` with the whole list and the clicked index, when a provider is mounted and the page is interactive. Without a mounted provider the list does not read the render mode at all, so it also renders in a test that has not set one.

The blocks share the theming model of Klyroo.UI. Each theme is a set of named slots with default classes (`KlyrooDisplayOptions.CodeBlock`, `SecretBlock`, `FileList`), the app adds classes in `AddKlyrooDisplay`, an instance adds classes through its `Ui` parameter, and `Class` goes to the root slot last. The component keeps small pieces of UI state itself: whether a secret is revealed and how many files are revealed. These reset when the component is recreated.

## The file preview flow

A file preview involves three parties: your code, `FilePreviewService` and `KlyFilePreviewProvider`.

1. Your code builds `FilePreviewFile` values and calls `Open` or `OpenGallery` on the injected service. The service copies the list, clamps the starting index, sets `IsOpen` and raises `Changed`.
2. The provider, subscribed to `Changed`, re-renders. For `CurrentFile` it detects the kind, picks the container and renders the matching stage.
3. Navigation (Previous, Next, GoTo, the thumbnail strip, arrow keys) goes back through the service, which updates `CurrentIndex` and raises `Changed` again. `Close` sets `IsOpen` to false.

**Detecting the kind.** `DetectKind` uses `FilePreviewFile.Kind` when set, then the MIME type, then the extension of `Name` or, without a name, of the URL. The first match wins and the result is one of ten `FilePreviewKind` values. Kinds without a built-in renderer (Office, Archive, Other) show a download card, and a `Rendition` lets you substitute a previewable file, usually a PDF, for an Office document while the original stays downloadable.

**Choosing the container.** `FilePreviewOptions.Mode` wins when set. Otherwise `ResolveContainer` takes the largest suggestion over the gallery: PDF, CSV, Markdown and text suggest a drawer, everything else a modal, so a gallery with one PDF opens as a drawer. The user can expand a modal or drawer to fullscreen.

**Content comes from you.** The provider renders images, video, audio and PDF straight from `Src`, so the browser does the loading. CSV, Markdown and text are rendered from `FilePreviewFile.Content` instead, which keeps the provider free of HTTP calls and of any decision about authentication or CORS: you fetch the text however your app does, then pass it in.

The service is registered as scoped. In Interactive Server that means one instance per circuit, in WebAssembly one for the app, and the provider and the code that calls `Open` must resolve the same instance, so they must render in the same interactive scope. `IsMounted` tells you whether a provider is currently subscribed.

## Media players: rules on top of the media element

`KlyAudioPlayer` and `KlyVideoPlayer` share one model, defined by a clip and a set of restrictions:

- **Clip**: `Start` and `End` bound the playable range. Without `End`, the clip runs to the end of the file, whose length the element reports once its metadata loads (`Duration` is the fallback until then). `ClipLength` and `ClipPosition` are relative to `Start`.
- **Play quota**: with `MaxPlays` above zero, starting a play consumes one, even when the browser then refuses to play (an autoplay policy, an unsupported format, a missing file): the quota counts what the user asked for, so a failure cannot be turned into an extra play. Pausing and resuming within a play does not. When the quota is spent and the last play ends, the player locks, shows that no plays are left, and raises `OnLimitReached`.
- **Seekability**: with `Seekable` false the timeline is a read-only progress bar and `SeekAsync` does nothing. The seek slider and the progress bar speak their position ("0:37 of 1:00", from `DisplayTexts.SeekValueText`) as `aria-valuetext`. A restricted player (a quota or no seeking) also ignores `Loop`.

`Plays` is an input parameter that the component also increments. It reports the new value through `PlaysChanged`, and you keep the number in your own state with `@bind-Plays`; otherwise a parent re-render passes the old value back and the quota resets.

**The rules are enforced in the browser only.** `Start`, `End`, `MaxPlays` and `Seekable` shape what the player lets the user do; they do not protect the file. The media URL is an ordinary `src`, so anyone can open it, download it, or play it past the clip in another tool, and `MaxPlays` lives in component state that a user resets by reloading the page unless you keep it yourself. Treat the rules as a courtesy of the interface. If the limit matters (a listening quota, a paid preview), count the plays on the server: save the number from `PlaysChanged` (it fires when a play starts), and refuse to render the player, or to serve the file through a signed or short-lived URL, once the quota is spent.

The players hold this state and drive their `audio` or `video` element through a small JavaScript module, `_content/Klyroo.Display/klyroo-display-media.js`, which the component imports once it is interactive. The buttons play, pause, seek, mute and change the speed of the element; the element reports its position (about four times a second, raised as `OnTimeUpdate`), its duration and its end. When playback reaches `End`, the module pauses the element and the player ends the play, which spends it. The rules stay in .NET: a play started with a hardware media key is counted like one started with the button, and refused when the player is locked.

## Render modes, threading and errors

**Render modes.** The blocks are server-renderable markup, so they appear in static server rendering. Everything that reacts to a click or a key (copy buttons, Show more, the secret Reveal toggle, the player controls and the preview overlay) needs an interactive render mode, and a file in a file list is a plain link until then. The copy buttons additionally need the browser Clipboard API, reached through JS interop. Details per mode are in [Platforms](platforms.md).

**Threading.** `FilePreviewService` is not synchronized. Call its methods from the thread that owns the UI, in Blazor from an event handler or inside `InvokeAsync`. The provider marshals its own re-render with `InvokeAsync`, so a `Changed` raised on another thread does not corrupt the render, but the service state itself is only safe for one caller at a time.

**Errors.** The library prefers to degrade over throwing. `Open` and `OpenGallery` throw `ArgumentNullException` for null arguments and do nothing for an empty gallery. `ParseCsv` accepts null and malformed text and returns what it could read, and `CodeHighlighter.Tokenize` never throws whatever the input. A JavaScript error from the clipboard call is swallowed, so a blocked clipboard does not break the page. The models validate nothing: a number such as a negative `Start` is clamped, not rejected.

## Design decisions and trade-offs

- **One component per block, with typed parameters** costs a few more components to learn than a single configurable one, but each has a short parameter list, an enum or a typed value where the set is closed (`CodeLanguage`, `MediaPreload`, `VideoTrackKind`), and no parameter that only another block reads.
- **The scanner is lexical, not a parser**, so it needs no grammar files, no regular expressions and no reflection, which keeps it small, safe to trim and compile ahead of time, and fast. The price is that a contextual keyword used as a name can be colored as a keyword.
- **Line elements only when asked for.** A flat run of tokens is the lightest markup and the simplest to select, so the block only splits into lines when numbers or emphasis need an element per line.
- **Display data as strings** (`Label`, the code itself) leaves formatting to your code, so the library never guesses a format. Dates in a file list use the culture resolved by Klyroo.UI.
- **The preview takes content instead of fetching it**, as described above. The trade-off is that you load CSV and text yourself.
- **A service plus a provider instead of a component per file** lets any code, including a file list deep in the tree, open a preview without owning the overlay. The cost is the shared-scope requirement.
- **Precompiled stylesheet**: `klyroo-display.css` is a compiled Tailwind build of the classes the components use, so apps without Tailwind need no build step. It is a second stylesheet to load next to the Klyroo.UI ones, and a class you add through a theme or `Ui` needs your own Tailwind build or CSS.
