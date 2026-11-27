namespace Klyroo.Display.Maui;

/// <summary>
/// What a <see cref="KlyAudioPlayer"/> or a <see cref="KlyVideoPlayer"/> plays through. MAUI has no media player control of its own, so the players draw the transport
/// (play and pause, timeline, time, volume, speed, the clip, plays and lock rules of Klyroo.Display) and leave the sound and the picture to an implementation of this interface:
/// wrap the media player of your app (the platform one, <c>MediaElement</c> of the Community Toolkit, LibVLCSharp...) and set it as <see cref="KlyMediaPlayerBase.Playback"/>.
/// Without one, a player uses <see cref="LauncherMediaPlayback"/>, which opens the file with another app.
/// </summary>
/// <remarks>
/// The events may be raised from any thread. <see cref="TimeChanged"/> is expected about four times a second while the file plays.
/// A playback that supports less than everything says so with <see cref="Capabilities"/> and the player hides the controls it cannot drive.
/// </remarks>
public interface IMediaPlayback : IDisposable
{
    /// <summary>What the playback supports.</summary>
    MediaPlaybackCapabilities Capabilities { get; }

    /// <summary>The view that draws the picture of a video, such as the native media view of the player behind the playback. <see langword="null"/> for audio, or when there is no picture: the video player then draws the poster.</summary>
    View? Surface { get; }

    /// <summary>Raised with the position in seconds of the file while it plays, and after a seek.</summary>
    event EventHandler<double>? TimeChanged;

    /// <summary>Raised with the duration in seconds of the file when it is known.</summary>
    event EventHandler<double>? DurationChanged;

    /// <summary>Raised when the file reaches its end.</summary>
    event EventHandler? Ended;

    /// <summary>Raised when playback starts or stops outside the controls of the player, for example with a hardware media key. The argument is <see langword="true"/> for playing.</summary>
    event EventHandler<bool>? PlayingChanged;

    /// <summary>Raised when the file could not start: the player goes back to paused.</summary>
    event EventHandler? PlayFailed;

    /// <summary>Loads a file. It is called when the player gets the playback, and again when <see cref="KlyMediaPlayerBase.Src"/> changes.</summary>
    /// <param name="source">The file and how to show it.</param>
    Task OpenAsync(MediaPlaybackSource source);

    /// <summary>Starts or resumes the file. An exception says that the file could not start: the player goes back to paused, as it does for <see cref="PlayFailed"/>.</summary>
    /// <param name="fromSeconds">The position to start from, in seconds of the file, when a new play starts; <see langword="null"/> to resume where it paused.</param>
    Task PlayAsync(double? fromSeconds);

    /// <summary>Pauses the file.</summary>
    Task PauseAsync();

    /// <summary>Moves the position.</summary>
    /// <param name="seconds">The new position, in seconds of the file.</param>
    Task SeekAsync(double seconds);

    /// <summary>Applies the state of the player: clip, loop, sound, speed and caption track.</summary>
    /// <param name="options">The state to apply.</param>
    void Apply(MediaPlaybackOptions options);
}
