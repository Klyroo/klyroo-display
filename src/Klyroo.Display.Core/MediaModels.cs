namespace Klyroo.Display;

/// <summary>A control of <c>KlyAudioPlayer</c> or <c>KlyVideoPlayer</c> that can be hidden through their <c>Hide</c> parameter.</summary>
public enum MediaPlayerControl
{
    /// <summary>The elapsed and total time label.</summary>
    Time,

    /// <summary>The mute button and the volume slider.</summary>
    Volume,

    /// <summary>The playback speed button.</summary>
    Rate,

    /// <summary>The captions button of the video player.</summary>
    Captions,

    /// <summary>The picture-in-picture button of the video player. It shows only once the browser reports picture-in-picture support, and not when the player is not seekable, since the video element then disables picture-in-picture.</summary>
    PictureInPicture,

    /// <summary>The fullscreen button of the video player, which shows the whole player fullscreen. It shows only once the browser reports fullscreen support.</summary>
    Fullscreen,

    /// <summary>The download link.</summary>
    Download,
}

/// <summary>The <c>kind</c> of a <see cref="VideoPlayerTrack"/>.</summary>
public enum VideoTrackKind
{
    /// <summary>A translation of the dialogue.</summary>
    Subtitles,
    /// <summary>A transcription of the dialogue and the sounds, for viewers who cannot hear.</summary>
    Captions,
    /// <summary>A description of the video for viewers who cannot see it.</summary>
    Descriptions,
    /// <summary>The titles of the chapters.</summary>
    Chapters,
    /// <summary>Data for scripts, not shown.</summary>
    Metadata,
}

/// <summary>How much of a clip the browser loads before the user plays it.</summary>
public enum MediaPreload
{
    /// <summary>Nothing.</summary>
    None,
    /// <summary>The metadata only (the duration and the size of a video).</summary>
    Metadata,
    /// <summary>Whatever the browser decides is good, up to the whole file.</summary>
    Auto,
}

/// <summary>A subtitle or caption track of a video, rendered as a <c>track</c> element.</summary>
/// <param name="Src">The URL of the WebVTT file.</param>
/// <param name="SrcLang">The BCP 47 language code of the track, for example <c>"fr"</c>.</param>
/// <param name="Label">The label shown in the browser's track list.</param>
/// <param name="Kind">The kind of the track, <see cref="VideoTrackKind.Subtitles"/> by default.</param>
/// <param name="Default">Whether the track is shown initially. With several default tracks, the first one wins.</param>
public sealed record VideoPlayerTrack(
    string Src,
    string SrcLang,
    string Label,
    VideoTrackKind Kind = VideoTrackKind.Subtitles,
    bool Default = false);

/// <summary>The playback position reported by the <c>OnTimeUpdate</c> callback of the audio and video players.</summary>
/// <param name="Position">Seconds elapsed relative to the clip start.</param>
/// <param name="Length">Total clip duration in seconds.</param>
/// <param name="CurrentTime">Raw media element time in seconds.</param>
public sealed record MediaPlayerProgress(
    double Position,
    double Length,
    double CurrentTime);
