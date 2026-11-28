namespace Klyroo.Display.Maui;

/// <summary>
/// The built-in texts of the Klyroo.Display controls: button labels, accessible names and counters. The controls read <see cref="Current"/> when they are built or refreshed,
/// so set it once at startup (the way <see cref="Klyroo.UI.Maui.KlyTheme.Register"/> is used) to translate them. Text you give to a control (titles, labels, values) is never translated.
/// </summary>
/// <remarks>
/// A text that holds a number or a name is a function, so a language can choose its own plural forms and word order. Build a variant with a <c>with</c> expression:
/// <c>DisplayTexts.Current = DisplayTexts.Current with { Reveal = "Afficher" };</c>. The Blazor package ships a set per locale; this one holds the English texts.
/// </remarks>
public sealed record DisplayTexts
{
    private static DisplayTexts _current = new();

    /// <summary>Creates the texts with their English defaults.</summary>
    public DisplayTexts()
    {
    }

    /// <summary>The texts the controls use. Replace it at startup to translate the controls.</summary>
    public static DisplayTexts Current
    {
        get => _current;
        set => _current = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Code, secret and file list blocks

    /// <summary>The Show more button of a file list, given the number of files the next press reveals (<c>"Show more · 3"</c>).</summary>
    public Func<int, string> ShowMore { get; init; } = n => $"Show more · {n}";

    /// <summary>The details button and panel title of the file preview (<c>"Details"</c>).</summary>
    public string Details { get; init; } = "Details";

    /// <summary>The accessible name of the copy button of a code block and of a secret block (<c>"Copy code"</c>).</summary>
    public string CopyCode { get; init; } = "Copy code";

    /// <summary>The label of a secret block without a label (<c>"Secret"</c>).</summary>
    public string Secret { get; init; } = "Secret";

    /// <summary>The label of a code block without a label or language (<c>"Code"</c>).</summary>
    public string Code { get; init; } = "Code";

    /// <summary>The button that unmasks a secret block (<c>"Reveal"</c>).</summary>
    public string Reveal { get; init; } = "Reveal";

    /// <summary>The button that masks a revealed secret block again (<c>"Conceal"</c>).</summary>
    public string Conceal { get; init; } = "Conceal";

    // Media players

    /// <summary>The accessible name of an audio player without a title (<c>"Audio player"</c>).</summary>
    public string AudioPlayer { get; init; } = "Audio player";

    /// <summary>The accessible name of a video player without a title (<c>"Video player"</c>).</summary>
    public string VideoPlayer { get; init; } = "Video player";

    /// <summary>The play button (<c>"Play"</c>).</summary>
    public string Play { get; init; } = "Play";

    /// <summary>The pause button (<c>"Pause"</c>).</summary>
    public string Pause { get; init; } = "Pause";

    /// <summary>The large play button over a paused video (<c>"Play the video"</c>).</summary>
    public string PlayVideo { get; init; } = "Play the video";

    /// <summary>The timeline slider (<c>"Seek"</c>).</summary>
    public string Seek { get; init; } = "Seek";

    /// <summary>The spoken value of the timeline slider and of the progress bar of a non-seekable player, given the position and the length of the clip (<c>"0:37 of 1:00"</c>).</summary>
    public Func<string, string, string> SeekValueText { get; init; } = (position, length) => $"{position} of {length}";

    /// <summary>The read-only timeline of a non-seekable player (<c>"Playback progress"</c>).</summary>
    public string PlaybackProgress { get; init; } = "Playback progress";

    /// <summary>The message of a locked player (<c>"No plays left"</c>).</summary>
    public string NoPlaysLeft { get; init; } = "No plays left";

    /// <summary>The remaining plays of a player with a quota, given their number (<c>"2 plays left"</c>).</summary>
    public Func<int, string> PlaysLeft { get; init; } = n => n == 1 ? "1 play left" : $"{n} plays left";

    /// <summary>The mute button of an unmuted player (<c>"Mute"</c>).</summary>
    public string Mute { get; init; } = "Mute";

    /// <summary>The mute button of a muted player (<c>"Unmute"</c>).</summary>
    public string Unmute { get; init; } = "Unmute";

    /// <summary>The volume slider (<c>"Volume"</c>).</summary>
    public string Volume { get; init; } = "Volume";

    /// <summary>The playback speed button (<c>"Playback speed"</c>).</summary>
    public string PlaybackSpeed { get; init; } = "Playback speed";

    /// <summary>The captions button of the video player (<c>"Subtitles"</c>).</summary>
    public string Captions { get; init; } = "Subtitles";

    /// <summary>The download button of the players and the file preview (<c>"Download"</c>).</summary>
    public string Download { get; init; } = "Download";

    // File preview

    /// <summary>The announcement of gallery navigation for screen readers, given the one-based position, the number of files and the file name (<c>"2 of 5: report.pdf"</c>).</summary>
    public Func<int, int, string, string> PreviewPosition { get; init; } = (position, count, name) => $"{position} of {count}: {name}";

    /// <summary>The previous file button of a gallery (<c>"Previous file"</c>).</summary>
    public string PreviousFile { get; init; } = "Previous file";

    /// <summary>The next file button of a gallery (<c>"Next file"</c>).</summary>
    public string NextFile { get; init; } = "Next file";

    /// <summary>The zoom out button of an image (<c>"Zoom out"</c>).</summary>
    public string ZoomOut { get; init; } = "Zoom out";

    /// <summary>The zoom in button of an image (<c>"Zoom in"</c>).</summary>
    public string ZoomIn { get; init; } = "Zoom in";

    /// <summary>The button that resets the zoom of an image (<c>"Reset zoom"</c>).</summary>
    public string ResetZoom { get; init; } = "Reset zoom";

    /// <summary>The rotate button of an image (<c>"Rotate 90 degrees"</c>).</summary>
    public string Rotate { get; init; } = "Rotate 90 degrees";

    /// <summary>The close button of the preview (<c>"Close preview"</c>).</summary>
    public string ClosePreview { get; init; } = "Close preview";

    /// <summary>The notice above a rendition, given the kind of the original file (<c>"Converted preview · The original is a Office file."</c>).</summary>
    public Func<string, string> ConvertedPreview { get; init; } = kind => $"Converted preview · The original is a {kind} file.";

    /// <summary>The download button of the rendition notice (<c>"Download original"</c>).</summary>
    public string DownloadOriginal { get; init; } = "Download original";

    /// <summary>The file name row of the details panel (<c>"Name"</c>).</summary>
    public string DetailName { get; init; } = "Name";

    /// <summary>The file type row of the details panel (<c>"Type"</c>).</summary>
    public string DetailType { get; init; } = "Type";

    /// <summary>The file size row of the details panel (<c>"Size"</c>).</summary>
    public string DetailSize { get; init; } = "Size";

    /// <summary>The title of the card shown for a file without a renderer (<c>"No preview available"</c>).</summary>
    public string NoPreview { get; init; } = "No preview available";

    /// <summary>The text of the card shown for a file without a renderer (<c>"Download this file to open it in another application."</c>).</summary>
    public string NoPreviewHint { get; init; } = "Download this file to open it in another application.";

    /// <summary>The footer of a CSV table, given the number of rows, the number of columns and the delimiter (<c>"12 rows · 4 columns · delimiter ','"</c>, with <c>"tab"</c> for a tab).</summary>
    public Func<int, int, string, string> CsvSummary { get; init; } = (rows, columns, delimiter) => $"{rows} {(rows == 1 ? "row" : "rows")} · {columns} {(columns == 1 ? "column" : "columns")} · delimiter '{delimiter}'";

    /// <summary>The notice of a CSV table cut to its first rows, given the number of rows shown (<c>"Showing first 1,000 rows"</c>).</summary>
    public Func<int, string> CsvTruncated { get; init; } = n => $"Showing first {n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)} rows";

    /// <summary>The tab that shows rendered Markdown (<c>"Preview"</c>).</summary>
    public string MarkdownPreview { get; init; } = "Preview";

    /// <summary>The tab that shows Markdown source (<c>"Source"</c>).</summary>
    public string MarkdownSource { get; init; } = "Source";

    /// <summary>The line count of a text file (<c>"1 line"</c>, <c>"12 lines"</c>).</summary>
    public Func<int, string> LineCount { get; init; } = n => n == 1 ? "1 line" : $"{n} lines";

    /// <summary>The toggle that wraps long lines of a text file (<c>"Wrap lines"</c>).</summary>
    public string WrapLines { get; init; } = "Wrap lines";

    /// <summary>The button of the default media playback, which opens the file with another app (<c>"Open"</c>).</summary>
    public string Open { get; init; } = "Open";
}
