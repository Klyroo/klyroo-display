# FAQ

Troubleshooting, known limitations and common questions. Each problem is explained from the code, so you can judge whether it applies to your case.

## Troubleshooting

### The block renders but has no styling

**Cause:** `klyroo-display.css` is not loaded. The blocks use Tailwind utility classes, and `klyroo-display.css` holds the compiled set for them.
**Fix:** add `<link rel="stylesheet" href="@Assets["_content/Klyroo.Display/klyroo-display.css"]" />` after the two Klyroo.UI stylesheets, see [Getting started](getting-started.md#4-add-imports-and-stylesheets).

### Copy, Show more, Reveal, accordion or action buttons do nothing

**Cause:** the page is rendered statically. These controls are event handlers, so they only work once the component is interactive. The copy buttons also return silently when the page is not interactive, and swallow a JavaScript error from the Clipboard API, which browsers expose only in secure contexts.
**Fix:** set an interactive render mode on the page or on `<Routes />`, and serve the site over HTTPS (localhost counts as secure).

### `FilePreviewService.Open` does nothing

**Cause:** there is no mounted `KlyFilePreviewProvider` that shares the service instance with your code, or the gallery you passed is empty (an empty list is ignored). With a per-page render mode, a provider in a static layout is a different instance from the one in your interactive page.
**Fix:** place `<KlyFilePreviewProvider />` once, in an interactive layout or in the interactive page, and check `FilePreviewService.IsMounted`.

### A CSV, Markdown or text preview is empty

**Cause:** these kinds render from `FilePreviewFile.Content`. The provider does not download `Src` for them.
**Fix:** load the text yourself and set `Content`, see [Guides](guides.md#preview-csv-markdown-and-text-that-you-already-hold).

### An Office document shows "No preview available"

**Cause:** the provider has no renderer for Office files, archives or unknown types.
**Fix:** for Office files, set `Rendition` to a PDF conversion. Otherwise the download card is the intended result.

### A file is detected as the wrong kind

**Cause:** the MIME type wins over the extension, so a wrong `Mime` overrides a correct name. A recognized MIME type is never second-guessed, while an unrecognized one such as `application/octet-stream` falls through to the extension.
**Fix:** correct `Mime`, or set `Kind` to skip detection.

### The PDF opens on the first page although `PdfPage` is set

**Cause:** `PdfPage` is appended as `#page=N&view=FitH` only when `Src` has no `#` fragment already. Also, page selection is up to the browser's PDF viewer, which may ignore it.
**Fix:** remove the fragment from `Src`, or put `#page=N` in it yourself.

### The play count resets, or the quota never runs out

**Cause:** `Plays` is a parameter. The player increments its own copy and reports it through `PlaysChanged`, and a parent re-render passes your old value back.
**Fix:** bind it: `@bind-Plays="plays"`.

### The audio or video player shows Play but nothing plays

**Cause:** the players do not drive the media element in this version, see [Known limitations](platforms.md#known-limitations).
**Fix:** none in the library yet. Treat the component as the rule layer and drive playback yourself.

### A parameter has no effect

**Cause:** `KlyDisplay` has one parameter list for 19 kinds, and a parameter is read only by the kinds its summary names. A misspelled name does not fail the build, because unmatched attributes are forwarded to the root element. A few model properties are not rendered at all.
**Fix:** check the parameter's summary in the [API reference](api-reference.md#klydisplay), and the kind in `Kind`.

### A Meter bar stays in the primary color

**Cause:** the thresholds apply to the single bar. When `MeterParts` is set, the bar is replaced by segments and the thresholds are ignored. Also `WarningThreshold` and `ErrorThreshold` are percentages of `MeterMax`, not raw values.
**Fix:** drop `MeterParts`, and express the thresholds as percentages.

### The countdown or timeline output changes between renders

**Cause:** the `countdown` variant of the Date block compares against the current UTC time unless you pass `Now`.
**Fix:** pass `Now` when the output must be stable, such as in a test or in cached markup.

## Known limitations

- The audio and video players need an interactive render mode, and a few of their parameters are unused. [Platforms](platforms.md#known-limitations) has the full list, which also covers unimplemented layouts and model properties that are not rendered.
- Text in the components is English only.
- Heading elements are always `h3`, and `HeadingLevel` has no effect.
- The file preview has no renderer for Office files, archives or unknown types, and relies on the browser for PDF.
- `Klyroo.Display` is not declared NativeAOT compatible. `Klyroo.Display.Core` is.

## Questions

### Can I use Klyroo.Display without Klyroo.UI?

Not the Blazor package: `Klyroo.Display` depends on Klyroo.UI for its theme tokens, `KlyColor`, `KlyButton` and `KlyBadge`. `Klyroo.Display.Core` has no such dependency.

### Does it need Tailwind in my app?

No. `klyroo-display.css` is a compiled stylesheet. If your app compiles Tailwind, follow the Klyroo.UI documentation for its theme import, see [Klyroo.UI](https://github.com/klyroo/klyroo-ui).

### Is `KlyDisplay` a replacement for a data grid or a chart library?

No. The table is a plain table of text cells, and the charts are small bars and a funnel without axes or tooltips. Use it for summaries next to richer components.

### Does the secret variant protect a secret?

No. It masks the value on screen. The real value is held by the component, and the copy button copies it, so only render secrets that the user is allowed to see.

### Is the Markdown output sanitized?

No. `MarkdownRenderer` returns HTML that the provider inserts as markup. Use a renderer that disables or sanitizes raw HTML for untrusted input.

### How do I localize the texts or the dates?

Dates in the `Date` block follow the culture Klyroo.UI resolves, and file sizes accept a `CultureInfo` in `FormatFileSize`. The component labels are not localizable in this version.
