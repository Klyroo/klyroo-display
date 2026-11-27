# FAQ

Troubleshooting, known limitations and common questions. Each problem is explained from the code, so you can judge whether it applies to your case.

## Troubleshooting

### A component renders but has no styling

**Cause:** `klyroo-display.css` is not loaded. The components use Tailwind utility classes, and `klyroo-display.css` holds the compiled set for them.
**Fix:** add `<link rel="stylesheet" href="@Assets["_content/Klyroo.Display/klyroo-display.css"]" />` after the two Klyroo.UI stylesheets, see [Getting started](getting-started.md#4-add-imports-and-stylesheets).

### Copy, Show more or Reveal do nothing

**Cause:** the page is rendered statically. These controls are event handlers, so they only work once the component is interactive. The copy buttons also return silently when the page is not interactive, and swallow a JavaScript error from the Clipboard API, which browsers expose only in secure contexts. In a file list, a click only follows the link until the page is interactive and a provider is mounted.
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

**Cause:** the page is not interactive, or the browser refused to play. The players drive their element from `klyroo-display-media.js` once the page is interactive, so under static server rendering the button does nothing. When the browser refuses a play (an autoplay policy, an unsupported format, a missing file), the button goes back to Play, and with `MaxPlays` that play still counts.
**Fix:** render the page or the player in an interactive render mode, and check the browser console and network tab for the media request.

### A code line is not emphasized, or the numbers are missing

**Cause:** `HighlightLines` counts lines from 1 and an `int` is one line, so `[0]` matches nothing, and a line past the end of the code is ignored. Line numbers need `LineNumbers`, and both options only apply to the text that is rendered, so check that `Code` has the line breaks you expect (a `\n` in the C# string, not a literal backslash).
**Fix:** count from 1 and use `new CodeLineRange(first, last)` for a run.

### A new class I added to a slot has no effect

**Cause:** `klyroo-display.css` is a compiled set that only contains the classes the library itself uses. A utility you add through `AddKlyrooDisplay` or a `Ui` parameter is in the markup but not in that file.
**Fix:** use a class the app already compiles, compile your own Tailwind (see the Klyroo.UI documentation), or write a CSS rule for it.

## Known limitations

- The audio and video players need an interactive render mode, and picture-in-picture and fullscreen depend on the browser. [Platforms](platforms.md#known-limitations) has the full list.
- The built-in texts follow the locale of Klyroo.UI, but only the English and French sets are reviewed by a native speaker; the others are drafts you can correct with `KlyrooDisplayOptions.Texts`.
- The highlighter is lexical and covers thirteen languages, see [Platforms](platforms.md#known-limitations).
- The file preview has no renderer for Office files, archives or unknown types, and relies on the browser for PDF.
- `Klyroo.Display` is not declared NativeAOT compatible. `Klyroo.Display.Core` is.

## Questions

### Can I use Klyroo.Display without Klyroo.UI?

Not the Blazor package: `Klyroo.Display` depends on Klyroo.UI for its theme tokens, the slot model, `KlyButton` and the icons. `Klyroo.Display.Core` has no such dependency.

### Does it need Tailwind in my app?

No. `klyroo-display.css` is a compiled stylesheet. If your app compiles Tailwind, follow the Klyroo.UI documentation for its theme import, see [Klyroo.UI](https://github.com/klyroo/klyroo-ui).

### Where did KlyDisplay and its blocks go?

`KlyDisplay` was removed: the plain read-only blocks are distinct typed components of Klyroo.UI now. The [changelog](../CHANGELOG.md) maps each former kind to its component, and `KlyCodeBlock` (the Code kind), `KlySecretBlock` (the secret variant of Code) and `KlyFileList` (the files variant of List) stay in this package.

### Why is my code not colored?

`KlyCodeBlock` colors a code only when `Language` is set to a `CodeLanguage`, `Highlight` is not `false`, and the code is within 100,000 characters and 5,000 lines. Otherwise it renders as plain text. A language that comes from a string (`"cs"`, `"language-ts"`, a file name) goes through `CodeHighlighter.ResolveLanguage` or `ResolveFileName` first, and gives `null`, which is plain text, when it is not supported. The file preview colors a text file from its extension, so a `Name` or `Src` without a supported extension (`Dockerfile`, `.txt`) stays plain. If colors are missing for a supported language, check that `klyroo-display.css` is loaded, see [A component renders but has no styling](#a-component-renders-but-has-no-styling).

### Does KlySecretBlock protect a secret?

No. It masks the value on screen. The real value is held by the component, and the copy button copies it, so only render secrets that the user is allowed to see.

### Is the Markdown output sanitized?

No. `MarkdownRenderer` returns HTML that the provider inserts as markup. Use a renderer that disables or sanitizes raw HTML for untrusted input.

### How do I localize the texts or the dates?

The date of a file in a file list follows the culture Klyroo.UI resolves, and file sizes accept a `CultureInfo` in `FormatFileSize`. The component labels come from `DisplayTexts`, with one built-in set per locale of Klyroo.UI (`DisplayLocales`): a component shows the set of the locale its Klyroo.UI texts come from, so a user who switches language with `KlyLocaleSelect` gets the labels in that language too. English and French are reviewed; the other sets are drafts. Change the texts of one locale at startup:

```csharp
builder.Services.AddKlyrooDisplay(d => d.Texts("fr", t => t with
{
    NoPlaysLeft = "Vous avez utilisé toutes vos lectures",
    PlaysLeft = n => n == 1 ? "Encore 1 lecture" : $"Encore {n} lectures",
}));
```

A text with a number or a name is a function, so each language can choose its plural forms. A locale you add to Klyroo.UI ("fr-CA") starts from the set of the language it resolves to, and takes the changes you register for its own code.
