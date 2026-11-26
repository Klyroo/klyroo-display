---
name: klyroo-display
description: Klyroo 19-kind read-only Display blocks, restricted Audio/Video media players, and multi-format FilePreview overlay.
---

# Klyroo.Display Skill

- Use `KlyDisplay` with `DisplayKind` (`Heading`, `Text`, `Alert`, `Facts`, `Stat`, `Breakdown`, `Meter`, `Chart`, `Table`, `Diff`, `Record`, `Person`, `List`, `Timeline`, `Date`, `Media`, `Code`, `Empty`, `Accordion`).
- Pick a layout with `Variant` (for example `ring` meter, `line` chart, `sticky` table, `log` timeline) and color items with their `Tone`.
- The Code block highlights its `Language` (C#, JavaScript, TypeScript, JSON, HTML, XML, CSS, SCSS, Less, SQL, Python, Bash, YAML; aliases and extensions accepted) with `Highlight` on by default; the `secret` variant is never highlighted. The file preview highlights text files by extension. `CodeHighlighter.Tokenize(code, language)` in `Klyroo.Display.Core` is the headless scanner.
- Use `KlyAudioPlayer` and `KlyVideoPlayer` for clips (`Start`/`End`), playback limits (`MaxPlays`/`Plays`), and non-seekable media. Hide controls with `Hide` (`MediaPlayerControl`).
- The built-in texts follow the Klyroo.UI locale (`DisplayLocales`, one `DisplayTexts` set per language); change one with `AddKlyrooDisplay(d => d.Texts("fr", t => t with { ... }))`.
- Use `FilePreviewService` and `KlyFilePreviewProvider` to preview PDF, images, video, audio, CSV, Markdown, JSON/code (highlighted), and Office renditions.
