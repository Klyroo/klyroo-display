namespace Klyroo.Display.Maui;

/// <summary>
/// A video frame with an overlay play button and a control bar (play and pause, timeline, mute and volume, speed, captions and an optional download button), that can restrict playback to a clip
/// (<see cref="KlyMediaPlayerBase.Start"/> and <see cref="KlyMediaPlayerBase.End"/>), to a number of plays (<see cref="KlyMediaPlayerBase.MaxPlays"/>) and to a non-seekable timeline.
/// </summary>
/// <remarks>
/// MAUI has no video player control: this one draws the frame and the transport, and shows the picture of the <see cref="IMediaPlayback"/> you set as <see cref="KlyMediaPlayerBase.Playback"/>
/// (its <see cref="IMediaPlayback.Surface"/>). Without one, the frame shows the <see cref="Poster"/> and the play button opens the file with the platform (<see cref="LauncherMediaPlayback"/>).
/// </remarks>
public class KlyVideoPlayer : KlyMediaPlayerBase
{
    /// <summary>Identifies <see cref="Poster"/>.</summary>
    public static readonly BindableProperty PosterProperty = BindableProperty.Create(nameof(Poster), typeof(string), typeof(KlyVideoPlayer), null, propertyChanged: (b, _, _) => ((KlyVideoPlayer)b).OnPosterChanged());
    /// <summary>Identifies <see cref="AspectRatio"/>.</summary>
    public static readonly BindableProperty AspectRatioProperty = BindableProperty.Create(nameof(AspectRatio), typeof(double), typeof(KlyVideoPlayer), 16d / 9d, propertyChanged: (b, _, _) => ((KlyVideoPlayer)b).FitHeight());
    /// <summary>Identifies <see cref="Tracks"/>.</summary>
    public static readonly BindableProperty TracksProperty = BindableProperty.Create(nameof(Tracks), typeof(IReadOnlyList<VideoPlayerTrack>), typeof(KlyVideoPlayer), null, propertyChanged: (b, _, _) => ((KlyVideoPlayer)b).OnTracksChanged());

    private readonly Image _poster = new() { Aspect = Aspect.AspectFit };
    private readonly ContentView _surface = new();
    private readonly Grid _overlay = new();
    private readonly MediaButton _start;
    private readonly Label _playsBadge;
    private readonly Border _playsBadgeBorder;
    private readonly Border _lockedBadge;
    private readonly Label _lockedText;
    private readonly Grid _barFrame = new() { VerticalOptions = LayoutOptions.End };
    private readonly MediaControlBar _bar;
    private bool _tracksInitialized;
    private int _activeTrack = -1;
    private View? _shownSurface;

    /// <summary>Creates a video player: set <see cref="KlyMediaPlayerBase.Src"/> and, to play inside the app, <see cref="KlyMediaPlayerBase.Playback"/>.</summary>
    public KlyVideoPlayer()
    {
        BackgroundColor = Colors.Black;
        StrokeThickness = 0;
        Padding = 0;
        this.SetRadius(DisplayThemeExtensions.RadiusOf(0.75));

        var texts = DisplayTexts.Current;
        _start = new MediaButton(KlyIconNames.Play, texts.PlayVideo, DisplayUi.PrimaryContrast, iconScale: 1.2, circleSize: 48, circleFill: DisplayUi.PrimaryFill) { WidthRequest = 56, HeightRequest = 56, HorizontalOptions = LayoutOptions.Center };
        _start.Clicked += (_, _) => _ = TogglePlayAsync();

        _playsBadge = DisplayUi.NewLabel(12, new KlyThemeColor(Colors.White, Colors.White), FontAttributes.None, LineBreakMode.NoWrap);
        _playsBadgeBorder = new Border { StrokeThickness = 0, Padding = new Thickness(10, 2), HorizontalOptions = LayoutOptions.Center, BackgroundColor = Colors.Black.WithAlpha(0.65f), Content = _playsBadge, IsVisible = false };
        _playsBadgeBorder.SetRadius(999);

        var lockIcon = new KlyIcon { Name = DisplayIcons.Lock, Size = KlySize.Sm, Scale = 1.1, IconColor = new KlyThemeColor(Colors.White, Colors.White), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _lockedBadge = new Border { StrokeThickness = 0, WidthRequest = 48, HeightRequest = 48, Padding = 0, BackgroundColor = Colors.Black.WithAlpha(0.7f), HorizontalOptions = LayoutOptions.Center, Content = lockIcon, IsVisible = false };
        _lockedBadge.SetRadius(24);
        _lockedText = DisplayUi.NewLabel(14, new KlyThemeColor(Colors.White, Colors.White), FontAttributes.Bold, LineBreakMode.WordWrap);
        _lockedText.HorizontalTextAlignment = TextAlignment.Center;
        _lockedText.IsVisible = false;

        _overlay.BackgroundColor = Colors.Black.WithAlpha(0.4f);
        _overlay.Padding = new Thickness(16);
        _overlay.Add(new VerticalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Children = { _start, _lockedBadge, _lockedText, _playsBadgeBorder } });

        _bar = new MediaControlBar(this, video: true, () => Tracks is { Count: > 0 }, CycleCaptions, () => _activeTrack >= 0);
        _barFrame.Padding = new Thickness(12, 24, 12, 10);
        _barFrame.Background = new LinearGradientBrush(
            [new GradientStop(Colors.Black.WithAlpha(0.8f), 0f), new GradientStop(Colors.Black.WithAlpha(0.5f), 0.5f), new GradientStop(Colors.Transparent, 1f)],
            new Point(0, 1), new Point(0, 0));
        _barFrame.Add(_bar);

        // A tap on the picture plays and pauses, as a tap on the video element does on the web.
        var tapLayer = new Grid { BackgroundColor = Colors.Transparent };
        DisplayUi.Tap(tapLayer, () => _ = TogglePlayAsync());

        var frame = new Grid();
        frame.Add(_poster);
        frame.Add(_surface);
        frame.Add(tapLayer);
        frame.Add(_overlay);
        frame.Add(_barFrame);
        Content = frame;

        SizeChanged += (_, _) => FitHeight();
        OnPosterChanged();
        OnStateChanged();
    }

