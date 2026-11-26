namespace Klyroo.Display;

/// <summary>
/// The built-in texts of <see cref="KlyDisplay"/>, <see cref="KlyAudioPlayer"/>, <see cref="KlyVideoPlayer"/> and
/// <see cref="KlyFilePreviewProvider"/>: button labels, accessible names, empty states and counters. There is one set per locale of
/// Klyroo.UI (<see cref="DisplayLocales"/>), and a component shows the set of its current locale, the one its <c>Messages</c> come from.
/// Change a set with <see cref="KlyrooDisplayOptions.Texts"/> in <c>AddKlyrooDisplay(d =&gt; ...)</c>.
/// </summary>
/// <remarks>
/// A text that contains a number or a name is a function, so a language can choose its own plural forms and word order.
/// Build a variant with a <c>with</c> expression. A new instance holds the English texts. Text you pass to the components
/// (titles, labels, values) is never translated.
/// </remarks>
/// <example>
/// <code>
/// builder.Services.AddKlyrooDisplay(d =&gt; d.Texts("fr", t =&gt; t with
/// {
///     NoPlaysLeft = "Vous avez utilisé toutes vos lectures",
///     PlaysLeft = n =&gt; n == 1 ? "Encore 1 lecture" : $"Encore {n} lectures",
/// }));
/// </code>
/// </example>
public sealed record DisplayTexts
{
    /// <summary>Creates the texts with their English defaults.</summary>
    public DisplayTexts()
    {
    }

    // KlyDisplay

    /// <summary>The Show more button of a list, given the number of items the next click reveals (<c>"Show more · 3"</c>).</summary>
    public Func<int, string> ShowMore { get; init; } = n => $"Show more · {n}";

    /// <summary>The toggle that expands a clamped Text block (<c>"Read more"</c>).</summary>
    public string ReadMore { get; init; } = "Read more";

    /// <summary>The toggle that clamps an expanded Text block again (<c>"Show less"</c>).</summary>
    public string ShowLess { get; init; } = "Show less";

    /// <summary>The toggle that opens the details of an Alert (<c>"Show details"</c>).</summary>
    public string ShowDetails { get; init; } = "Show details";

    /// <summary>The toggle that closes the details of an Alert or the lines of a <c>collapsible</c> Breakdown (<c>"Hide details"</c>).</summary>
    public string HideDetails { get; init; } = "Hide details";

    /// <summary>The toggle that opens the lines of a <c>collapsible</c> Breakdown, and the details button and panel title of the file preview (<c>"Details"</c>).</summary>
    public string Details { get; init; } = "Details";

    /// <summary>The accessible name of a copy button, given the label or value it copies (<c>"Copy Email"</c>).</summary>
    public Func<string, string> CopyValue { get; init; } = label => $"Copy {label}";

    /// <summary>The accessible name of the copy button of a Code block (<c>"Copy code"</c>).</summary>
    public string CopyCode { get; init; } = "Copy code";

    /// <summary>The text of a Chart block without data (<c>"No data"</c>).</summary>
    public string NoData { get; init; } = "No data";

    /// <summary>The accessible name of a Meter without a title (<c>"Meter"</c>).</summary>
    public string Meter { get; init; } = "Meter";

    /// <summary>The <c>countdown</c> Date text for the current day (<c>"Today"</c>).</summary>
    public string Today { get; init; } = "Today";

    /// <summary>The <c>countdown</c> Date text for the next day (<c>"Tomorrow"</c>).</summary>
    public string Tomorrow { get; init; } = "Tomorrow";

    /// <summary>The <c>countdown</c> Date text for a date two days away or more, given the number of days (<c>"In 5 days"</c>).</summary>
    public Func<int, string> InDays { get; init; } = n => $"In {n} days";

    /// <summary>The <c>countdown</c> Date text for a past date, given the number of days (<c>"3 days overdue"</c>).</summary>
    public Func<int, string> DaysOverdue { get; init; } = n => n == 1 ? "1 day overdue" : $"{n} days overdue";

    /// <summary>The length of a <c>range</c> Date, given the number of days (<c>"7 days"</c>).</summary>
    public Func<int, string> DayCount { get; init; } = n => n == 1 ? "1 day" : $"{n} days";

    /// <summary>The alternative text of a gallery image when the block has no title, given its one-based position (<c>"Image 2"</c>).</summary>
    public Func<int, string> ImageAlt { get; init; } = n => $"Image {n}";

    /// <summary>The alternative text of a Media image when the block has no title (<c>"Media"</c>).</summary>
    public string MediaAlt { get; init; } = "Media";

    /// <summary>The label of a <c>secret</c> Code block without a title or language (<c>"Secret"</c>).</summary>
    public string Secret { get; init; } = "Secret";

    /// <summary>The label of a Code block without a title or language (<c>"Code"</c>).</summary>
    public string Code { get; init; } = "Code";

    /// <summary>The button that unmasks a <c>secret</c> Code block (<c>"Reveal"</c>).</summary>
    public string Reveal { get; init; } = "Reveal";

    /// <summary>The button that masks a revealed <c>secret</c> Code block again (<c>"Conceal"</c>).</summary>
    public string Conceal { get; init; } = "Conceal";

    /// <summary>The title of an Empty block without a title (<c>"Nothing to show"</c>).</summary>
    public string NothingToShow { get; init; } = "Nothing to show";

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

    /// <summary>The picture-in-picture button of the video player (<c>"Picture in picture"</c>).</summary>
    public string PictureInPicture { get; init; } = "Picture in picture";

    /// <summary>The fullscreen button of the video player (<c>"Fullscreen"</c>).</summary>
    public string Fullscreen { get; init; } = "Fullscreen";

    /// <summary>The fullscreen button of the video player while it is fullscreen (<c>"Exit fullscreen"</c>).</summary>
    public string ExitFullscreen { get; init; } = "Exit fullscreen";

    /// <summary>The download link of the players and the file preview (<c>"Download"</c>).</summary>
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

    /// <summary>The button that expands a modal or drawer to the whole viewport (<c>"Expand preview"</c>).</summary>
    public string ExpandPreview { get; init; } = "Expand preview";

    /// <summary>The button that returns an expanded preview to its container (<c>"Restore preview size"</c>).</summary>
    public string RestorePreview { get; init; } = "Restore preview size";

    /// <summary>The close button of the preview (<c>"Close preview"</c>).</summary>
    public string ClosePreview { get; init; } = "Close preview";

    /// <summary>The notice above a rendition, given the kind of the original file (<c>"Converted preview · The original is a Office file."</c>).</summary>
    public Func<string, string> ConvertedPreview { get; init; } = kind => $"Converted preview · The original is a {kind} file.";

    /// <summary>The download link of the rendition notice (<c>"Download original"</c>).</summary>
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
    public Func<int, string> CsvTruncated { get; init; } = n => $"Showing first {DisplayLocales.Grouped(n, ",")} rows";

    /// <summary>The tab that shows rendered Markdown (<c>"Preview"</c>).</summary>
    public string MarkdownPreview { get; init; } = "Preview";

    /// <summary>The tab that shows Markdown source (<c>"Source"</c>).</summary>
    public string MarkdownSource { get; init; } = "Source";

    /// <summary>The line count of a text file (<c>"1 line"</c>, <c>"12 lines"</c>).</summary>
    public Func<int, string> LineCount { get; init; } = n => n == 1 ? "1 line" : $"{n} lines";

    /// <summary>The toggle that wraps long lines of a text file (<c>"Wrap lines"</c>).</summary>
    public string WrapLines { get; init; } = "Wrap lines";
}
