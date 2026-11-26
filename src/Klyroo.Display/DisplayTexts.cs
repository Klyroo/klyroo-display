using System.Globalization;

namespace Klyroo.Display;

/// <summary>
/// The built-in texts of <see cref="KlyDisplay"/>, <see cref="KlyAudioPlayer"/>, <see cref="KlyVideoPlayer"/> and
/// <see cref="KlyFilePreviewProvider"/>: button labels, accessible names, empty states and counters. They are English by default.
/// Change them once in <c>AddKlyrooDisplay(texts =&gt; ...)</c>; every component of the app reads the same instance.
/// </summary>
/// <remarks>
/// A text that contains a number or a name is a function, so a language can choose its own plural forms and word order.
/// The texts are not per culture: to follow the user's language, set them from your own resources in the callback, or change
/// the properties when the culture changes. Text you pass to the components (titles, labels, values) is never translated.
/// </remarks>
/// <example>
/// <code>
/// builder.Services.AddKlyrooDisplay(texts =&gt;
/// {
///     texts.ShowMore = n =&gt; $"Afficher {n} de plus";
///     texts.NoPlaysLeft = "Plus de lecture possible";
///     texts.PlaysLeft = n =&gt; n == 1 ? "1 lecture restante" : $"{n} lectures restantes";
/// });
/// </code>
/// </example>
public sealed class DisplayTexts
{
    /// <summary>Creates the texts with their English defaults.</summary>
    public DisplayTexts()
    {
    }

    // KlyDisplay

    /// <summary>The Show more button of a list, given the number of items the next click reveals (<c>"Show more · 3"</c>).</summary>
    public Func<int, string> ShowMore { get; set; } = n => $"Show more · {n}";

    /// <summary>The toggle that expands a clamped Text block (<c>"Read more"</c>).</summary>
    public string ReadMore { get; set; } = "Read more";

    /// <summary>The toggle that clamps an expanded Text block again (<c>"Show less"</c>).</summary>
    public string ShowLess { get; set; } = "Show less";

    /// <summary>The toggle that opens the details of an Alert (<c>"Show details"</c>).</summary>
    public string ShowDetails { get; set; } = "Show details";

    /// <summary>The toggle that closes the details of an Alert or the lines of a <c>collapsible</c> Breakdown (<c>"Hide details"</c>).</summary>
    public string HideDetails { get; set; } = "Hide details";

    /// <summary>The toggle that opens the lines of a <c>collapsible</c> Breakdown, and the details button and panel title of the file preview (<c>"Details"</c>).</summary>
    public string Details { get; set; } = "Details";

    /// <summary>The accessible name of a copy button, given the label or value it copies (<c>"Copy Email"</c>).</summary>
    public Func<string, string> CopyValue { get; set; } = label => $"Copy {label}";

    /// <summary>The accessible name of the copy button of a Code block (<c>"Copy code"</c>).</summary>
    public string CopyCode { get; set; } = "Copy code";

    /// <summary>The text of a Chart block without data (<c>"No data"</c>).</summary>
    public string NoData { get; set; } = "No data";

    /// <summary>The accessible name of a Meter without a title (<c>"Meter"</c>).</summary>
    public string Meter { get; set; } = "Meter";

    /// <summary>The <c>countdown</c> Date text for the current day (<c>"Today"</c>).</summary>
    public string Today { get; set; } = "Today";

    /// <summary>The <c>countdown</c> Date text for the next day (<c>"Tomorrow"</c>).</summary>
    public string Tomorrow { get; set; } = "Tomorrow";

    /// <summary>The <c>countdown</c> Date text for a date two days away or more, given the number of days (<c>"In 5 days"</c>).</summary>
    public Func<int, string> InDays { get; set; } = n => $"In {n} days";

    /// <summary>The <c>countdown</c> Date text for a past date, given the number of days (<c>"3 days overdue"</c>).</summary>
    public Func<int, string> DaysOverdue { get; set; } = n => n == 1 ? "1 day overdue" : $"{n} days overdue";

    /// <summary>The length of a <c>range</c> Date, given the number of days (<c>"7 days"</c>).</summary>
    public Func<int, string> DayCount { get; set; } = n => n == 1 ? "1 day" : $"{n} days";

