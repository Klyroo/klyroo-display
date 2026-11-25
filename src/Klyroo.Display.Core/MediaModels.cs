namespace Klyroo.Display;

/// <summary>Controls that can be selectively hidden on <c>KlyAudioPlayer</c> and <c>KlyVideoPlayer</c>.</summary>
public enum MediaPlayerControl
{
    Time,
    Volume,
    Rate,
    Captions,
    PictureInPicture,
    Fullscreen,
    Download,
}

/// <summary>A subtitle or caption track for <c>KlyVideoPlayer</c>.</summary>
public sealed record VideoPlayerTrack(
    string Src,
    string SrcLang,
    string Label,
    string Kind = "subtitles",
    bool Default = false);

/// <summary>Playback progress snapshot reported by <c>KlyAudioPlayer</c> and <c>KlyVideoPlayer</c>.</summary>
/// <param name="Position">Seconds elapsed relative to the clip start.</param>
/// <param name="Length">Total clip duration in seconds.</param>
/// <param name="CurrentTime">Raw media element time in seconds.</param>
public sealed record MediaPlayerProgress(
    double Position,
    double Length,
    double CurrentTime);
