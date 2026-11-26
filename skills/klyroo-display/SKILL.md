---
name: klyroo-display
description: Klyroo 19-kind read-only Display blocks, restricted Audio/Video media players, and multi-format FilePreview overlay.
---

# Klyroo.Display Skill

- Use `KlyDisplay` with `DisplayKind` (`Heading`, `Text`, `Alert`, `Facts`, `Stat`, `Breakdown`, `Meter`, `Chart`, `Table`, `Diff`, `Record`, `Person`, `List`, `Timeline`, `Date`, `Media`, `Code`, `Empty`, `Accordion`).
- Pick a layout with `Variant` (for example `ring` meter, `line` chart, `sticky` table, `log` timeline) and color items with their `Tone`.
- Use `KlyAudioPlayer` and `KlyVideoPlayer` for clips (`Start`/`End`), playback limits (`MaxPlays`/`Plays`), and non-seekable media. Hide controls with `Hide` (`MediaPlayerControl`).
- Replace the built-in English texts with `AddKlyrooDisplay(texts => ...)` (`DisplayTexts`).
- Use `FilePreviewService` and `KlyFilePreviewProvider` to preview PDF, images, video, audio, CSV, Markdown, JSON/code, and Office renditions.
