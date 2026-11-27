namespace Klyroo.Display.Maui;

/// <summary>
/// A compact audio control bar with play and pause, a timeline, a mute button and a volume slider, a speed button and an optional download button,
/// that can restrict playback to a clip (<see cref="KlyMediaPlayerBase.Start"/> and <see cref="KlyMediaPlayerBase.End"/>), to a number of plays (<see cref="KlyMediaPlayerBase.MaxPlays"/>)
/// and to a non-seekable timeline.
/// </summary>
/// <remarks>
/// MAUI has no audio player control: this one draws the transport and plays through the <see cref="IMediaPlayback"/> you set as <see cref="KlyMediaPlayerBase.Playback"/>.
/// Without one, the play button opens the file with the platform (<see cref="LauncherMediaPlayback"/>) and the bar shows only the play button, the plays left and the download button.
/// </remarks>
public class KlyAudioPlayer : KlyMediaPlayerBase
{
    private readonly MediaControlBar _bar;

    /// <summary>Creates an audio player: set <see cref="KlyMediaPlayerBase.Src"/> and, to play inside the app, <see cref="KlyMediaPlayerBase.Playback"/>.</summary>
    public KlyAudioPlayer()
    {
        this.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundElevated);
        this.SetStroke(KlyTheme.Border);
        StrokeThickness = 1;
        this.SetRadius(999);
        _bar = new MediaControlBar(this, video: false, () => false, () => { }, () => false);
        Content = _bar;
        OnStateChanged();
    }

    internal override bool IsVideo => false;

    /// <summary>The control bar, for the tests.</summary>
    internal MediaControlBar Bar => _bar;

    private protected override MediaPlaybackSource SourceFor() => new(Src, IsVideo: false, null, Preload, null);

    private protected override void OnStateChanged()
    {
        if (_bar is null) return;
        Padding = Compact ? new Thickness(8, 4) : new Thickness(12, 6);
        SemanticProperties.SetDescription(this, Title ?? DisplayTexts.Current.AudioPlayer);
        _bar.Refresh();
    }
}
