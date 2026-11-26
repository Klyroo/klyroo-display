namespace Klyroo.Display;

/// <summary>
/// Renders the file preview overlay that <see cref="FilePreviewService"/> opens: a dialog, drawer or fullscreen panel with a header
/// (name, kind, size, zoom and rotate for images, custom actions, details, download, close), a stage that renders the file, a rendition notice,
/// and a strip of thumbnails for galleries. Place one provider in your layout, once.
/// </summary>
/// <remarks>
/// When the overlay opens, the focus moves into it and Tab and Shift+Tab stay inside it; when it closes, the focus returns to the element that had it before (usually the button that opened the preview). Closing the overlay also returns a modal or drawer that was expanded to fullscreen to its own
/// size for the next file. The overlay responds to Escape, the arrow keys, Home and End, plus and minus, 0 and R. It needs an interactive
/// render mode. Its texts come from <see cref="DisplayTexts"/>.
/// </remarks>
public partial class KlyFilePreviewProvider
{
}
