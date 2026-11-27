namespace Klyroo.Display.Maui;

/// <summary>
/// The control bar of a player: play and pause, time, timeline, plays left, mute and volume, speed, captions and download, drawn once and refreshed from the state of the player.
/// It wraps onto several lines when the screen is narrow, as the web bar does. The video bar draws white on the dark picture; the audio bar uses the tokens of the theme.
/// </summary>
internal sealed class MediaControlBar : ContentView
{
    private static readonly KlyThemeColor White = new(Colors.White, Colors.White);

    private readonly KlyMediaPlayerBase _player;
    private readonly bool _video;
    private readonly Func<bool> _hasCaptions;
    private readonly Action _cycleCaptions;
    private readonly Func<bool> _captionsOn;

    private readonly FlexLayout _controls = new() { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center, AlignContent = Microsoft.Maui.Layouts.FlexAlignContent.Center };
    private readonly HorizontalStackLayout _locked = new() { Spacing = 8, VerticalOptions = LayoutOptions.Center, IsVisible = false };
    private readonly Grid _timeline = new();
    private bool _syncing;

    public MediaControlBar(KlyMediaPlayerBase player, bool video, Func<bool> hasCaptions, Action cycleCaptions, Func<bool> captionsOn)
    {
        MediaIcons.Ensure();
        _player = player;
        _video = video;
        _hasCaptions = hasCaptions;
        _cycleCaptions = cycleCaptions;
        _captionsOn = captionsOn;

        var texts = DisplayTexts.Current;
        var text = video ? White.WithAlpha(0.9f) : KlyTheme.TextMuted;
        var playIcon = video ? White : DisplayUi.PrimaryContrast;
        var playFill = video ? White.WithAlpha(0.15f) : DisplayUi.PrimaryFill;
        var trackFill = DisplayUi.PrimaryFill;
        var trackRest = video ? White.WithAlpha(0.25f) : KlyTheme.BackgroundAccented;
        var badgeFill = video ? White.WithAlpha(0.15f) : KlyTheme.BackgroundMuted;

        Play = new MediaButton(KlyIconNames.Play, texts.Play, playIcon, iconScale: 0.9, circleSize: 32, circleFill: playFill);
        Play.Clicked += (_, _) => _ = _player.TogglePlayAsync();

        Time = DisplayUi.NewLabel(12, text, FontAttributes.None, LineBreakMode.NoWrap);
        Time.FontFamily = DisplayUi.MonoFont();
        Time.VerticalOptions = LayoutOptions.Center;

        Seek = new Slider { Minimum = 0, Maximum = 1, VerticalOptions = LayoutOptions.Center };
        Seek.SetThemeColor(Slider.MinimumTrackColorProperty, trackFill);
        Seek.SetThemeColor(Slider.MaximumTrackColorProperty, trackRest);
        Seek.SetThemeColor(Slider.ThumbColorProperty, trackFill);
        Seek.ValueChanged += (_, e) =>
        {
            if (!_syncing) _ = _player.SeekAsync(e.NewValue);
        };

        Progress = new ProgressBar { VerticalOptions = LayoutOptions.Center, IsVisible = false };
        Progress.SetThemeColor(ProgressBar.ProgressColorProperty, trackFill);
        _timeline.Add(Seek);
        _timeline.Add(Progress);
        FlexLayout.SetGrow(_timeline, 1);

        PlaysLabel = DisplayUi.NewLabel(11, text, FontAttributes.Bold, LineBreakMode.NoWrap);
        PlaysLabel.HorizontalOptions = LayoutOptions.Center;
        PlaysLabel.VerticalOptions = LayoutOptions.Center;
        PlaysBadge = new Border { StrokeThickness = 0, Padding = new Thickness(8, 2), VerticalOptions = LayoutOptions.Center, Content = PlaysLabel, IsVisible = false };
        PlaysBadge.SetRadius(999);
        PlaysBadge.SetThemeColor(BackgroundColorProperty, badgeFill);

        Mute = new MediaButton(MediaIcons.VolumeOn, texts.Mute, text);
        Mute.Clicked += (_, _) => _ = _player.ToggleMuteAsync();

        Volume = new Slider { Minimum = 0, Maximum = 1, VerticalOptions = LayoutOptions.Center };
        Volume.SetThemeColor(Slider.MinimumTrackColorProperty, trackFill);
        Volume.SetThemeColor(Slider.MaximumTrackColorProperty, trackRest);
        Volume.SetThemeColor(Slider.ThumbColorProperty, trackFill);
        Volume.ValueChanged += (_, e) =>
        {
            if (!_syncing) _ = _player.SetVolumeAsync(e.NewValue);
        };
        SemanticProperties.SetDescription(Volume, texts.Volume);

        RateButton = new DisplayTextAction(null, "1×", text);
        RateButton.Clicked += (_, _) => _ = _player.CycleRateAsync();

        Captions = new DisplayTextAction(null, "CC", text);
        Captions.Clicked += (_, _) => _cycleCaptions();

        Download = new MediaButton(DisplayIcons.Download, texts.Download, text);
        Download.Clicked += (_, _) => _ = _player.DownloadAsync();

        LockIcon = new KlyIcon { Name = DisplayIcons.Lock, Size = KlySize.Sm, IconColor = video ? White : KlyTheme.Resolve(KlyColor.Warning).Text, VerticalOptions = LayoutOptions.Center };
        LockedText = DisplayUi.NewLabel(12, text, FontAttributes.Bold, LineBreakMode.NoWrap);
        LockedText.VerticalOptions = LayoutOptions.Center;
        LockedTime = DisplayUi.NewLabel(12, text, FontAttributes.None, LineBreakMode.NoWrap);
        LockedTime.FontFamily = DisplayUi.MonoFont();
        LockedTime.VerticalOptions = LayoutOptions.Center;
        _locked.Add(LockIcon);
        _locked.Add(LockedText);
        _locked.Add(LockedTime);

        foreach (var view in new View[] { Play, Time, _timeline, PlaysBadge, Mute, Volume, RateButton, Captions, Download }) _controls.Add(view);
        Content = new Grid { Children = { _controls, _locked } };
        Refresh();
    }

