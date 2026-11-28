namespace Klyroo.Display.Maui.Tests;

/// <summary>An <see cref="IMediaPlayback"/> that records what the player asks, and lets a test report what a real playback would.</summary>
internal sealed class FakePlayback : IMediaPlayback
{
    public MediaPlaybackCapabilities Capabilities { get; set; } = MediaPlaybackCapabilities.All;

    public View? Surface { get; set; }

    public bool Disposed { get; private set; }

    public bool FailPlay { get; set; }

    public List<MediaPlaybackSource> Opened { get; } = [];

    public List<double?> Plays { get; } = [];

    public int Pauses { get; private set; }

    public List<double> Seeks { get; } = [];

    public List<MediaPlaybackOptions> Applied { get; } = [];

    public MediaPlaybackOptions? LastOptions => Applied.Count == 0 ? null : Applied[^1];

    public event EventHandler<double>? TimeChanged;

    public event EventHandler<double>? DurationChanged;

    public event EventHandler? Ended;

    public event EventHandler<bool>? PlayingChanged;

    public event EventHandler? PlayFailed;

    public Task OpenAsync(MediaPlaybackSource source)
    {
        Opened.Add(source);
        return Task.CompletedTask;
    }

    public Task PlayAsync(double? fromSeconds)
    {
        if (FailPlay) throw new InvalidOperationException("cannot play");
        Plays.Add(fromSeconds);
        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        Pauses++;
        return Task.CompletedTask;
    }

    public Task SeekAsync(double seconds)
    {
        Seeks.Add(seconds);
        return Task.CompletedTask;
    }

    public void Apply(MediaPlaybackOptions options) => Applied.Add(options);

    public void Dispose() => Disposed = true;

    public void RaiseTime(double seconds) => TimeChanged?.Invoke(this, seconds);

    public void RaiseDuration(double seconds) => DurationChanged?.Invoke(this, seconds);

    public void RaiseEnded() => Ended?.Invoke(this, EventArgs.Empty);

    public void RaisePlaying(bool playing) => PlayingChanged?.Invoke(this, playing);

    public void RaiseFailed() => PlayFailed?.Invoke(this, EventArgs.Empty);
}
