# Changelog

All notable changes to this project are documented in this file. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- `KlyAudioPlayer` and `KlyVideoPlayer` now play their media. A JavaScript module drives the `audio` or `video` element: the buttons play, pause, seek, mute and change the speed, the position and the duration come from the element, and playback stops at `End` and ends the play. `Autoplay` and `Volume` take effect, `OnTimeUpdate` is raised during playback, and the CC button of the video player switches the shown track.
- `KlyVideoPlayer` has `CompletePlayAsync`, like `KlyAudioPlayer`.
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
