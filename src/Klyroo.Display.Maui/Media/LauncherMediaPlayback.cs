namespace Klyroo.Display.Maui;

/// <summary>
/// The playback a player control uses when the app sets none: pressing play opens the file with the platform (<see cref="Launcher"/>), so the browser or the media app of the device plays it.
/// It reports nothing back and supports nothing (<see cref="MediaPlaybackCapabilities.None"/>), so the control shows the play button, the plays left and the download button only.
/// Opening the file counts as a play for <see cref="KlyMediaPlayerBase.MaxPlays"/>.
/// </summary>
public sealed class LauncherMediaPlayback : IMediaPlayback
{
    private readonly Func<string, Task>? _opener;
    private string? _src;

    /// <summary>Creates a playback that opens files with the platform.</summary>
    public LauncherMediaPlayback()
    {
    }

    /// <summary>Creates a playback that opens files with <paramref name="opener"/> instead of the platform.</summary>
    /// <param name="opener">Opens the address or the path of the file.</param>
    public LauncherMediaPlayback(Func<string, Task> opener)
    {
        ArgumentNullException.ThrowIfNull(opener);
        _opener = opener;
    }

    /// <inheritdoc />
    public MediaPlaybackCapabilities Capabilities => MediaPlaybackCapabilities.None;

    /// <inheritdoc />
    public View? Surface => null;

    /// <inheritdoc />
    public event EventHandler<double>? TimeChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public event EventHandler<double>? DurationChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public event EventHandler? Ended
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public event EventHandler<bool>? PlayingChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public event EventHandler? PlayFailed
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public Task OpenAsync(MediaPlaybackSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _src = source.Src;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The platform could not open the file: the player goes back to paused.</exception>
    public async Task PlayAsync(double? fromSeconds)
    {
        if (!await DisplayLauncher.OpenAsync(_src, _opener)) throw new InvalidOperationException($"The platform could not open '{_src}'.");
    }

    /// <inheritdoc />
    public Task PauseAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public Task SeekAsync(double seconds) => Task.CompletedTask;

    /// <inheritdoc />
    public void Apply(MediaPlaybackOptions options)
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