    public MediaButton Play { get; }
    public Label Time { get; }
    public Slider Seek { get; }
    public ProgressBar Progress { get; }
    public Border PlaysBadge { get; }
    public Label PlaysLabel { get; }
    public MediaButton Mute { get; }
    public Slider Volume { get; }
    public DisplayTextAction RateButton { get; }
    public DisplayTextAction Captions { get; }
    public MediaButton Download { get; }
    public KlyIcon LockIcon { get; }
    public Label LockedText { get; }
    public Label LockedTime { get; }
    public FlexLayout Controls => _controls;
    public bool ShowsLocked => _locked.IsVisible;

    /// <summary>Redraws the bar from the state of the player.</summary>
    public void Refresh()
    {
        var texts = DisplayTexts.Current;
        var player = _player;
        var compact = player.Compact;
        var caps = player.Capabilities;
        var clock = $"{KlyMediaPlayerBase.FormatTime(player.ClipPosition)} / {KlyMediaPlayerBase.FormatTime(player.ClipLength)}";

        _controls.IsVisible = !player.IsLocked;
        _locked.IsVisible = player.IsLocked;
        LockedText.Text = texts.NoPlaysLeft;
        LockedTime.Text = clock;
        IsEnabled = player.IsEnabled;

        var gap = compact ? 4 : 6;
        Play.SetDescription(player.IsPlaying ? texts.Pause : texts.Play);
        Play.Icon.Name = player.IsPlaying ? KlyIconNames.Pause : KlyIconNames.Play;
        if (Play.Circle is { } circle)
        {
            var size = compact ? 28 : 32;
            circle.WidthRequest = size;
            circle.HeightRequest = size;
            circle.SetRadius(size / 2d);
        }

        var hasPosition = player.HasPosition;
        Time.Text = clock;
        Time.IsVisible = !player.IsHidden(MediaPlayerControl.Time) && hasPosition;
        _timeline.IsVisible = hasPosition;
        FlexLayout.SetBasis(_timeline, new Microsoft.Maui.Layouts.FlexBasis(compact ? 64f : 96f, false));

        var seekable = player.CanSeek;
        Seek.IsVisible = seekable;
        Progress.IsVisible = !seekable;
        var length = player.ClipLength;
        var position = player.ClipPosition;
        _syncing = true;
        try
        {
            Seek.Maximum = Math.Max(length, 0.1);
            Seek.Value = Math.Min(position, Seek.Maximum);
            Volume.Value = player.Volume;
        }
        finally
        {
            _syncing = false;
        }
        Progress.Progress = length > 0 ? Math.Clamp(position / length, 0, 1) : 0;
        var valueText = player.SeekValueText;
        SemanticProperties.SetDescription(Seek, texts.Seek);
        SemanticProperties.SetHint(Seek, valueText);
        SemanticProperties.SetDescription(Progress, texts.PlaybackProgress);
        SemanticProperties.SetHint(Progress, valueText);

        PlaysBadge.IsVisible = player.MaxPlays > 0;
        if (player.MaxPlays > 0)
        {
            PlaysLabel.Text = player.RemainingPlays.ToString(System.Globalization.CultureInfo.InvariantCulture);
            SemanticProperties.SetDescription(PlaysBadge, texts.PlaysLeft(player.RemainingPlays));
        }

        var showVolume = !player.IsHidden(MediaPlayerControl.Volume) && caps.HasFlag(MediaPlaybackCapabilities.Volume);
        Mute.IsVisible = showVolume;
        Volume.IsVisible = showVolume;
        Mute.Icon.Name = player.Muted ? MediaIcons.VolumeOff : MediaIcons.VolumeOn;
        Mute.SetDescription(player.Muted ? texts.Unmute : texts.Mute);
        Volume.WidthRequest = compact ? 56 : 80;

        var showRate = !player.IsHidden(MediaPlayerControl.Rate) && player.Rates is { Count: > 1 } && caps.HasFlag(MediaPlaybackCapabilities.Rate);
        RateButton.IsVisible = showRate;
        RateButton.SetText($"{player.Rate.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}×");
        RateButton.SetDescription(texts.PlaybackSpeed);
        SemanticProperties.SetHint(RateButton, RateButton.Text);

        Captions.IsVisible = _video && !player.IsHidden(MediaPlayerControl.Captions) && caps.HasFlag(MediaPlaybackCapabilities.Captions) && _hasCaptions();
        Captions.SetDescription(texts.Captions);
        SemanticProperties.SetHint(Captions, _captionsOn() ? "on" : "off");

        Download.IsVisible = player.ShowsDownload;
        Download.SetDescription(texts.Download);

        foreach (var view in new View[] { Play, Time, PlaysBadge, Mute, Volume, RateButton, Captions, Download }) view.Margin = new Thickness(0, 0, gap, 0);
    }
}
