---
name: klyroo-display
description: Klyroo rich content: a highlighted KlyCodeBlock, a masked KlySecretBlock, a KlyFileList with a multi-format FilePreview overlay, and restricted Audio/Video media players. The plain read-only blocks live in Klyroo.UI.
---

# Klyroo.Display Skill

- `KlyDisplay` and its `DisplayKind` blocks no longer exist. Alerts, statistics, tables, timelines, facts and the other read-only blocks are components of Klyroo.UI (`KlyAlert`, `KlyStat`, `KlyTable`, `KlyTimeline`, `KlyFacts`...). Do not generate `KlyDisplay` or `DisplayKind`.
- Use `KlyCodeBlock` (`Code`, `Language`, `Label`, `Highlight`, `Copy`, `LineNumbers`, `HighlightLines`, `Ui`) for code. `Language` is a typed `CodeLanguage?` (`Bash`, `CSharp`, `Css`, `Html`, `JavaScript`, `Json`, `Less`, `Python`, `Scss`, `Sql`, `TypeScript`, `Xml`, `Yaml`; `null` is plain text). Resolve a string from outside with `CodeHighlighter.ResolveLanguage(name)` (name, alias or extension) or `ResolveFileName(file)`, which return `CodeLanguage?`. `HighlightLines` is an `IReadOnlyList<CodeLineRange>`: `[2, new CodeLineRange(4, 5)]`, counted from 1.
- Use `KlySecretBlock` (`Value`, `Label`, `VisibleChars`, `Copy`) for a value that stays masked until Reveal. It is never highlighted and is not a security boundary.
- Use `KlyFileList` (`Files` as `IReadOnlyList<FilePreviewFile>`, `Title`, `Max`, `Step`) for attachments: a click opens the list as a gallery in the file preview when a `KlyFilePreviewProvider` is mounted and the page is interactive.
- The blocks follow the Klyroo.UI slot model: `AddKlyrooDisplay(d => d.CodeBlock.Slot("label", "..."))` for the app, `Ui="@(new CodeBlockUi { Pre = "..." })"` (or `SecretBlockUi`, `FileListUi`) for one instance.
- `CodeHighlighter` in `Klyroo.Display.Core` is headless: `Highlight(code, language)` returns escaped markup with `data-token` spans, `Tokenize(code, language)` returns `CodeToken` values.
- Use `KlyAudioPlayer` and `KlyVideoPlayer` for clips (`Start`/`End`), playback limits (`MaxPlays`/`Plays`), and non-seekable media. Hide controls with `Hide` (`MediaPlayerControl`).
- The built-in texts follow the Klyroo.UI locale (`DisplayLocales`, one `DisplayTexts` set per language); change one with `AddKlyrooDisplay(d => d.Texts("fr", t => t with { ... }))`.
- Use `FilePreviewService` and `KlyFilePreviewProvider` to preview PDF, images, video, audio, CSV, Markdown, JSON/code (highlighted), and Office renditions.
