namespace Klyroo.Display.Maui;

/// <summary>A file of a <see cref="KlyFileList"/>: the file the preview opens, and what the row shows besides it.</summary>
/// <param name="File">The file. Its name, size, MIME type and date feed the row, and the whole item list opens as a gallery in the file preview.</param>
/// <param name="Description">A line under the name, before the size.</param>
/// <param name="Meta">Text at the end of the row. Without it, the last-modified date of the file, when it has one.</param>
/// <param name="Icon">An icon name (see <see cref="KlyIconNames"/>) shown in the badge instead of the extension.</param>
/// <param name="Tone">A theme color for the badge, custom colors included.</param>
public sealed record FileListItem(FilePreviewFile File, string? Description = null, string? Meta = null, string? Icon = null, KlyColor? Tone = null)
{
    /// <summary>Lists a file with nothing but its own data, so <c>[new FilePreviewFile { Src = "/a.pdf" }]</c> converts to a list of items.</summary>
    /// <param name="file">The file.</param>
    public static implicit operator FileListItem(FilePreviewFile file) => new(file);

    /// <summary>Lists a file with nothing but its own data.</summary>
    /// <param name="file">The file.</param>
    public static FileListItem FromFile(FilePreviewFile file) => new(file);
}
