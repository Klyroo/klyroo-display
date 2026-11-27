namespace Klyroo.Display.Maui;

/// <summary>What an <see cref="IMediaPlayback"/> can do. A player control only shows the controls its playback supports.</summary>
[Flags]
public enum MediaPlaybackCapabilities
{
    /// <summary>Nothing but starting the file: the playback hands it to another app and reports nothing back. The player shows the play button and nothing else.</summary>
    None = 0,

    /// <summary>The playback reports the position (<see cref="IMediaPlayback.TimeChanged"/>), the duration and the end of the file, so the player shows a time and a timeline and can pause.</summary>
    Position = 1,

    /// <summary><see cref="IMediaPlayback.SeekAsync"/> moves the position, so the timeline is a slider. Without it the timeline is a read-only progress bar.</summary>
    Seek = 2,

    /// <summary>The playback applies <see cref="MediaPlaybackOptions.Volume"/> and <see cref="MediaPlaybackOptions.Muted"/>, so the player shows the mute button and the volume slider.</summary>
    Volume = 4,

    /// <summary>The playback applies <see cref="MediaPlaybackOptions.Rate"/>, so the player shows the speed button.</summary>
    Rate = 8,

    /// <summary>The playback shows the caption track <see cref="MediaPlaybackOptions.Track"/> of the video, so the video player shows the captions button.</summary>
    Captions = 16,

    /// <summary>Everything above.</summary>
    All = Position | Seek | Volume | Rate | Captions,
}