    /// <summary>The alternative text of a gallery image when the block has no title, given its one-based position (<c>"Image 2"</c>).</summary>
    public Func<int, string> ImageAlt { get; set; } = n => $"Image {n}";

    /// <summary>The alternative text of a Media image when the block has no title (<c>"Media"</c>).</summary>
    public string MediaAlt { get; set; } = "Media";

    /// <summary>The label of a <c>secret</c> Code block without a title or language (<c>"Secret"</c>).</summary>
    public string Secret { get; set; } = "Secret";

    /// <summary>The label of a Code block without a title or language (<c>"Code"</c>).</summary>
    public string Code { get; set; } = "Code";

    /// <summary>The button that unmasks a <c>secret</c> Code block (<c>"Reveal"</c>).</summary>
    public string Reveal { get; set; } = "Reveal";

    /// <summary>The button that masks a revealed <c>secret</c> Code block again (<c>"Conceal"</c>).</summary>
    public string Conceal { get; set; } = "Conceal";

    /// <summary>The title of an Empty block without a title (<c>"Nothing to show"</c>).</summary>
    public string NothingToShow { get; set; } = "Nothing to show";

    // Media players

    /// <summary>The accessible name of an audio player without a title (<c>"Audio player"</c>).</summary>
    public string AudioPlayer { get; set; } = "Audio player";

    /// <summary>The accessible name of a video player without a title (<c>"Video player"</c>).</summary>
    public string VideoPlayer { get; set; } = "Video player";

    /// <summary>The play button (<c>"Play"</c>).</summary>
    public string Play { get; set; } = "Play";

    /// <summary>The pause button (<c>"Pause"</c>).</summary>
    public string Pause { get; set; } = "Pause";

    /// <summary>The large play button over a paused video (<c>"Play the video"</c>).</summary>
    public string PlayVideo { get; set; } = "Play the video";

    /// <summary>The timeline slider (<c>"Seek"</c>).</summary>
    public string Seek { get; set; } = "Seek";

    /// <summary>The read-only timeline of a non-seekable player (<c>"Playback progress"</c>).</summary>
    public string PlaybackProgress { get; set; } = "Playback progress";

    /// <summary>The message of a locked player (<c>"No plays left"</c>).</summary>
    public string NoPlaysLeft { get; set; } = "No plays left";

    /// <summary>The remaining plays of a player with a quota, given their number (<c>"2 plays left"</c>).</summary>
    public Func<int, string> PlaysLeft { get; set; } = n => n == 1 ? "1 play left" : $"{n} plays left";

    /// <summary>The mute button of an unmuted player (<c>"Mute"</c>).</summary>
    public string Mute { get; set; } = "Mute";

    /// <summary>The mute button of a muted player (<c>"Unmute"</c>).</summary>
    public string Unmute { get; set; } = "Unmute";

    /// <summary>The volume slider (<c>"Volume"</c>).</summary>
    public string Volume { get; set; } = "Volume";

    /// <summary>The playback speed button (<c>"Playback speed"</c>).</summary>
    public string PlaybackSpeed { get; set; } = "Playback speed";

    /// <summary>The captions button of the video player (<c>"Subtitles"</c>).</summary>
    public string Captions { get; set; } = "Subtitles";

    /// <summary>The picture-in-picture button of the video player (<c>"Picture in picture"</c>).</summary>
    public string PictureInPicture { get; set; } = "Picture in picture";

    /// <summary>The fullscreen button of the video player (<c>"Fullscreen"</c>).</summary>
    public string Fullscreen { get; set; } = "Fullscreen";

    /// <summary>The fullscreen button of the video player while it is fullscreen (<c>"Exit fullscreen"</c>).</summary>
    public string ExitFullscreen { get; set; } = "Exit fullscreen";

    /// <summary>The download link of the players and the file preview (<c>"Download"</c>).</summary>
    public string Download { get; set; } = "Download";

    // File preview