    /// <summary>The poster image shown before the video plays: an address, or the name of an image of the app.</summary>
    public string? Poster { get => (string?)GetValue(PosterProperty); set => SetValue(PosterProperty, value); }

    /// <summary>The width over the height of the frame (<c>16 / 9</c> by default). The frame is as high as its width allows.</summary>
    public double AspectRatio { get => (double)GetValue(AspectRatioProperty); set => SetValue(AspectRatioProperty, value); }

    /// <summary>The caption tracks. The track marked <see cref="VideoPlayerTrack.Default"/> is shown first; the CC button then cycles through the tracks and off. The choice is kept and starts over when <see cref="KlyMediaPlayerBase.Src"/> changes.</summary>
    public IReadOnlyList<VideoPlayerTrack>? Tracks { get => (IReadOnlyList<VideoPlayerTrack>?)GetValue(TracksProperty); set => SetValue(TracksProperty, value); }

    /// <summary>The index of the caption track that is shown, or <c>-1</c> when the captions are off.</summary>
    public int ActiveTrackIndex => _activeTrack;

    internal override bool IsVideo => true;

    private protected override int ActiveTrack => _activeTrack;

    /// <summary>The control bar, for the tests.</summary>
    internal MediaControlBar Bar => _bar;

    /// <summary>The overlay over a paused video, for the tests.</summary>
    internal Grid Overlay => _overlay;

    /// <summary>The large play button, for the tests.</summary>
    internal MediaButton StartButton => _start;

    /// <summary>The poster, for the tests.</summary>
    internal Image PosterImage => _poster;

    internal Label LockedText => _lockedText;

    internal Border PlaysBadge => _playsBadgeBorder;

    internal Label PlaysBadgeText => _playsBadge;

    /// <summary>Selects the next caption track (wrapping around to off), as the CC button does.</summary>
    public void CycleCaptions()
    {
        if (Tracks is not { Count: > 0 } tracks) return;
        _activeTrack = _activeTrack + 1 >= tracks.Count ? -1 : _activeTrack + 1;
        PushOptions();
        OnStateChanged();
    }

    private protected override MediaPlaybackSource SourceFor() => new(Src, IsVideo: true, Poster, Preload, Tracks);

    private protected override void OnSourceReset()
    {
        // A new file comes with its own tracks: select its default track again.
        _tracksInitialized = false;
        InitTracks();
    }

    private void OnTracksChanged()
    {
        InitTracks();
        OpenCurrent();
        PushOptions();
        OnStateChanged();
    }

    private void InitTracks()
    {
        if (!_tracksInitialized && Tracks is { Count: > 0 } tracks)
        {
            // Only once: afterwards the CC button owns the choice, including turning the captions off.
            _tracksInitialized = true;
            _activeTrack = -1;
            for (var i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].Default) { _activeTrack = i; break; }
            }
        }
        if (_activeTrack >= (Tracks?.Count ?? 0)) _activeTrack = -1;
    }

    private void OnPosterChanged()
    {
        _poster.Source = DisplayImages.From(Poster);
        _poster.IsVisible = !string.IsNullOrEmpty(Poster);
        OpenCurrent();
    }

    private void FitHeight()
    {
        if (Width <= 0 || AspectRatio <= 0) return;
        var height = Width / AspectRatio;
        if (Math.Abs(HeightRequest - height) > 0.5) HeightRequest = height;
    }

    private protected override void OnStateChanged()
    {
        if (_bar is null) return;
        var texts = DisplayTexts.Current;
        var locked = IsLocked;
        SemanticProperties.SetDescription(this, Title ?? texts.VideoPlayer);

        _overlay.IsVisible = !IsPlaying;
        _start.IsVisible = !locked;
        _start.SetDescription(texts.PlayVideo);
        _start.IsEnabled = IsEnabled;
        _lockedBadge.IsVisible = locked;
        _lockedText.IsVisible = locked;
        _lockedText.Text = texts.NoPlaysLeft;
        _playsBadgeBorder.IsVisible = !locked && MaxPlays > 0;
        if (MaxPlays > 0) _playsBadge.Text = texts.PlaysLeft(RemainingPlays);

        _barFrame.Padding = Compact ? new Thickness(8, 16, 8, 6) : new Thickness(12, 24, 12, 10);
        _bar.Refresh();

        var surface = EffectivePlayback.Surface;
        if (!ReferenceEquals(surface, _shownSurface))
        {
            _shownSurface = surface;
            _surface.Content = surface;
        }
    }
}
