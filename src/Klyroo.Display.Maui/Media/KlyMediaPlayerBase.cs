using System.Globalization;

namespace Klyroo.Display.Maui;

/// <summary>
/// The state and the rules shared by <see cref="KlyAudioPlayer"/> and <see cref="KlyVideoPlayer"/>: the clip (<see cref="Start"/>, <see cref="End"/>, <see cref="Duration"/>), the number of plays
/// (<see cref="MaxPlays"/>), the non-seekable timeline, the volume, the speed, and the transport buttons. The sound and the picture come from an <see cref="IMediaPlayback"/>, set with <see cref="Playback"/>.
/// </summary>
/// <remarks>
/// The play count and the lock are enforced by the control, whatever the playback is. The position, the duration and the end of the file come back from the playback.
/// The Blazor players' keyboard shortcuts, picture-in-picture and fullscreen buttons and the download file name are not ported: a touch screen has no focus to key on, and a fullscreen or
/// picture-in-picture window is a decision of the app that owns the media player.
/// </remarks>
public abstract class KlyMediaPlayerBase : Border
{
    /// <summary>Identifies <see cref="Src"/>.</summary>
    public static readonly BindableProperty SrcProperty = BindableProperty.Create(nameof(Src), typeof(string), typeof(KlyMediaPlayerBase), "", propertyChanged: (b, o, n) => ((KlyMediaPlayerBase)b).OnSrcChanged((string?)o));
    /// <summary>Identifies <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(KlyMediaPlayerBase), null, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Preload"/>.</summary>
    public static readonly BindableProperty PreloadProperty = BindableProperty.Create(nameof(Preload), typeof(MediaPreload), typeof(KlyMediaPlayerBase), MediaPreload.Metadata, propertyChanged: (b, _, _) => ((KlyMediaPlayerBase)b).OpenCurrent());
    /// <summary>Identifies <see cref="Autoplay"/>.</summary>
    public static readonly BindableProperty AutoplayProperty = BindableProperty.Create(nameof(Autoplay), typeof(bool), typeof(KlyMediaPlayerBase), false);
    /// <summary>Identifies <see cref="Loop"/>.</summary>
    public static readonly BindableProperty LoopProperty = BindableProperty.Create(nameof(Loop), typeof(bool), typeof(KlyMediaPlayerBase), false, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Compact"/>.</summary>
    public static readonly BindableProperty CompactProperty = BindableProperty.Create(nameof(Compact), typeof(bool), typeof(KlyMediaPlayerBase), false, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Start"/>.</summary>
    public static readonly BindableProperty StartProperty = BindableProperty.Create(nameof(Start), typeof(double), typeof(KlyMediaPlayerBase), 0d, propertyChanged: (b, _, _) => ((KlyMediaPlayerBase)b).OnClipChanged());
    /// <summary>Identifies <see cref="End"/>.</summary>
    public static readonly BindableProperty EndProperty = BindableProperty.Create(nameof(End), typeof(double?), typeof(KlyMediaPlayerBase), null, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Duration"/>.</summary>
    public static readonly BindableProperty DurationProperty = BindableProperty.Create(nameof(Duration), typeof(double), typeof(KlyMediaPlayerBase), 60d, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Seekable"/>.</summary>
    public static readonly BindableProperty SeekableProperty = BindableProperty.Create(nameof(Seekable), typeof(bool), typeof(KlyMediaPlayerBase), true, propertyChanged: Changed);
    /// <summary>Identifies <see cref="MaxPlays"/>.</summary>
    public static readonly BindableProperty MaxPlaysProperty = BindableProperty.Create(nameof(MaxPlays), typeof(int), typeof(KlyMediaPlayerBase), 0, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Plays"/>.</summary>
    public static readonly BindableProperty PlaysProperty = BindableProperty.Create(nameof(Plays), typeof(int), typeof(KlyMediaPlayerBase), 0, BindingMode.TwoWay, propertyChanged: (b, _, n) => ((KlyMediaPlayerBase)b).OnPlaysChanged((int)n));
    /// <summary>Identifies <see cref="Rate"/>.</summary>
    public static readonly BindableProperty RateProperty = BindableProperty.Create(nameof(Rate), typeof(double), typeof(KlyMediaPlayerBase), 1d, BindingMode.TwoWay, propertyChanged: (b, _, n) => ((KlyMediaPlayerBase)b).OnRateChanged((double)n));
    /// <summary>Identifies <see cref="Rates"/>.</summary>
    public static readonly BindableProperty RatesProperty = BindableProperty.Create(nameof(Rates), typeof(IReadOnlyList<double>), typeof(KlyMediaPlayerBase), (IReadOnlyList<double>)[0.5, 0.75, 1, 1.25, 1.5, 2], propertyChanged: Changed);
    /// <summary>Identifies <see cref="Volume"/>.</summary>
    public static readonly BindableProperty VolumeProperty = BindableProperty.Create(nameof(Volume), typeof(double), typeof(KlyMediaPlayerBase), 1d, BindingMode.TwoWay,
        coerceValue: (_, v) => double.IsNaN((double)v) ? 1d : Math.Clamp((double)v, 0, 1), propertyChanged: (b, _, n) => ((KlyMediaPlayerBase)b).OnVolumeChanged((double)n));
    /// <summary>Identifies <see cref="Muted"/>.</summary>
    public static readonly BindableProperty MutedProperty = BindableProperty.Create(nameof(Muted), typeof(bool), typeof(KlyMediaPlayerBase), false, BindingMode.TwoWay, propertyChanged: (b, _, n) => ((KlyMediaPlayerBase)b).OnMutedChanged((bool)n));
    /// <summary>Identifies <see cref="Download"/>.</summary>
    public static readonly BindableProperty DownloadProperty = BindableProperty.Create(nameof(Download), typeof(bool), typeof(KlyMediaPlayerBase), false, propertyChanged: Changed);
    /// <summary>Identifies <see cref="DownloadUrl"/>.</summary>
    public static readonly BindableProperty DownloadUrlProperty = BindableProperty.Create(nameof(DownloadUrl), typeof(string), typeof(KlyMediaPlayerBase), null, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Hide"/>.</summary>
    public static readonly BindableProperty HideProperty = BindableProperty.Create(nameof(Hide), typeof(IReadOnlyList<MediaPlayerControl>), typeof(KlyMediaPlayerBase), null, propertyChanged: Changed);
    /// <summary>Identifies <see cref="Playback"/>.</summary>
    public static readonly BindableProperty PlaybackProperty = BindableProperty.Create(nameof(Playback), typeof(IMediaPlayback), typeof(KlyMediaPlayerBase), null, propertyChanged: (b, o, n) => ((KlyMediaPlayerBase)b).OnPlaybackChanged((IMediaPlayback?)o));

    private LauncherMediaPlayback? _default;
    private IMediaPlayback? _attached;
    private bool _playing;
    private bool _playActive;
    private double _currentTime;
    private double? _mediaDuration;
    private string? _loadedSrc;
    private bool _autoplayed;

    private protected KlyMediaPlayerBase()
    {
        HorizontalOptions = LayoutOptions.Fill;
        Loaded += (_, _) => _ = RunAutoplayAsync();
        Unloaded += (_, _) => Suspend();
    }

    /// <summary>Raised when playback starts or resumes.</summary>
    public event EventHandler? Played;

    /// <summary>Raised when the user pauses.</summary>
    public event EventHandler? Paused;

    /// <summary>Raised when the position reaches the end of the clip.</summary>
    public event EventHandler? Ended;

    /// <summary>Raised when no plays are left: when a play ends with <see cref="Plays"/> at <see cref="MaxPlays"/>, and each time a play is requested while the player is locked.</summary>
    public event EventHandler? LimitReached;

    /// <summary>Raised with the position during playback, about four times a second, and when the user seeks.</summary>
    public event EventHandler<MediaPlayerProgress>? TimeUpdated;

    /// <summary>Raised when the download button is pressed, before the file opens.</summary>
    public event EventHandler? DownloadClicked;

    /// <summary>Raised with the new count when a play starts or <see cref="Plays"/> changes.</summary>
    public event EventHandler<int>? PlaysChanged;

    /// <summary>Raised with the new rate when the speed button is pressed or <see cref="Rate"/> changes.</summary>
    public event EventHandler<double>? RateChanged;

    /// <summary>Raised with the new volume, from 0 to 1, when the volume slider moves or <see cref="Volume"/> changes.</summary>
    public event EventHandler<double>? VolumeChanged;

    /// <summary>Raised with the new value when the mute button is pressed or <see cref="Muted"/> changes.</summary>
    public event EventHandler<bool>? MutedChanged;

    /// <summary>The address or the path of the media file.</summary>
    public string Src { get => (string)GetValue(SrcProperty); set => SetValue(SrcProperty, value); }

    /// <summary>What a screen reader says for the player. Without it, the localized "Audio player" or "Video player".</summary>
    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>How much of the file the playback loads before the user plays it: <see cref="MediaPreload.Metadata"/> by default.</summary>
    public MediaPreload Preload { get => (MediaPreload)GetValue(PreloadProperty); set => SetValue(PreloadProperty, value); }

    /// <summary>Starts playing once the player is in the page.</summary>
    public bool Autoplay { get => (bool)GetValue(AutoplayProperty); set => SetValue(AutoplayProperty, value); }

    /// <summary>Starts the clip again when it ends. It is ignored when the player is restricted by <see cref="MaxPlays"/> or by <see cref="Seekable"/> being <see langword="false"/>.</summary>
    public bool Loop { get => (bool)GetValue(LoopProperty); set => SetValue(LoopProperty, value); }

    /// <summary>Draws a denser control bar: less padding and spacing, smaller buttons and a shorter volume slider.</summary>
    public bool Compact { get => (bool)GetValue(CompactProperty); set => SetValue(CompactProperty, value); }

    /// <summary>The clip start, in seconds. Positions and durations are reported relative to it.</summary>
    public double Start { get => (double)GetValue(StartProperty); set => SetValue(StartProperty, value); }

    /// <summary>The clip end, in seconds. It is used when it is greater than <see cref="Start"/>, otherwise <see cref="Duration"/> gives the end.</summary>
    public double? End { get => (double?)GetValue(EndProperty); set => SetValue(EndProperty, value); }

    /// <summary>The total duration in seconds, used until the playback reports its own (<c>60</c> by default). Set <see cref="End"/> to stop before the end of the file.</summary>
    public double Duration { get => (double)GetValue(DurationProperty); set => SetValue(DurationProperty, value); }

    /// <summary>Whether the user can move the position with the timeline. When <see langword="false"/> the timeline is a read-only progress bar (<see langword="true"/> by default). The playback must also support <see cref="MediaPlaybackCapabilities.Seek"/>.</summary>
    public bool Seekable { get => (bool)GetValue(SeekableProperty); set => SetValue(SeekableProperty, value); }

    /// <summary>The number of times the clip can be started (<c>0</c>, the default, means unlimited). Once <see cref="Plays"/> reaches it and the current play ends, the player locks and shows that no plays are left.</summary>
    public int MaxPlays { get => (int)GetValue(MaxPlaysProperty); set => SetValue(MaxPlaysProperty, value); }

    /// <summary>The number of plays already used. The player increments it when a play starts: keep the value in your own state and bind it, so a new page does not lose it. Two-way bindable.</summary>
    public int Plays { get => (int)GetValue(PlaysProperty); set => SetValue(PlaysProperty, value); }

    /// <summary>The playback rate shown on the speed button (<c>1.0</c> by default). Two-way bindable.</summary>
    public double Rate { get => (double)GetValue(RateProperty); set => SetValue(RateProperty, value); }

    /// <summary>The rates the speed button cycles through. The button is hidden when there are fewer than two (0.5, 0.75, 1, 1.25, 1.5 and 2 by default).</summary>
    public IReadOnlyList<double>? Rates { get => (IReadOnlyList<double>?)GetValue(RatesProperty); set => SetValue(RatesProperty, value); }

    /// <summary>The volume, from 0 to 1 (<c>1.0</c> by default). The volume slider changes it. Two-way bindable.</summary>
    public double Volume { get => (double)GetValue(VolumeProperty); set => SetValue(VolumeProperty, value); }

    /// <summary>Whether the player is muted. The mute button toggles it. Two-way bindable.</summary>
    public bool Muted { get => (bool)GetValue(MutedProperty); set => SetValue(MutedProperty, value); }

    /// <summary>Shows the download button (<see langword="false"/> by default). It also shows when <see cref="DownloadUrl"/> is set. It opens <see cref="DownloadUrl"/>, or <see cref="Src"/>, with the platform.</summary>
    public bool Download { get => (bool)GetValue(DownloadProperty); set => SetValue(DownloadProperty, value); }

    /// <summary>The address the download button opens. When <see langword="null"/>, <see cref="Src"/> is used.</summary>
    public string? DownloadUrl { get => (string?)GetValue(DownloadUrlProperty); set => SetValue(DownloadUrlProperty, value); }

    /// <summary>The controls to remove from the control bar. <see cref="MediaPlayerControl.Volume"/> removes the mute button and the volume slider. <see cref="MediaPlayerControl.PictureInPicture"/> and <see cref="MediaPlayerControl.Fullscreen"/> are not drawn by the native players.</summary>
    public IReadOnlyList<MediaPlayerControl>? Hide { get => (IReadOnlyList<MediaPlayerControl>?)GetValue(HideProperty); set => SetValue(HideProperty, value); }

    /// <summary>
    /// What plays the file, usually a wrapper around the media player of the app. Without one the player uses <see cref="LauncherMediaPlayback"/>, which opens the file with another app,
    /// and shows only the play button, the plays left and the download button.
    /// </summary>
    public IMediaPlayback? Playback { get => (IMediaPlayback?)GetValue(PlaybackProperty); set => SetValue(PlaybackProperty, value); }

    /// <summary>The number of plays left, or <see cref="int.MaxValue"/> when <see cref="MaxPlays"/> is zero.</summary>
    public int RemainingPlays => MaxPlays > 0 ? Math.Max(0, MaxPlays - Plays) : int.MaxValue;

    /// <summary>Whether the player is locked: <see cref="MaxPlays"/> is set, no plays are left and no play is in progress.</summary>
    public bool IsLocked => MaxPlays > 0 && RemainingPlays <= 0 && !_playActive;

    /// <summary>Whether the file is playing now.</summary>
    public bool IsPlaying => _playing;

    /// <summary>The clip end in seconds: <see cref="End"/> when it is greater than <see cref="Start"/>, otherwise the duration the playback reported, or <see cref="Duration"/>.</summary>
    public double ClipEnd => End is { } e && e > Start ? e : Math.Max(Start, _mediaDuration ?? Duration);

    /// <summary>The clip length in seconds, <see cref="ClipEnd"/> minus <see cref="Start"/>.</summary>
    public double ClipLength => Math.Max(0, ClipEnd - Start);

    /// <summary>The position inside the clip in seconds, between zero and <see cref="ClipLength"/>.</summary>
    public double ClipPosition => Math.Clamp(_currentTime - Start, 0, ClipLength);

    /// <summary>The playback in use: <see cref="Playback"/>, or the default one that opens the file with the platform.</summary>
    internal IMediaPlayback EffectivePlayback => Playback ?? (_default ??= new LauncherMediaPlayback());

    /// <summary>Whether this player is a video player.</summary>
    internal abstract bool IsVideo { get; }

    internal MediaPlaybackCapabilities Capabilities => EffectivePlayback.Capabilities;

    internal bool HasPosition => Capabilities.HasFlag(MediaPlaybackCapabilities.Position);

    /// <summary>Whether the timeline moves the position: the player is seekable, the playback can seek, and the player is not locked.</summary>
    internal bool CanSeek => Seekable && Capabilities.HasFlag(MediaPlaybackCapabilities.Seek) && !IsLocked;

    internal bool IsRestricted => MaxPlays > 0 || !Seekable;

    /// <summary>Whether a control is removed by <see cref="Hide"/>.</summary>
    internal bool IsHidden(MediaPlayerControl control) => Hide is not null && Hide.Contains(control);

    internal bool ShowsDownload => !IsHidden(MediaPlayerControl.Download) && (Download || !string.IsNullOrEmpty(DownloadUrl));

    /// <summary>Opens the address instead of the platform; for the tests.</summary>
    internal Func<string, Task>? Opener { get; set; }

    /// <summary>Called when something the control draws changed.</summary>
    private protected abstract void OnStateChanged();

    /// <inheritdoc />
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName != nameof(IsEnabled)) return;
        Opacity = DisplayUi.OpacityOf(IsEnabled);
        if (!IsEnabled && _playing)
        {
            _playing = false;
            _ = SafePauseAsync();
        }
        OnStateChanged();
    }

    // --- transport ---------------------------------------------------------------------------------------------------------------------------

    /// <summary>Starts, resumes or pauses the player, as the play button does. Starting a new play consumes one of <see cref="MaxPlays"/>. It does nothing when the control is disabled, and only raises <see cref="LimitReached"/> when <see cref="IsLocked"/>.</summary>
    /// <returns>A task that completes when the playback has been asked.</returns>
    public async Task TogglePlayAsync()
    {
        if (!IsEnabled) return;
        if (IsLocked)
        {
            LimitReached?.Invoke(this, EventArgs.Empty);
            return;
        }
        var playback = EffectivePlayback;
        Attach(playback);
        if (!HasPosition)
        {
            // A playback that reports nothing (the default one hands the file to another app): opening it is one play, and the control never shows it as playing.
            Plays++;
            await StartAsync(playback, Start, markPlaying: false);
            return;
        }
        if (_playing)
        {
            _playing = false;
            await SafePauseAsync();
            Paused?.Invoke(this, EventArgs.Empty);
            OnStateChanged();
            return;
        }

        double? from = null;
        if (!_playActive)
        {
            _playActive = true;
            _currentTime = Start;
            Plays++;
            from = Start;
        }
        await StartAsync(playback, from, markPlaying: true);
    }

    private async Task StartAsync(IMediaPlayback playback, double? from, bool markPlaying)
    {
        _playing = markPlaying;
        try
        {
            await playback.PlayAsync(from);
        }
        catch (Exception)
        {
            HandlePlayFailed();
            return;
        }
        Played?.Invoke(this, EventArgs.Empty);
        OnStateChanged();
    }

    /// <summary>Ends the current play: stops, moves the position to the clip end, and raises <see cref="Ended"/> and, when no plays are left, <see cref="LimitReached"/>. The player calls it when the playback reaches the clip end.</summary>
    /// <returns>A task that completes when the playback has been paused.</returns>
    public async Task CompletePlayAsync()
    {
        _playing = false;
        _playActive = false;
        _currentTime = ClipEnd;
        await SafePauseAsync();
        Ended?.Invoke(this, EventArgs.Empty);
        if (MaxPlays > 0 && Plays >= MaxPlays) LimitReached?.Invoke(this, EventArgs.Empty);
        OnStateChanged();
    }

    /// <summary>Moves the position to an offset inside the clip, raises <see cref="TimeUpdated"/>, and ends the play when the offset reaches the clip end. It does nothing unless <see cref="Seekable"/> and the playback can seek, and when the control is disabled or <see cref="IsLocked"/>.</summary>
    /// <param name="clipOffsetSeconds">The offset in seconds from <see cref="Start"/>, clamped to the clip.</param>
    /// <returns>A task that completes when the events have been raised.</returns>
    public async Task SeekAsync(double clipOffsetSeconds)
    {
        if (!CanSeek || !IsEnabled) return;
        var clamped = Math.Clamp(clipOffsetSeconds, 0, ClipLength);
        _currentTime = Start + clamped;
        var reachesEnd = _currentTime >= ClipEnd - 0.05 && ClipLength > 0;
        if (reachesEnd && _playing)
        {
            // Pause first, so the playback does not also report the end while TimeUpdated runs.
            _playing = false;
            await SafePauseAsync();
        }
        try
        {
            await EffectivePlayback.SeekAsync(_currentTime);
        }
        catch (Exception)
        {
            // A playback that cannot seek keeps its position; the timeline follows what it reports.
        }
        TimeUpdated?.Invoke(this, new MediaPlayerProgress(ClipPosition, ClipLength, _currentTime));
        if (reachesEnd)
        {
            await CompletePlayAsync();
            return;
        }
        OnStateChanged();
    }

    /// <summary>Toggles <see cref="Muted"/>. It does nothing when the control is disabled.</summary>
    /// <returns>A completed task.</returns>
    public Task ToggleMuteAsync()
    {
        if (IsEnabled) Muted = !Muted;
        return Task.CompletedTask;
    }

    /// <summary>Sets <see cref="Volume"/>, as the volume slider does.</summary>
    /// <param name="volume">The new volume, clamped between 0 and 1.</param>
    /// <returns>A completed task.</returns>
    public Task SetVolumeAsync(double volume)
    {
        if (IsEnabled && !double.IsNaN(volume)) Volume = volume;
        return Task.CompletedTask;
    }

    /// <summary>Selects the next entry of <see cref="Rates"/> (wrapping around). It does nothing with fewer than two rates.</summary>
    /// <returns>A completed task.</returns>
    public Task CycleRateAsync()
    {
        if (Rates is not { Count: > 1 } rates) return Task.CompletedTask;
        var index = -1;
        for (var i = 0; i < rates.Count; i++)
        {
            if (Math.Abs(rates[i] - Rate) < 0.001) { index = i; break; }
        }
        Rate = rates[(index + 1) % rates.Count];
        return Task.CompletedTask;
    }

    /// <summary>Starts the player when <see cref="Autoplay"/> is on and it has not started yet. The control calls it once it is in the page.</summary>
    internal async Task RunAutoplayAsync()
    {
        if (!Autoplay || _autoplayed) return;
        _autoplayed = true;
        await TogglePlayAsync();
    }

    /// <summary>Presses the download button: raises <see cref="DownloadClicked"/>, then opens the file with the platform.</summary>
    internal async Task DownloadAsync()
    {
        DownloadClicked?.Invoke(this, EventArgs.Empty);
        await DisplayLauncher.OpenAsync(DownloadUrl ?? Src, Opener);
    }

    // --- what the playback reports ------------------------------------------------------------------------------------------------------------

    internal Task ReceiveTimeAsync(double time) => HandleTimeAsync(time);

    internal void ReceiveDuration(double duration)
    {
        _mediaDuration = duration;
        OnStateChanged();
    }

    internal Task ReceiveEndedAsync()
    {
        if (!_playActive) return Task.CompletedTask;
        if (Loop && !IsRestricted)
        {
            // The playback does not loop by itself: a clip that reached its end starts again, and is not a new play.
            _currentTime = Start;
            return LoopAsync();
        }
        return CompletePlayAsync();
    }

    internal async Task ReceivePlayingAsync(bool playing)
    {
        // Playback started or stopped outside the buttons, for example with a hardware media key: the same rules as the play button apply.
        if (playing == _playing) return;
        await TogglePlayAsync();
        if (playing && !_playing) await SafePauseAsync();
    }

    internal void HandlePlayFailed()
    {
        _playing = false;
        OnStateChanged();
    }

    private async Task HandleTimeAsync(double time)
    {
        if (!_playing) return;
        _currentTime = time;
        TimeUpdated?.Invoke(this, new MediaPlayerProgress(ClipPosition, ClipLength, _currentTime));
        if (End is { } end && end > Start && time >= end - 0.05)
        {
            if (Loop && !IsRestricted)
            {
                _currentTime = Start;
                await LoopAsync();
                return;
            }
            await CompletePlayAsync();
            return;
        }
        OnStateChanged();
    }

    private async Task LoopAsync()
    {
        try
        {
            await EffectivePlayback.PlayAsync(Start);
        }
        catch (Exception)
        {
            HandlePlayFailed();
            return;
        }
        OnStateChanged();
    }

    private async Task SafePauseAsync()
    {
        try
        {
            await EffectivePlayback.PauseAsync();
        }
        catch (Exception)
        {
            // Nothing playing on the playback side: the control already shows it paused.
        }
    }

    // --- property changes ---------------------------------------------------------------------------------------------------------------------

    private static void Changed(BindableObject bindable, object oldValue, object newValue)
    {
        var player = (KlyMediaPlayerBase)bindable;
        player.PushOptions();
        player.OnStateChanged();
    }

    private void OnClipChanged()
    {
        if (_currentTime < Start) _currentTime = Start;
        PushOptions();
        OnStateChanged();
    }

    private void OnSrcChanged(string? oldSrc)
    {
        if (_loadedSrc is not null && _loadedSrc != Src)
        {
            // A new file: the playback reloads and stops, so the play in progress ends without spending anything more.
            _playing = false;
            _playActive = false;
            _mediaDuration = null;
            _currentTime = Start;
            OnSourceReset();
        }
        _loadedSrc = Src;
        OpenCurrent();
        OnStateChanged();
    }

    private void OnPlaysChanged(int plays)
    {
        PlaysChanged?.Invoke(this, plays);
        OnStateChanged();
    }

    private void OnRateChanged(double rate)
    {
        RateChanged?.Invoke(this, rate);
        PushOptions();
        OnStateChanged();
    }

    private void OnVolumeChanged(double volume)
    {
        VolumeChanged?.Invoke(this, volume);
        PushOptions();
        OnStateChanged();
    }

    private void OnMutedChanged(bool muted)
    {
        MutedChanged?.Invoke(this, muted);
        PushOptions();
        OnStateChanged();
    }

    private void OnPlaybackChanged(IMediaPlayback? old)
    {
        Detach(old ?? _default);
        _playing = false;
        _playActive = false;
        Attach(EffectivePlayback);
        OpenCurrent();
        OnStateChanged();
    }

    private void Attach(IMediaPlayback playback)
    {
        if (ReferenceEquals(_attached, playback)) return;
        Detach(_attached);
        _attached = playback;
        playback.TimeChanged += OnPlaybackTime;
        playback.DurationChanged += OnPlaybackDuration;
        playback.Ended += OnPlaybackEnded;
        playback.PlayingChanged += OnPlaybackPlaying;
        playback.PlayFailed += OnPlaybackFailed;
        PushOptions();
    }

    private void Detach(IMediaPlayback? playback)
    {
        if (playback is null) return;
        playback.TimeChanged -= OnPlaybackTime;
        playback.DurationChanged -= OnPlaybackDuration;
        playback.Ended -= OnPlaybackEnded;
        playback.PlayingChanged -= OnPlaybackPlaying;
        playback.PlayFailed -= OnPlaybackFailed;
        if (ReferenceEquals(_attached, playback)) _attached = null;
    }

    private void OnPlaybackTime(object? sender, double time) => DisplayUi.Post(this, () => _ = ReceiveTimeAsync(time));

    private void OnPlaybackDuration(object? sender, double duration) => DisplayUi.Post(this, () => ReceiveDuration(duration));

    private void OnPlaybackEnded(object? sender, EventArgs e) => DisplayUi.Post(this, () => _ = ReceiveEndedAsync());

    private void OnPlaybackPlaying(object? sender, bool playing) => DisplayUi.Post(this, () => _ = ReceivePlayingAsync(playing));

    private void OnPlaybackFailed(object? sender, EventArgs e) => DisplayUi.Post(this, HandlePlayFailed);

    /// <summary>Asks the playback to load the file again, with what the control knows now (the poster, the tracks, the preload).</summary>
    private protected void OpenCurrent()
    {
        if (string.IsNullOrEmpty(Src)) return;
        Attach(EffectivePlayback);
        _ = OpenAsync(EffectivePlayback, SourceFor());
    }

    private static async Task OpenAsync(IMediaPlayback playback, MediaPlaybackSource source)
    {
        try
        {
            await playback.OpenAsync(source);
        }
        catch (Exception)
        {
            // The playback could not load the file: playing it reports the failure.
        }
    }

    /// <summary>What the playback is asked to load.</summary>
    private protected abstract MediaPlaybackSource SourceFor();

    /// <summary>Called when <see cref="Src"/> changes to another file, after the play in progress ended.</summary>
    private protected virtual void OnSourceReset()
    {
    }

    /// <summary>The caption track to show, or <c>-1</c>.</summary>
    private protected virtual int ActiveTrack => -1;

    internal MediaPlaybackOptions Options => new(Start, End is { } end && end > Start ? end : null, Loop && !IsRestricted, Muted, Rate, Volume, ActiveTrack);

    private protected void PushOptions()
    {
        if (_attached is not { } playback) return;
        try
        {
            playback.Apply(Options);
        }
        catch (Exception)
        {
            // A playback that rejects a setting keeps the previous one.
        }
    }

    private void Suspend()
    {
        // Leaving the page stops the sound; the plays already used stay counted, and the position stays.
        if (!_playing) return;
        _playing = false;
        _ = SafePauseAsync();
        OnStateChanged();
    }

    // --- formatting ---------------------------------------------------------------------------------------------------------------------------

    /// <summary>A time as <c>m:ss</c>, or <c>h:mm:ss</c> from one hour. A negative, infinite or not-a-number value is zero.</summary>
    internal static string FormatTime(double seconds)
    {
        if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) seconds = 0;
        var ts = TimeSpan.FromSeconds(Math.Round(seconds));
        return ts.TotalHours >= 1
            ? string.Create(CultureInfo.InvariantCulture, $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}")
            : string.Create(CultureInfo.InvariantCulture, $"{ts.Minutes}:{ts.Seconds:D2}");
    }

    internal string SeekValueText => DisplayTexts.Current.SeekValueText(FormatTime(ClipPosition), FormatTime(ClipLength));
}
