# Concepts

This page explains how the pieces of Klyroo.Display fit together and why they are shaped the way they are. For task recipes see [Guides](guides.md), and for every member see the [API reference](api-reference.md).

## Two packages

```text
Klyroo.Display.Core                          Klyroo.Display
(no UI dependency, NativeAOT compatible)     (Blazor, builds on Klyroo.UI)

FilePreviewService   ─── Changed ──────────► KlyFilePreviewProvider   overlay for CurrentFile
  state: Files, CurrentIndex, Options          ▲
  Open / OpenGallery / Next / Previous /       │ injects the same instance
  GoTo / Close                                 │
  static: DetectKind, ParseCsv,              KlyDisplay               19 block kinds, one component
          FormatFileSize, ...                  │ Variant="files" opens items in the service
FilePreviewFile, FilePreviewOptions,         KlyAudioPlayer, KlyVideoPlayer
ParsedCsvPreview, VideoPlayerTrack, ...        clip, play-limit and seek rules
```

`Klyroo.Display.Core` holds everything that can be expressed without a renderer: the file and media models, the preview state, kind detection, the CSV parser and size formatting. Because nothing in it touches Blazor, you can unit test it with plain xUnit, call it from an API, and compile it with NativeAOT. `Klyroo.Display` adds the components and the service registration, and references Klyroo.UI for the theme tokens, `KlyColor`, `KlyButton` and `KlyBadge`.

## KlyDisplay: one component, many kinds

`KlyDisplay` is a single component whose `Kind` parameter selects one of 19 renderers. All kinds share one parameter list, so a parameter only has an effect on the kinds its documentation names: `Facts` is read by `DisplayKind.Facts`, `MeterValue` by `DisplayKind.Meter`, and so on. The [API reference](api-reference.md#klydisplay) says which kind reads each parameter.

A kind can have variants, chosen with the string `Variant`: `row` for a statistic, `pipeline` for a timeline, `secret` for a code value. Matching ignores case, and an unknown value renders the kind's default layout without an error. The item types (`DisplayFact`, `DisplayTimelineItem` and the others) are plain data: records or classes with init-only properties, so you build them in code and pass lists.

Three features are common to all kinds:

- **Surface**: `Surface` sets a border, background and padding. When it is not set, each kind has a default, for example bordered for facts and tables and plain for text.
- **Loading**: `Loading="true"` replaces the content with a skeleton, which you can use while the data loads.
- **Progressive reveal**: `Max` and `Step` hide items behind a Show more button for the list-like kinds.

The component keeps small pieces of UI state itself: whether Alert details are open, which accordion sections are open, whether a secret is revealed, how many items are revealed, whether a Text is expanded. These reset when the component is recreated, and parameters such as `DefaultOpen` are read once, the first time the component receives its parameters.

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
- **Play quota**: with `MaxPlays` above zero, starting a play consumes one. Pausing and resuming within a play does not. When the quota is spent and the last play ends, the player locks, shows that no plays are left, and raises `OnLimitReached`.
- **Seekability**: with `Seekable` false the timeline is a read-only progress bar and `SeekAsync` does nothing. A restricted player (a quota or no seeking) also ignores `Loop`.

`Plays` is an input parameter that the component also increments. It reports the new value through `PlaysChanged`, and you keep the number in your own state with `@bind-Plays`; otherwise a parent re-render passes the old value back and the quota resets.

The players hold this state and drive their `audio` or `video` element through a small JavaScript module, `_content/Klyroo.Display/klyroo-display-media.js`, which the component imports once it is interactive. The buttons play, pause, seek, mute and change the speed of the element; the element reports its position (about four times a second, raised as `OnTimeUpdate`), its duration and its end. When playback reaches `End`, the module pauses the element and the player ends the play, which spends it. The rules stay in .NET: a play started with a hardware media key is counted like one started with the button, and refused when the player is locked.

## Render modes, threading and errors

**Render modes.** The blocks are server-renderable markup, so they appear in static server rendering. Everything that reacts to a click or a key (copy buttons, Show more, Alert details, accordion, secret reveal, action callbacks, the player controls and the preview overlay) needs an interactive render mode. The copy buttons additionally need the browser Clipboard API, reached through JS interop. Details per mode are in [Platforms](platforms.md).

**Threading.** `FilePreviewService` is not synchronized. Call its methods from the thread that owns the UI, in Blazor from an event handler or inside `InvokeAsync`. The provider marshals its own re-render with `InvokeAsync`, so a `Changed` raised on another thread does not corrupt the render, but the service state itself is only safe for one caller at a time.

**Errors.** The library prefers to degrade over throwing. `Open` and `OpenGallery` throw `ArgumentNullException` for null arguments and do nothing for an empty gallery. `ParseCsv` accepts null and malformed text and returns what it could read. An unknown `Variant` renders the default layout. A JavaScript error from the clipboard call is swallowed, so a blocked clipboard does not break the page. The models validate nothing: a number such as a negative `Start` is clamped, not rejected.

## Design decisions and trade-offs

- **One component with a large parameter list** keeps the surface small and uniform, since adding a kind adds no new component to learn, but it means a parameter can be set on a kind that ignores it, and a misspelled parameter name does not fail the build: unmatched attributes are forwarded to the root element. Read the summary in the API reference when a parameter seems to do nothing.
- **String variants** avoid one enum per kind and let a variant be added without a breaking change, at the cost of no compile-time check. A typo falls back to the default layout.
- **Display data as strings** (`Value`, `Delta`, `DisplayFact.Value`) leaves formatting, currency and culture to your code, so the library never guesses a format. Dates in the `Date` kind use the culture resolved by Klyroo.UI.
- **The preview takes content instead of fetching it**, as described above. The trade-off is that you load CSV and text yourself.
- **A service plus a provider instead of a component per file** lets any code, including a list item deep in the tree, open a preview without owning the overlay. The cost is the shared-scope requirement.
- **Precompiled stylesheet**: `klyroo-display.css` is a compiled Tailwind build of the classes the blocks use, so apps without Tailwind need no build step. It is a second stylesheet to load next to the Klyroo.UI ones.