    /// <summary>The announcement of gallery navigation for screen readers, given the one-based position, the number of files and the file name (<c>"2 of 5: report.pdf"</c>).</summary>
    public Func<int, int, string, string> PreviewPosition { get; set; } = (position, count, name) => $"{position} of {count}: {name}";

    /// <summary>The previous file button of a gallery (<c>"Previous file"</c>).</summary>
    public string PreviousFile { get; set; } = "Previous file";

    /// <summary>The next file button of a gallery (<c>"Next file"</c>).</summary>
    public string NextFile { get; set; } = "Next file";

    /// <summary>The zoom out button of an image (<c>"Zoom out"</c>).</summary>
    public string ZoomOut { get; set; } = "Zoom out";

    /// <summary>The zoom in button of an image (<c>"Zoom in"</c>).</summary>
    public string ZoomIn { get; set; } = "Zoom in";

    /// <summary>The button that resets the zoom of an image (<c>"Reset zoom"</c>).</summary>
    public string ResetZoom { get; set; } = "Reset zoom";

    /// <summary>The rotate button of an image (<c>"Rotate 90 degrees"</c>).</summary>
    public string Rotate { get; set; } = "Rotate 90 degrees";

    /// <summary>The button that expands a modal or drawer to the whole viewport (<c>"Expand preview"</c>).</summary>
    public string ExpandPreview { get; set; } = "Expand preview";

    /// <summary>The button that returns an expanded preview to its container (<c>"Restore preview size"</c>).</summary>
    public string RestorePreview { get; set; } = "Restore preview size";

    /// <summary>The close button of the preview (<c>"Close preview"</c>).</summary>
    public string ClosePreview { get; set; } = "Close preview";

    /// <summary>The notice above a rendition, given the kind of the original file (<c>"Converted preview · The original is a Office file."</c>).</summary>
    public Func<string, string> ConvertedPreview { get; set; } = kind => $"Converted preview · The original is a {kind} file.";

    /// <summary>The download link of the rendition notice (<c>"Download original"</c>).</summary>
    public string DownloadOriginal { get; set; } = "Download original";

    /// <summary>The file name row of the details panel (<c>"Name"</c>).</summary>
    public string DetailName { get; set; } = "Name";

    /// <summary>The file type row of the details panel (<c>"Type"</c>).</summary>
    public string DetailType { get; set; } = "Type";

    /// <summary>The file size row of the details panel (<c>"Size"</c>).</summary>
    public string DetailSize { get; set; } = "Size";

    /// <summary>The title of the card shown for a file without a renderer (<c>"No preview available"</c>).</summary>
    public string NoPreview { get; set; } = "No preview available";

    /// <summary>The text of the card shown for a file without a renderer (<c>"Download this file to open it in another application."</c>).</summary>
    public string NoPreviewHint { get; set; } = "Download this file to open it in another application.";

    /// <summary>The footer of a CSV table, given the number of rows, the number of columns and the delimiter (<c>"12 rows · 4 columns · delimiter ','"</c>, with <c>"tab"</c> for a tab).</summary>
    public Func<int, int, string, string> CsvSummary { get; set; } = (rows, columns, delimiter) => $"{rows} rows · {columns} columns · delimiter '{delimiter}'";

    /// <summary>The notice of a CSV table cut to its first rows, given the number of rows shown (<c>"Showing first 1,000 rows"</c>).</summary>
    public Func<int, string> CsvTruncated { get; set; } = n => $"Showing first {n.ToString("N0", CultureInfo.InvariantCulture)} rows";

    /// <summary>The tab that shows rendered Markdown (<c>"Preview"</c>).</summary>
    public string MarkdownPreview { get; set; } = "Preview";

    /// <summary>The tab that shows Markdown source (<c>"Source"</c>).</summary>
    public string MarkdownSource { get; set; } = "Source";

    /// <summary>The line count of a text file (<c>"1 line"</c>, <c>"12 lines"</c>).</summary>
    public Func<int, string> LineCount { get; set; } = n => n == 1 ? "1 line" : $"{n} lines";

    /// <summary>The toggle that wraps long lines of a text file (<c>"Wrap lines"</c>).</summary>
    public string WrapLines { get; set; } = "Wrap lines";
}
