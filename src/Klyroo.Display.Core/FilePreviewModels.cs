using System.Globalization;

namespace Klyroo.Display;

/// <summary>
/// The kind of a file as seen by the preview. <see cref="FilePreviewService.DetectKind(FilePreviewFile)"/> returns one of
/// these values, and <c>KlyFilePreviewProvider</c> picks its renderer from it.
/// </summary>
public enum FilePreviewKind
{
    /// <summary>A PDF document, shown in the browser's built-in PDF viewer inside an <c>iframe</c>.</summary>
    Pdf,

    /// <summary>A raster or vector image, shown with zoom and rotation controls.</summary>
    Image,

    /// <summary>A video, shown in <c>KlyVideoPlayer</c>.</summary>
    Video,

    /// <summary>An audio file, shown in <c>KlyAudioPlayer</c>.</summary>
    Audio,

    /// <summary>A CSV or TSV file, shown as a table parsed from <see cref="FilePreviewFile.Content"/>.</summary>
    Csv,

    /// <summary>A Markdown file, shown as source, or as HTML when the provider has a <c>MarkdownRenderer</c>.</summary>
    Markdown,

    /// <summary>A plain text, JSON, XML or source code file, shown from <see cref="FilePreviewFile.Content"/>.</summary>
    Text,

    /// <summary>A word-processing, spreadsheet or presentation file. It has no built-in renderer, so provide a <see cref="FilePreviewFile.Rendition"/> to preview it.</summary>
    Office,

    /// <summary>A compressed archive. It has no built-in renderer and offers a download instead.</summary>
    Archive,

    /// <summary>Any file whose kind could not be detected. It offers a download instead of a preview.</summary>
    Other,
}

/// <summary>The layout of the preview overlay rendered by <c>KlyFilePreviewProvider</c>.</summary>
public enum FilePreviewContainer
{
    /// <summary>A centered dialog, at most 5xl wide.</summary>
    Modal,

    /// <summary>A panel docked to the end edge of the viewport, at most 4xl wide.</summary>
    Drawer,

    /// <summary>The whole viewport.</summary>
    Fullscreen,
}

/// <summary>A label and value pair shown in the details panel of the file preview.</summary>
/// <param name="Label">The row label, for example <c>"Uploaded by"</c>.</param>
/// <param name="Value">The row value.</param>
public sealed record FilePreviewDetail(string Label, string Value);

/// <summary>A custom button added to the header of the file preview.</summary>
public sealed class FilePreviewAction
{
    /// <summary>Creates an action. Set <see cref="Label"/> before use.</summary>
    public FilePreviewAction()
    {
    }

    /// <summary>The button text.</summary>
    public required string Label { get; init; }

    /// <summary>An icon name shown before the label, resolved through the icons of Klyroo.UI.</summary>
    public string? Icon { get; init; }

    /// <summary>A URL. When set, the action renders as a link to it instead of a button, and <see cref="OnSelect"/>, when also set, still runs on click.</summary>
    public string? Href { get; init; }

    /// <summary>The callback invoked with the file being previewed when the action is clicked.</summary>
    public Func<FilePreviewFile, Task>? OnSelect { get; init; }
}

/// <summary>A file to show in <c>KlyFilePreviewProvider</c>. Pass it to <see cref="FilePreviewService.Open"/> or <see cref="FilePreviewService.OpenGallery"/>.</summary>
public sealed class FilePreviewFile
{
    /// <summary>Creates a file. Set <see cref="Src"/> before use.</summary>
    public FilePreviewFile()
    {
    }

    /// <summary>The URL or data URI of the file. Images, video, audio and PDF are loaded from it by the browser.</summary>
    public required string Src { get; init; }

    /// <summary>An optional stable identifier of the file. The provider uses it as the key of the file's thumbnail in the gallery strip, so the strip keeps its elements when a gallery is reordered; it must be unique within a gallery.</summary>
    public string? Id { get; init; }

    /// <summary>The display file name. When <see langword="null"/>, the last path segment of <see cref="Src"/> is used.</summary>
    public string? Name { get; init; }

    /// <summary>The MIME type. When set and recognized, it takes precedence over the file extension when the kind is detected.</summary>
    public string? Mime { get; init; }

    /// <summary>The kind to use, skipping detection. Set it when the name and MIME type do not tell the kind, for example a signed URL.</summary>
    public FilePreviewKind? Kind { get; init; }

    /// <summary>The size in bytes, shown in the header and in the details panel.</summary>
    public long? Size { get; init; }

    /// <summary>The last-modified time, shown in the header as a date.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The poster image of a video, or the cover image of an audio file.</summary>
    public string? Poster { get; init; }

    /// <summary>The text of the file. CSV, Markdown and text files are rendered from it: the provider does not download <see cref="Src"/> for them.</summary>
    public string? Content { get; init; }

    /// <summary>A file shown instead of this one, for example a PDF conversion of an Office document. The original stays available for download.</summary>
    public FilePreviewFile? Rendition { get; init; }

    /// <summary>Extra rows for the details panel.</summary>
    public IReadOnlyList<FilePreviewDetail>? Details { get; init; }

    /// <summary>The URL of the download link. When <see langword="null"/>, <see cref="Src"/> is used.</summary>
    public string? DownloadUrl { get; init; }

    /// <summary>Custom buttons added to the header while this file is shown.</summary>
    public IReadOnlyList<FilePreviewAction>? Actions { get; init; }

    /// <summary>Caption tracks passed to the video player.</summary>
    public IReadOnlyList<VideoPlayerTrack>? Tracks { get; init; }
}

/// <summary>Options for a preview session started with <see cref="FilePreviewService.Open"/> or <see cref="FilePreviewService.OpenGallery"/>.</summary>
public sealed class FilePreviewOptions
{
    /// <summary>Creates options with the defaults.</summary>
    public FilePreviewOptions()
    {
    }

    /// <summary>The zero-based index of the file shown first. It is clamped to the gallery.</summary>
    public int Index { get; init; }

    /// <summary>The container to use. When <see langword="null"/>, <see cref="FilePreviewService.ResolveContainer"/> picks one from the kinds of the files.</summary>
    public FilePreviewContainer? Mode { get; init; }

    /// <summary>Whether Previous and Next wrap around at the ends of the gallery (<see langword="false"/> by default).</summary>
    public bool Loop { get; init; }

    /// <summary>The page a PDF opens on, passed to the browser viewer as <c>#page=N</c>. It is ignored when the URL already has a fragment.</summary>
    public int? PdfPage { get; init; }
}
