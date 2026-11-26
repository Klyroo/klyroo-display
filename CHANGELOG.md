# Changelog

All notable changes to this project are documented in this file. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Syntax highlighting. `CodeHighlighter` in `Klyroo.Display.Core` (`Tokenize`, `ResolveLanguage`, `ResolveFileName`, `CanHighlight`, with `CodeToken` and `CodeTokenKind`) is a hand-written scanner without dependencies or regular expressions, safe for trimming and NativeAOT, for C#, JavaScript, TypeScript, JSON, HTML (Razor expressions, `script` and `style`), XML, CSS, SCSS, Less, SQL, Python, Bash and YAML. It never throws, its tokens concatenate to the input, and it leaves a code over 100,000 characters or 5,000 lines, or in another language, as plain text. The Code block of `KlyDisplay` highlights its `Language` (`Highlight` turns it off, the `secret` variant is never highlighted), and `KlyFilePreviewProvider` highlights text files from their extension. Each colored token is a `span` with `data-token` and a class that uses a Klyroo.UI semantic text color.
- `DisplayTexts`, the built-in texts of every component (labels, counters, accessible names), with one set per locale of Klyroo.UI in `DisplayLocales` (English and French reviewed, the ten others drafts). A component shows the set of its current Klyroo.UI locale. `KlyrooDisplayOptions.Texts(localeCode, change)` changes the set of a locale, and a locale added to Klyroo.UI starts from the set of the language it resolves to.
- `KlyAudioPlayer` and `KlyVideoPlayer` have a volume slider that raises `VolumeChanged` (hidden with `MediaPlayerControl.Volume`, like the mute button), `SetVolumeAsync`, and a denser bar with `Compact`.
- `KlyVideoPlayer` has picture-in-picture and fullscreen buttons (`MediaPlayerControl.PictureInPicture` and `Fullscreen`), shown once the browser reports support, with `TogglePictureInPictureAsync` and `ToggleFullscreenAsync`. Picture-in-picture stays hidden when `Seekable` is `false`.
- New `KlyDisplay` variants: `ring` for Meter, `line` for Chart, `sticky` for Table (a header that stays visible over rows scrolling in a box) and `log` for Timeline (a compact line per event, starting with its date and time).
- `KlyDisplay.HeadingLevel` chooses `h1` to `h6`, and `CompareColumns` with `DisplayBreakdownLine.CompareValue` gives a Breakdown a two-column comparison layout.
- Model members that were declared but not rendered now are: `Tone` on facts, stat items, breakdown lines, meter parts, chart points, records, people, list items and timeline events (as `data-color`, so theme and custom colors apply); `DisplayChip.Icon`; `DisplayFact.Hint`; `DisplayBreakdownLine.Detail` and `Chip`; `DisplayMeterPart.ValueLabel`, `Icon` and `Max` (with a legend); `DisplayDiffItem.Note`; `DisplayRecordItem.MetaIcon`, `ImageSrc`, `Badges`, `TrailingCaption` and `OnClick`; `DisplayPersonItem.AvatarSrc`; `DisplayContactItem.Icon` and `Href`; `DisplayListItem.Icon`; `DisplayTimelineItem.Date` and `Icon`; `DisplayAction.Target`; `FilePreviewAction.Icon` and `Href`; and `FilePreviewFile.Id`, which keys the gallery thumbnails.
- `MaxLines` clamps a Text block to that number of lines instead of always two.

### Changed

- `KlyDisplay.Language` now selects the syntax highlighting, as well as labelling a Code block. A Code block with a supported `Language` renders colored `span` elements inside `code`; set `Highlight="false"` to keep the previous plain output.
- `AddKlyrooDisplay` takes an optional `Action<KlyrooDisplayOptions>`. Code compiled against the previous signature must be recompiled.
- The player keys (Space, K, M) are handled by the media module on the player root, and only when the root itself has the focus. The players no longer have a Blazor `onkeydown` handler.
- The countdown text for a past date says "1 day overdue" in the singular, and the range length says "1 day".

### Fixed

- `Disabled` on the players now also disables the mute button and the volume slider, as its summary said.
- Space on a focused player button clicked it and also toggled playback, and M on the mute button muted twice.
- Turning captions off in `KlyVideoPlayer` was undone by the next render when a track was `Default`.
- `OnLimitReached` was not raised when a play was requested on a locked player, although its summary said so.
- With `Download` set and no `DownloadFileName`, the players rendered `download="True"`; the file name now comes from the URL, or the browser picks one for a `data:` or `blob:` URL.
- `KlyFilePreviewProvider` moves the focus into its dialog when it opens, so the keyboard shortcuts work at once, and a preview expanded to fullscreen no longer stays expanded for the next one.
- A `FilePreviewAction` with only `Href` rendered a button that did nothing; it renders a link.
- The text preview said "1 lines" for an empty file and never used the singular.
- The `row` Stat dropped `Title`, an Alert with an empty `Value` hid its `Description`, and countdown and range dates ignored `Title`.
- The `group` Person ignored each person's `Description` and `Chip`; they now appear in the avatar tooltip.
- A Stat with an unknown variant used the plain surface instead of the documented muted one.
- A Person block without `People` or `Title` showed the name "User"; it now shows no name and a person icon.

- `KlyAudioPlayer` and `KlyVideoPlayer` now play their media. A JavaScript module drives the `audio` or `video` element: the buttons play, pause, seek, mute and change the speed, the position and the duration come from the element, and playback stops at `End` and ends the play. `Autoplay` and `Volume` take effect, `OnTimeUpdate` is raised during playback, and the CC button of the video player switches the shown track.
- `KlyVideoPlayer` has `CompletePlayAsync`, like `KlyAudioPlayer`.
- Because the players now import `_content/Klyroo.Display/klyroo-display-media.js`, a bUnit test that renders one in strict JS interop mode needs `JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;`.
- Clicking a file in the `files` variant of a List opened the wrong file when an earlier item had no `Href`.

## [0.1.0-preview.1] - 2026-11-24

### Added

- `KlyDisplay`, one component that renders 19 kinds of read-only blocks (heading, text, alert, facts, stat, breakdown, meter, chart, table, diff, record, person, list, timeline, date, media, code, empty and accordion) from typed models, with variants, surfaces, a loading skeleton and a Show more reveal.
- A `secret` code block that masks all but the last characters of a value, with a Reveal toggle and a copy button.
- `FilePreviewService` and `KlyFilePreviewProvider`: a file preview overlay for PDF, images, video, audio, CSV, Markdown and text, in a modal, drawer or fullscreen container, with galleries, keyboard navigation, image zoom and rotation, a details panel, custom actions, downloads and renditions of files the browser cannot show.
- `Klyroo.Display.Core`, a UI-free package with file kind detection from MIME type and extension, a CSV and TSV parser with RFC 4180 quoting and delimiter detection, and file size formatting. It is NativeAOT compatible and trimmable.
- `KlyAudioPlayer` and `KlyVideoPlayer` control bars that model a clip window, a play quota, a non-seekable timeline, playback rates, captions and downloads, and report them through callbacks.
- Support for .NET 10, Interactive Server, WebAssembly, Auto and static server rendering, and declared support for the `browser`, `ios`, `android`, `maccatalyst` and `windows` platforms of .NET MAUI Blazor Hybrid.

[Unreleased]: https://github.com/klyroo/klyroo-display/compare/v0.1.0-preview.1...HEAD
[0.1.0-preview.1]: https://github.com/klyroo/klyroo-display/releases/tag/v0.1.0-preview.1
