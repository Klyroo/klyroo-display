namespace Klyroo.Display;

/// <summary>
/// Renders the file preview overlay that <see cref="FilePreviewService"/> opens: a dialog, drawer or fullscreen panel with a header
/// (name, kind, size, zoom and rotate for images, custom actions, details, download, close), a stage that renders the file, a rendition notice,
/// and a strip of thumbnails for galleries. Place one provider in your layout, once.
/// </summary>
/// <remarks>
/// The overlay responds to Escape, the arrow keys, Home and End, plus and minus, 0 and R. It needs an interactive render mode.
/// </remarks>
public partial class KlyFilePreviewProvider
{
}
