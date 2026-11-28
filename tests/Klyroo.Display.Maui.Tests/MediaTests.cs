using System.ComponentModel;

namespace Klyroo.Display.Maui.Tests;

public class MediaTests
{
    static MediaTests() => TestSupport.InstallDispatcher();

    private static (KlyAudioPlayer Player, FakePlayback Playback) Audio(Action<KlyAudioPlayer>? configure = null, MediaPlaybackCapabilities caps = MediaPlaybackCapabilities.All)
    {
        var fake = new FakePlayback { Capabilities = caps };
        var player = new KlyAudioPlayer { Src = "/a.mp3", Playback = fake };
        configure?.Invoke(player);
        return (player, fake);
    }

    private static (KlyVideoPlayer Player, FakePlayback Playback) Video(Action<KlyVideoPlayer>? configure = null, MediaPlaybackCapabilities caps = MediaPlaybackCapabilities.All)
    {
        var fake = new FakePlayback { Capabilities = caps };
        var player = new KlyVideoPlayer { Src = "/a.mp4", Playback = fake };
        configure?.Invoke(player);
        return (player, fake);
    }

    // --- formatting and the clip ----------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, "0:00")]
    [InlineData(37, "0:37")]
    [InlineData(60, "1:00")]
    [InlineData(59.6, "1:00")]
    [InlineData(3661, "1:01:01")]
    [InlineData(-5, "0:00")]
    [InlineData(double.NaN, "0:00")]
    [InlineData(double.PositiveInfinity, "0:00")]
    public void Time_is_minutes_and_seconds_and_hours_from_one_hour(double seconds, string expected)
    {
        Assert.Equal(expected, KlyMediaPlayerBase.FormatTime(seconds));
    }

    [Fact]
    public void Clip_defaults_to_a_minute_from_the_start()
    {
        var (player, _) = Audio();

        Assert.Equal(0, player.Start);
        Assert.Null(player.End);
        Assert.Equal(60, player.Duration);
        Assert.Equal(60, player.ClipEnd);
        Assert.Equal(60, player.ClipLength);
        Assert.Equal(0, player.ClipPosition);
    }

    [Fact]
    public void Clip_end_is_used_when_it_is_after_the_start_and_positions_are_relative_to_the_start()
    {
        var (player, _) = Audio(p => { p.Start = 10; p.End = 40; });

        Assert.Equal(40, player.ClipEnd);
        Assert.Equal(30, player.ClipLength);
        Assert.Equal(0, player.ClipPosition);
    }

    [Fact]
    public void Clip_end_at_or_before_the_start_gives_way_to_the_duration()
    {
        var (player, _) = Audio(p => { p.Start = 10; p.End = 10; p.Duration = 25; });

        Assert.Equal(25, player.ClipEnd);
        Assert.Equal(15, player.ClipLength);
    }

    [Fact]
    public void Clip_uses_the_duration_the_playback_reports()
    {
        var (player, fake) = Audio();

        fake.RaiseDuration(125);

        Assert.Equal(125, player.ClipEnd);
        Assert.Equal(125, player.ClipLength);
        Assert.Contains("0:00 / 2:05", TestSupport.Texts(player.Bar));
    }

    // --- defaults ---------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Player_defaults_match_the_blazor_players()
    {
        var player = new KlyAudioPlayer();

        Assert.Equal("", player.Src);
        Assert.Equal(MediaPreload.Metadata, player.Preload);
        Assert.False(player.Autoplay);
        Assert.False(player.Loop);
        Assert.False(player.Compact);
        Assert.True(player.Seekable);
        Assert.Equal(0, player.MaxPlays);
        Assert.Equal(0, player.Plays);
        Assert.Equal(int.MaxValue, player.RemainingPlays);
        Assert.False(player.IsLocked);
        Assert.False(player.IsPlaying);
        Assert.Equal(1, player.Rate);
        Assert.Equal([0.5, 0.75, 1, 1.25, 1.5, 2], player.Rates);
        Assert.Equal(1, player.Volume);
        Assert.False(player.Muted);
        Assert.False(player.Download);
        Assert.Null(player.DownloadUrl);
        Assert.Null(player.Hide);
        Assert.Null(player.Playback);
        Assert.IsType<LauncherMediaPlayback>(player.EffectivePlayback);
        Assert.True(player.IsEnabled);
    }

    [Fact]
    public void Video_player_defaults()
    {
        var player = new KlyVideoPlayer();

        Assert.Null(player.Poster);
        Assert.Null(player.Tracks);
        Assert.Equal(16d / 9d, player.AspectRatio);
        Assert.Equal(-1, player.ActiveTrackIndex);
        Assert.Equal(0, player.Plays);
    }

    // --- the default playback ---------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task The_default_playback_opens_the_file_with_the_platform_and_counts_one_play()
    {
        var opened = new List<string>();
        var player = new KlyAudioPlayer
        {
            Src = "https://x.test/a.mp3",
            MaxPlays = 1,
            Playback = new LauncherMediaPlayback(src =>
            {
                opened.Add(src);
                return Task.CompletedTask;
            }),
        };
        var played = 0;
        var limit = 0;
        player.Played += (_, _) => played++;
        player.LimitReached += (_, _) => limit++;

        await player.TogglePlayAsync();

        Assert.Equal(["https://x.test/a.mp3"], opened);
        Assert.Equal(1, played);
        Assert.Equal(1, player.Plays);
        Assert.False(player.IsPlaying);
        Assert.True(player.IsLocked);

        await player.TogglePlayAsync();

        Assert.Single(opened);
        Assert.Equal(1, limit);
    }

    [Fact]
    public async Task The_default_playback_reports_a_file_the_platform_cannot_open_as_not_played()
    {
        var player = new KlyAudioPlayer { Src = "/a.mp3", Playback = new LauncherMediaPlayback(_ => throw new InvalidOperationException("no app")) };
        var played = 0;
        player.Played += (_, _) => played++;

        await player.TogglePlayAsync();

        Assert.Equal(0, played);
        Assert.False(player.IsPlaying);
    }

    [Fact]
    public void The_default_playback_supports_nothing_and_the_bar_shows_only_play_plays_and_download()
    {
        var player = new KlyAudioPlayer { Src = "/a.mp3", Playback = new LauncherMediaPlayback(), MaxPlays = 3, Download = true };

        var capabilities = player.EffectivePlayback.Capabilities;
        Assert.Equal(MediaPlaybackCapabilities.None, capabilities);
        Assert.Null(player.EffectivePlayback.Surface);
        var bar = player.Bar;
        Assert.True(bar.Play.IsVisible);
        Assert.False(bar.Time.IsVisible);
        Assert.False(((View)bar.Seek.Parent).IsVisible);
        Assert.False(bar.Mute.IsVisible);
        Assert.False(bar.Volume.IsVisible);
        Assert.False(bar.RateButton.IsVisible);
        Assert.True(bar.PlaysBadge.IsVisible);
        Assert.True(bar.Download.IsVisible);
    }

    [Fact]
    public void A_player_without_a_playback_uses_the_launcher_one()
    {
        var player = new KlyAudioPlayer { Src = "/a.mp3" };

        Assert.IsType<LauncherMediaPlayback>(player.EffectivePlayback);
        Assert.Same(player.EffectivePlayback, player.EffectivePlayback);
        Assert.False(player.HasPosition);
    }

    [Fact]
    public async Task Launcher_playback_has_nothing_to_report_and_nothing_to_do_but_open()
    {
        var playback = new LauncherMediaPlayback(_ => Task.CompletedTask);
        playback.TimeChanged += (_, _) => { };
        playback.DurationChanged += (_, _) => { };
        playback.Ended += (_, _) => { };
        playback.PlayingChanged += (_, _) => { };
        playback.PlayFailed += (_, _) => { };

        await playback.OpenAsync(new MediaPlaybackSource("/a.mp3", false));
        await playback.PlayAsync(null);
        await playback.PauseAsync();
        await playback.SeekAsync(3);
        playback.Apply(new MediaPlaybackOptions(0, null, false, false, 1, 1, -1));
        playback.Dispose();
        Assert.Throws<ArgumentNullException>(() => new LauncherMediaPlayback(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => playback.OpenAsync(null!));
    }

    [Fact]
    public async Task Launcher_playback_without_a_file_cannot_play()
    {
        var playback = new LauncherMediaPlayback(_ => Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidOperationException>(() => playback.PlayAsync(null));
    }

    [Theory]
    [InlineData("https://x.test/a.mp3", true)]
    [InlineData("mailto:a@b.c", true)]
    [InlineData("/data/a.mp3", false)]
    [InlineData("file:///data/a.mp3", false)]
    [InlineData("a.mp3", false)]
    [InlineData(null, false)]
    public void The_launcher_tells_addresses_from_files_of_the_device(string? src, bool remote)
    {
        Assert.Equal(remote, DisplayLauncher.IsRemote(src));
    }

    [Fact]
    public async Task The_launcher_ignores_an_empty_source_and_a_platform_that_refuses()
    {
        Assert.False(await DisplayLauncher.OpenAsync("", _ => Task.CompletedTask));
        Assert.False(await DisplayLauncher.OpenAsync(null, _ => Task.CompletedTask));
        Assert.False(await DisplayLauncher.OpenAsync("/a.mp3", _ => throw new InvalidOperationException()));
        var opened = new List<string>();
        Assert.True(await DisplayLauncher.OpenAsync("/a.mp3", s =>
        {
            opened.Add(s);
            return Task.CompletedTask;
        }));
        Assert.Equal(["/a.mp3"], opened);
    }

    // --- playing through a playback ---------------------------------------------------------------------------------------------------------

    [Fact]
    public void The_player_loads_the_file_into_its_playback_and_pushes_its_state()
    {
        var (player, fake) = Audio(p => { p.Start = 5; p.End = 30; p.Preload = MediaPreload.Auto; });

        var source = fake.Opened[^1];
        Assert.Equal("/a.mp3", source.Src);
        Assert.False(source.IsVideo);
        Assert.Equal(MediaPreload.Auto, source.Preload);
        Assert.Equal(new MediaPlaybackOptions(5, 30, false, false, 1, 1, -1), fake.LastOptions);
        Assert.Same(fake, player.EffectivePlayback);
    }

    [Fact]
    public async Task Play_starts_a_play_at_the_clip_start_and_resume_does_not_spend_another()
    {
        var (player, fake) = Audio(p => { p.Start = 10; p.End = 40; });
        var played = 0;
        var paused = 0;
        var plays = new List<int>();
        player.Played += (_, _) => played++;
        player.Paused += (_, _) => paused++;
        player.PlaysChanged += (_, n) => plays.Add(n);

        await player.TogglePlayAsync();

        Assert.True(player.IsPlaying);
        Assert.Equal(1, player.Plays);
        Assert.Equal([10d], fake.Plays.Select(p => p!.Value));
        Assert.Equal(1, played);
        Assert.Equal([1], plays);
        Assert.Equal("Pause", SemanticProperties.GetDescription(player.Bar.Play));
        Assert.Equal("pause", player.Bar.Play.Icon.Name);

        await player.TogglePlayAsync();

        Assert.False(player.IsPlaying);
        Assert.Equal(1, paused);
        Assert.Equal(1, fake.Pauses);
        Assert.Equal("Play", SemanticProperties.GetDescription(player.Bar.Play));
        Assert.Equal("play", player.Bar.Play.Icon.Name);

        await player.TogglePlayAsync();

        Assert.True(player.IsPlaying);
        Assert.Equal(1, player.Plays);
        Assert.Equal(new double?[] { 10, null }, fake.Plays);
        Assert.Equal(2, played);
    }

    [Fact]
    public async Task Position_follows_the_playback_relative_to_the_start_and_is_reported()
    {
        var (player, fake) = Audio(p => { p.Start = 10; p.End = 40; });
        var progress = new List<MediaPlayerProgress>();
        player.TimeUpdated += (_, e) => progress.Add(e);

        fake.RaiseTime(20);
        Assert.Empty(progress);

        await player.TogglePlayAsync();
        fake.RaiseTime(25);

        Assert.Equal(15, player.ClipPosition);
        Assert.Equal(new MediaPlayerProgress(15, 30, 25), Assert.Single(progress));
        Assert.Contains("0:15 / 0:30", TestSupport.Texts(player.Bar));
        Assert.Equal(15, player.Bar.Seek.Value);
        Assert.Equal(30, player.Bar.Seek.Maximum);
        Assert.Empty(fake.Seeks);
    }

    [Fact]
    public async Task Reaching_the_clip_end_ends_the_play()
    {
        var (player, fake) = Audio(p => { p.Start = 10; p.End = 40; });
        var ended = 0;
        player.Ended += (_, _) => ended++;
        await player.TogglePlayAsync();

        await player.ReceiveTimeAsync(40);

        Assert.Equal(1, ended);
        Assert.False(player.IsPlaying);
        Assert.Equal(30, player.ClipPosition);
        Assert.Equal(1, fake.Pauses);
    }

    [Fact]
    public async Task The_end_of_the_file_ends_the_play_and_a_stray_end_does_not()
    {
        var (player, fake) = Audio();
        var ended = 0;
        player.Ended += (_, _) => ended++;

        await player.ReceiveEndedAsync();
        Assert.Equal(0, ended);

        await player.TogglePlayAsync();
        fake.RaiseEnded();

        Assert.Equal(1, ended);
        Assert.False(player.IsPlaying);
        Assert.Equal(60, player.ClipPosition);
    }

    [Fact]
    public async Task A_loop_starts_the_clip_again_without_a_new_play()
    {
        var (player, fake) = Audio(p => { p.Loop = true; p.Start = 5; p.End = 20; });
        var ended = 0;
        player.Ended += (_, _) => ended++;
        await player.TogglePlayAsync();
        Assert.True(fake.LastOptions!.Value.Loop);

        await player.ReceiveTimeAsync(20);
        await player.ReceiveEndedAsync();

        Assert.Equal(0, ended);
        Assert.True(player.IsPlaying);
        Assert.Equal(1, player.Plays);
        Assert.Equal(new double?[] { 5, 5, 5 }, fake.Plays);
    }

    [Fact]
    public void A_player_restricted_by_plays_or_a_fixed_timeline_never_loops()
    {
        var (limited, limitedFake) = Audio(p => { p.Loop = true; p.MaxPlays = 1; });
        Assert.False(limitedFake.LastOptions!.Value.Loop);
        Assert.True(limited.IsRestricted);

        var (fixedTimeline, fixedFake) = Audio(p => { p.Loop = true; p.Seekable = false; });
        Assert.False(fixedFake.LastOptions!.Value.Loop);
        Assert.True(fixedTimeline.IsRestricted);
    }

    [Fact]
    public async Task A_playback_that_cannot_start_leaves_the_player_paused()
    {
        var (player, fake) = Audio();
        fake.FailPlay = true;
        var played = 0;
        player.Played += (_, _) => played++;

        await player.TogglePlayAsync();

        Assert.False(player.IsPlaying);
        Assert.Equal(0, played);
    }

    [Fact]
    public async Task A_playback_reporting_a_failure_goes_back_to_paused()
    {
        var (player, fake) = Audio();
        await player.TogglePlayAsync();
        Assert.True(player.IsPlaying);

        fake.RaiseFailed();

        Assert.False(player.IsPlaying);
    }

    [Fact]
    public async Task Playback_started_or_stopped_outside_the_buttons_follows_the_same_rules()
    {
        var (player, fake) = Audio();

        fake.RaisePlaying(true);
        await Task.Yield();
        Assert.True(player.IsPlaying);
        Assert.Equal(1, player.Plays);

        fake.RaisePlaying(false);
        await Task.Yield();
        Assert.False(player.IsPlaying);
    }

    [Fact]
    public async Task Autoplay_starts_the_player_once()
    {
        var (player, fake) = Audio(p => p.Autoplay = true);

        await player.RunAutoplayAsync();
        await player.RunAutoplayAsync();

        Assert.Single(fake.Plays);
        Assert.True(player.IsPlaying);
    }

    [Fact]
    public async Task Without_autoplay_the_player_stays_paused()
    {
        var (player, fake) = Audio();

        await player.RunAutoplayAsync();

        Assert.Empty(fake.Plays);
    }

    // --- seeking ----------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Seek_moves_the_playback_inside_the_clip_and_reports_the_position()
    {
        var (player, fake) = Audio(p => { p.Start = 10; p.End = 40; });
        var progress = new List<MediaPlayerProgress>();
        player.TimeUpdated += (_, e) => progress.Add(e);

        await player.SeekAsync(12);

        Assert.Equal([22d], fake.Seeks);
        Assert.Equal(12, player.ClipPosition);
        Assert.Equal(new MediaPlayerProgress(12, 30, 22), Assert.Single(progress));
    }

    [Fact]
    public async Task Seek_is_clamped_to_the_clip()
    {
        var (player, fake) = Audio(p => { p.Start = 10; p.End = 40; p.Duration = 100; });

        await player.SeekAsync(-5);
        Assert.Equal(10, fake.Seeks[^1]);
        Assert.Equal(0, player.ClipPosition);

        await player.SeekAsync(500);
        Assert.Equal(40, fake.Seeks[^1]);
    }

    [Fact]
    public async Task Seeking_to_the_end_of_a_play_in_progress_ends_it()
    {
        var (player, fake) = Audio(p => { p.Start = 0; p.End = 30; });
        var ended = 0;
        player.Ended += (_, _) => ended++;
        await player.TogglePlayAsync();

        await player.SeekAsync(30);

        Assert.Equal(1, ended);
        Assert.False(player.IsPlaying);
        Assert.True(fake.Pauses >= 1);
    }

    [Fact]
    public async Task Seek_does_nothing_on_a_fixed_timeline_a_playback_that_cannot_seek_a_disabled_or_a_locked_player()
    {
        var (fixedTimeline, fixedFake) = Audio(p => p.Seekable = false);
        await fixedTimeline.SeekAsync(5);
        Assert.Empty(fixedFake.Seeks);
        Assert.False(fixedTimeline.CanSeek);

        var (noSeek, noSeekFake) = Audio(caps: MediaPlaybackCapabilities.Position);
        await noSeek.SeekAsync(5);
        Assert.Empty(noSeekFake.Seeks);

        var (disabled, disabledFake) = Audio(p => p.IsEnabled = false);
        await disabled.SeekAsync(5);
        Assert.Empty(disabledFake.Seeks);

        var (locked, lockedFake) = Audio(p => p.MaxPlays = 1);
        await locked.TogglePlayAsync();
        await locked.CompletePlayAsync();
        Assert.True(locked.IsLocked);
        await locked.SeekAsync(5);
        Assert.Empty(lockedFake.Seeks);
    }

    [Fact]
    public void The_slider_seeks_when_the_user_moves_it_but_not_when_the_player_moves_it()
    {
        var (player, fake) = Audio(p => { p.Start = 10; p.End = 40; });

        player.Bar.Seek.Value = 12;

        Assert.Equal([22d], fake.Seeks);
        Assert.Equal(12, player.ClipPosition);
    }

    // --- plays and the lock -----------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Plays_run_out_lock_the_player_and_report_the_limit()
    {
        var (player, fake) = Audio(p => p.MaxPlays = 2);
        var limit = 0;
        var ended = 0;
        player.LimitReached += (_, _) => limit++;
        player.Ended += (_, _) => ended++;

        await player.TogglePlayAsync();
        Assert.Equal(1, player.RemainingPlays);
        Assert.False(player.IsLocked);
        await player.CompletePlayAsync();
        Assert.Equal(1, ended);
        Assert.Equal(0, limit);
        Assert.False(player.IsLocked);

        await player.TogglePlayAsync();
        Assert.Equal(0, player.RemainingPlays);
        Assert.False(player.IsLocked);
        await player.CompletePlayAsync();
        Assert.Equal(2, ended);
        Assert.Equal(1, limit);
        Assert.True(player.IsLocked);

        var starts = fake.Plays.Count;
        await player.TogglePlayAsync();
        Assert.Equal(2, limit);
        Assert.Equal(starts, fake.Plays.Count);
        Assert.Equal(2, player.Plays);
    }

    [Fact]
    public async Task A_locked_player_shows_the_lock_instead_of_the_controls()
    {
        var (player, _) = Audio(p => { p.MaxPlays = 1; p.Start = 0; p.End = 20; });
        await player.TogglePlayAsync();
        await player.CompletePlayAsync();

        var bar = player.Bar;
        Assert.True(bar.ShowsLocked);
        Assert.False(bar.Controls.IsVisible);
        Assert.Equal("No plays left", bar.LockedText.Text);
        Assert.Equal("0:20 / 0:20", bar.LockedTime.Text);
        Assert.Equal("lock", bar.LockIcon.Name);
    }

    [Fact]
    public async Task The_plays_badge_counts_down_and_is_spoken()
    {
        var (player, _) = Audio(p => { p.MaxPlays = 3; p.Plays = 1; });

        var bar = player.Bar;
        Assert.True(bar.PlaysBadge.IsVisible);
        Assert.Equal("2", bar.PlaysLabel.Text);
        Assert.Equal("2 plays left", SemanticProperties.GetDescription(bar.PlaysBadge));

        await player.TogglePlayAsync();
        Assert.Equal("1", bar.PlaysLabel.Text);
        Assert.Equal("1 play left", SemanticProperties.GetDescription(bar.PlaysBadge));
    }

    [Fact]
    public async Task Changing_the_file_ends_the_play_without_spending_more_and_loads_the_new_file()
    {
        var (player, fake) = Audio(p => p.MaxPlays = 3);
        await player.TogglePlayAsync();
        await player.ReceiveTimeAsync(12);

        player.Src = "/b.mp3";

        Assert.False(player.IsPlaying);
        Assert.Equal(1, player.Plays);
        Assert.Equal(0, player.ClipPosition);
        Assert.Equal("/b.mp3", fake.Opened[^1].Src);

        await player.TogglePlayAsync();
        Assert.Equal(2, player.Plays);
    }

    [Fact]
    public void Plays_binds_two_way()
    {
        var source = new Source();
        var (player, _) = Audio(p =>
        {
            p.MaxPlays = 3;
            p.BindingContext = source;
            p.SetBinding(KlyMediaPlayerBase.PlaysProperty, nameof(Source.Plays), BindingMode.TwoWay);
        });

        source.Plays = 2;
        Assert.Equal(2, player.Plays);
        Assert.Equal("1", player.Bar.PlaysLabel.Text);

        _ = player.TogglePlayAsync();
        Assert.Equal(3, source.Plays);
    }

    // --- disabled ---------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task A_disabled_player_is_dimmed_ignores_play_and_pauses_what_plays()
    {
        var (player, fake) = Audio();
        await player.TogglePlayAsync();
        Assert.True(player.IsPlaying);

        player.IsEnabled = false;

        Assert.False(player.IsPlaying);
        Assert.Equal(0.6, player.Opacity);
        Assert.Equal(1, fake.Pauses);
        var plays = fake.Plays.Count;
        await player.TogglePlayAsync();
        Assert.Equal(plays, fake.Plays.Count);

        player.IsEnabled = true;
        Assert.Equal(1, player.Opacity);
    }

    // --- volume, mute and speed -------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Mute_toggles_raises_once_and_reaches_the_playback()
    {
        var (player, fake) = Audio();
        var raised = new List<bool>();
        player.MutedChanged += (_, v) => raised.Add(v);

        await player.ToggleMuteAsync();

        Assert.True(player.Muted);
        Assert.Equal([true], raised);
        Assert.True(fake.LastOptions!.Value.Muted);
        Assert.Equal("Unmute", SemanticProperties.GetDescription(player.Bar.Mute));
        Assert.Equal("volume-x", player.Bar.Mute.Icon.Name);

        await player.ToggleMuteAsync();
        Assert.Equal([true, false], raised);
        Assert.Equal("Mute", SemanticProperties.GetDescription(player.Bar.Mute));
        Assert.Equal("volume-2", player.Bar.Mute.Icon.Name);
    }

    [Fact]
    public async Task Volume_is_clamped_raised_once_per_change_and_pushed()
    {
        var (player, fake) = Audio();
        var raised = new List<double>();
        player.VolumeChanged += (_, v) => raised.Add(v);

        await player.SetVolumeAsync(0.5);
        await player.SetVolumeAsync(0.5);
        await player.SetVolumeAsync(7);
        await player.SetVolumeAsync(-1);
        await player.SetVolumeAsync(double.NaN);

        Assert.Equal([0.5, 1, 0], raised);
        Assert.Equal(0, player.Volume);
        Assert.Equal(0, fake.LastOptions!.Value.Volume);
        Assert.Equal(0, player.Bar.Volume.Value);
    }

    [Fact]
    public void The_volume_slider_changes_the_volume()
    {
        var (player, fake) = Audio();

        player.Bar.Volume.Value = 0.25;

        Assert.Equal(0.25, player.Volume);
        Assert.Equal(0.25, fake.LastOptions!.Value.Volume);
        Assert.Equal("Volume", SemanticProperties.GetDescription(player.Bar.Volume));
    }

    [Fact]
    public async Task Volume_and_mute_do_nothing_while_disabled()
    {
        var (player, _) = Audio(p => p.IsEnabled = false);

        await player.ToggleMuteAsync();
        await player.SetVolumeAsync(0.2);

        Assert.False(player.Muted);
        Assert.Equal(1, player.Volume);
    }

    [Fact]
    public async Task The_speed_button_cycles_through_the_rates_and_wraps()
    {
        var (player, fake) = Audio();
        var raised = new List<double>();
        player.RateChanged += (_, v) => raised.Add(v);

        await player.CycleRateAsync();
        await player.CycleRateAsync();
        await player.CycleRateAsync();
        await player.CycleRateAsync();

        Assert.Equal([1.25, 1.5, 2, 0.5], raised);
        Assert.Equal(0.5, fake.LastOptions!.Value.Rate);
        Assert.Equal("0.5×", player.Bar.RateButton.Text);
        Assert.Equal("Playback speed", SemanticProperties.GetDescription(player.Bar.RateButton));
    }

    [Fact]
    public async Task The_speed_button_needs_two_rates_and_starts_at_the_first_for_an_unknown_rate()
    {
        var (single, _) = Audio(p => p.Rates = [1]);
        await single.CycleRateAsync();
        Assert.Equal(1, single.Rate);
        Assert.False(single.Bar.RateButton.IsVisible);

        var (unknown, _) = Audio(p => { p.Rates = [1, 2]; p.Rate = 1.75; });
        await unknown.CycleRateAsync();
        Assert.Equal(1, unknown.Rate);

        var (none, _) = Audio(p => p.Rates = null);
        await none.CycleRateAsync();
        Assert.Equal(1, none.Rate);
        Assert.False(none.Bar.RateButton.IsVisible);
    }

    [Fact]
    public void Volume_muted_and_rate_bind_two_way()
    {
        var source = new Source();
        var (player, fake) = Audio(p =>
        {
            p.BindingContext = source;
            p.SetBinding(KlyMediaPlayerBase.VolumeProperty, nameof(Source.Volume), BindingMode.TwoWay);
            p.SetBinding(KlyMediaPlayerBase.MutedProperty, nameof(Source.Muted), BindingMode.TwoWay);
            p.SetBinding(KlyMediaPlayerBase.RateProperty, nameof(Source.Rate), BindingMode.TwoWay);
        });

        source.Volume = 0.3;
        source.Muted = true;
        source.Rate = 1.5;
        Assert.Equal(new MediaPlaybackOptions(0, null, false, true, 1.5, 0.3, -1), fake.LastOptions);

        player.Bar.Volume.Value = 0.8;
        _ = player.ToggleMuteAsync();
        _ = player.CycleRateAsync();
        Assert.Equal(0.8, source.Volume);
        Assert.False(source.Muted);
        Assert.Equal(2, source.Rate);
    }

    // --- the bar ----------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void The_bar_of_a_capable_playback_shows_time_timeline_volume_and_speed()
    {
        var (player, _) = Audio();

        var bar = player.Bar;
        Assert.True(bar.Time.IsVisible);
        Assert.Equal("0:00 / 1:00", bar.Time.Text);
        Assert.True(bar.Seek.IsVisible);
        Assert.False(bar.Progress.IsVisible);
        Assert.True(bar.Mute.IsVisible);
        Assert.True(bar.Volume.IsVisible);
        Assert.True(bar.RateButton.IsVisible);
        Assert.Equal("1×", bar.RateButton.Text);
        Assert.False(bar.PlaysBadge.IsVisible);
        Assert.False(bar.Download.IsVisible);
        Assert.False(bar.Captions.IsVisible);
        Assert.False(bar.ShowsLocked);
    }

    [Fact]
    public async Task A_fixed_timeline_is_a_read_only_progress_bar()
    {
        var (player, _) = Audio(p => { p.Seekable = false; p.End = 40; });
        await player.TogglePlayAsync();
        await player.ReceiveTimeAsync(10);

        var bar = player.Bar;
        Assert.False(bar.Seek.IsVisible);
        Assert.True(bar.Progress.IsVisible);
        Assert.Equal(0.25, bar.Progress.Progress, 3);
        Assert.Equal("Playback progress", SemanticProperties.GetDescription(bar.Progress));
        Assert.Equal("0:10 of 0:40", SemanticProperties.GetHint(bar.Progress));
    }

    [Fact]
    public async Task The_slider_is_named_and_speaks_the_position()
    {
        var (player, _) = Audio(p => p.End = 30);
        await player.TogglePlayAsync();
        await player.ReceiveTimeAsync(15);

        Assert.Equal("Seek", SemanticProperties.GetDescription(player.Bar.Seek));
        Assert.Equal("0:15 of 0:30", SemanticProperties.GetHint(player.Bar.Seek));
    }

    [Fact]
    public void Hide_removes_the_controls_it_names()
    {
        var (player, _) = Audio(p => p.Hide = [MediaPlayerControl.Time, MediaPlayerControl.Volume, MediaPlayerControl.Rate]);

        var bar = player.Bar;
        Assert.False(bar.Time.IsVisible);
        Assert.False(bar.Mute.IsVisible);
        Assert.False(bar.Volume.IsVisible);
        Assert.False(bar.RateButton.IsVisible);
        Assert.True(bar.Seek.IsVisible);
    }

    [Fact]
    public void The_bar_hides_what_the_playback_cannot_do()
    {
        var (player, _) = Audio(caps: MediaPlaybackCapabilities.Position);

        var bar = player.Bar;
        Assert.True(bar.Time.IsVisible);
        Assert.False(bar.Seek.IsVisible);
        Assert.True(bar.Progress.IsVisible);
        Assert.False(bar.Mute.IsVisible);
        Assert.False(bar.RateButton.IsVisible);
    }

    [Fact]
    public void Compact_makes_the_bar_denser()
    {
        var (player, _) = Audio();
        Assert.Equal(new Thickness(12, 6), player.Padding);
        Assert.Equal(32, player.Bar.Play.Circle!.WidthRequest);
        Assert.Equal(80, player.Bar.Volume.WidthRequest);

        player.Compact = true;

        Assert.Equal(new Thickness(8, 4), player.Padding);
        Assert.Equal(28, player.Bar.Play.Circle.WidthRequest);
        Assert.Equal(56, player.Bar.Volume.WidthRequest);
    }

    [Fact]
    public void The_buttons_are_touch_targets_and_the_bar_wraps()
    {
        var (player, _) = Audio();

        Assert.Equal(40, player.Bar.Play.WidthRequest);
        Assert.Equal(40, player.Bar.Play.HeightRequest);
        Assert.Equal(40, player.Bar.Mute.HeightRequest);
        Assert.Equal(40, player.Bar.Download.HeightRequest);
        Assert.True(player.Bar.RateButton.MinimumHeightRequest >= 40);
        Assert.Equal(Microsoft.Maui.Layouts.FlexWrap.Wrap, player.Bar.Controls.Wrap);
    }

    [Fact]
    public async Task Download_shows_with_the_flag_or_an_address_opens_it_and_raises_the_event()
    {
        var (player, _) = Audio(p => p.Download = true);
        var opened = new List<string>();
        var raised = 0;
        player.Opener = s =>
        {
            opened.Add(s);
            return Task.CompletedTask;
        };
        player.DownloadClicked += (_, _) => raised++;
        Assert.True(player.Bar.Download.IsVisible);
        Assert.Equal("Download", SemanticProperties.GetDescription(player.Bar.Download));

        player.Bar.Download.SendClicked();
        await Task.Yield();
        Assert.Equal(["/a.mp3"], opened);
        Assert.Equal(1, raised);

        player.Download = false;
        Assert.False(player.Bar.Download.IsVisible);
        player.DownloadUrl = "https://x.test/dl.mp3";
        Assert.True(player.Bar.Download.IsVisible);
        await player.DownloadAsync();
        Assert.Equal("https://x.test/dl.mp3", opened[^1]);

        player.Hide = [MediaPlayerControl.Download];
        Assert.False(player.Bar.Download.IsVisible);
    }

    [Fact]
    public void The_region_is_named_by_the_title_or_by_its_kind()
    {
        var (audio, _) = Audio();
        Assert.Equal("Audio player", SemanticProperties.GetDescription(audio));
        audio.Title = "Interview";
        Assert.Equal("Interview", SemanticProperties.GetDescription(audio));

        var (video, _) = Video();
        Assert.Equal("Video player", SemanticProperties.GetDescription(video));
    }

    [Fact]
    public void The_audio_player_is_a_pill_on_the_elevated_surface()
    {
        var (player, _) = Audio();

        Assert.Equal(1, player.StrokeThickness);
        Assert.Equal(999, ((Microsoft.Maui.Controls.Shapes.RoundRectangle)player.StrokeShape!).CornerRadius.TopLeft);
        Assert.Equal(LayoutOptions.Fill, player.HorizontalOptions);
    }

    [Fact]
    public void The_players_use_the_texts()
    {
        TestSupport.WithTexts(new DisplayTexts { Play = "Lire", Pause = "Pause", AudioPlayer = "Lecteur audio", NoPlaysLeft = "Plus de lectures", Seek = "Position" }, () =>
        {
            var (player, _) = Audio();
            Assert.Equal("Lire", SemanticProperties.GetDescription(player.Bar.Play));
            Assert.Equal("Lecteur audio", SemanticProperties.GetDescription(player));
            Assert.Equal("Position", SemanticProperties.GetDescription(player.Bar.Seek));
        });
    }

    [Fact]
    public void The_volume_icons_are_registered_once_when_the_set_lacks_them()
    {
        _ = new KlyAudioPlayer();

        Assert.True(KlyIcons.TryGet("volume-2", out var on));
        Assert.True(KlyIcons.TryGet("volume-x", out var off));
        Assert.NotEmpty(on);
        Assert.Equal(3, off.Length);
    }

    [Fact]
    public async Task Dropping_the_playback_for_another_detaches_the_old_one()
    {
        var (player, first) = Audio();
        var second = new FakePlayback();

        player.Playback = second;
        first.RaiseDuration(300);
        await player.TogglePlayAsync();

        Assert.Equal(60, player.ClipEnd);
        Assert.Single(second.Plays);
        Assert.Empty(first.Plays);
        Assert.Equal("/a.mp3", second.Opened[^1].Src);

        player.Playback = null;
        Assert.IsType<LauncherMediaPlayback>(player.EffectivePlayback);
    }

    // --- the video player -------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Video_asks_the_playback_for_a_video_with_its_poster_and_tracks()
    {
        var tracks = new[] { new VideoPlayerTrack("/en.vtt", "en", "English") };
        var (_, fake) = Video(p => { p.Poster = "/p.png"; p.Tracks = tracks; });

        var source = fake.Opened[^1];
        Assert.True(source.IsVideo);
        Assert.Equal("/a.mp4", source.Src);
        Assert.Same(tracks, source.Tracks);
    }

    [Fact]
    public void Video_selects_the_default_track_and_the_captions_button_cycles_through_the_tracks_and_off()
    {
        var (player, fake) = Video(p => p.Tracks =
        [
            new VideoPlayerTrack("/en.vtt", "en", "English"),
            new VideoPlayerTrack("/fr.vtt", "fr", "Français", Default: true),
            new VideoPlayerTrack("/de.vtt", "de", "Deutsch"),
        ]);

        Assert.Equal(1, player.ActiveTrackIndex);
        Assert.Equal(1, fake.LastOptions!.Value.Track);

        player.CycleCaptions();
        Assert.Equal(2, player.ActiveTrackIndex);
        player.CycleCaptions();
        Assert.Equal(-1, player.ActiveTrackIndex);
        Assert.Equal(-1, fake.LastOptions!.Value.Track);
        player.CycleCaptions();
        Assert.Equal(0, player.ActiveTrackIndex);
        Assert.Equal(0, fake.LastOptions!.Value.Track);
    }

    [Fact]
    public void Video_without_a_default_track_starts_with_the_captions_off_and_a_new_file_resets_the_choice()
    {
        var (player, _) = Video(p => p.Tracks = [new VideoPlayerTrack("/en.vtt", "en", "English"), new VideoPlayerTrack("/fr.vtt", "fr", "Français", Default: true)]);
        Assert.Equal(1, player.ActiveTrackIndex);

        player.CycleCaptions();
        Assert.Equal(-1, player.ActiveTrackIndex);

        player.Src = "/b.mp4";
        Assert.Equal(1, player.ActiveTrackIndex);

        // The choice is kept while it still names a track, and falls back to off when the new list is too short for it.
        player.Tracks = [new VideoPlayerTrack("/x.vtt", "xx", "X")];
        Assert.Equal(-1, player.ActiveTrackIndex);
        player.Tracks = null;
        Assert.Equal(-1, player.ActiveTrackIndex);
    }

    [Fact]
    public void Cycling_the_captions_without_tracks_does_nothing()
    {
        var (player, _) = Video();

        player.CycleCaptions();

        Assert.Equal(-1, player.ActiveTrackIndex);
    }

    [Fact]
    public void The_captions_button_needs_tracks_a_capable_playback_and_is_hideable()
    {
        var track = new VideoPlayerTrack("/en.vtt", "en", "English");
        var (none, _) = Video();
        Assert.False(none.Bar.Captions.IsVisible);

        var (tracked, _) = Video(p => p.Tracks = [track]);
        Assert.True(tracked.Bar.Captions.IsVisible);
        Assert.Equal("CC", tracked.Bar.Captions.Text);
        Assert.Equal("Subtitles", SemanticProperties.GetDescription(tracked.Bar.Captions));
        Assert.Equal("off", SemanticProperties.GetHint(tracked.Bar.Captions));
        tracked.Bar.Captions.SendClicked();
        Assert.Equal("on", SemanticProperties.GetHint(tracked.Bar.Captions));

        var (limited, _) = Video(p => p.Tracks = [track], MediaPlaybackCapabilities.All & ~MediaPlaybackCapabilities.Captions);
        Assert.False(limited.Bar.Captions.IsVisible);

        var (hidden, _) = Video(p => { p.Tracks = [track]; p.Hide = [MediaPlayerControl.Captions]; });
        Assert.False(hidden.Bar.Captions.IsVisible);
    }

    [Fact]
    public async Task The_video_overlay_shows_while_paused_and_the_big_button_plays()
    {
        var (player, fake) = Video();
        Assert.True(player.Overlay.IsVisible);
        Assert.True(player.StartButton.IsVisible);
        Assert.Equal("Play the video", SemanticProperties.GetDescription(player.StartButton));
        Assert.Equal(40, Math.Min(player.StartButton.WidthRequest, player.StartButton.HeightRequest) - 16);

        player.StartButton.SendClicked();
        await Task.Yield();

        Assert.True(player.IsPlaying);
        Assert.False(player.Overlay.IsVisible);
        Assert.Single(fake.Plays);
    }

    [Fact]
    public async Task The_video_overlay_shows_the_plays_left_and_then_the_lock()
    {
        var (player, _) = Video(p => { p.MaxPlays = 2; p.Start = 0; p.End = 20; });
        Assert.True(player.PlaysBadge.IsVisible);
        Assert.Equal("2 plays left", player.PlaysBadgeText.Text);
        Assert.False(player.LockedText.IsVisible);

        await player.TogglePlayAsync();
        await player.CompletePlayAsync();
        await player.TogglePlayAsync();
        await player.CompletePlayAsync();

        Assert.True(player.IsLocked);
        Assert.False(player.StartButton.IsVisible);
        Assert.False(player.PlaysBadge.IsVisible);
        Assert.True(player.LockedText.IsVisible);
        Assert.Equal("No plays left", player.LockedText.Text);
        Assert.True(player.Bar.ShowsLocked);
    }

    [Fact]
    public void A_disabled_video_player_cannot_be_started_from_the_big_button()
    {
        var (player, _) = Video(p => p.IsEnabled = false);

        Assert.False(player.StartButton.IsEnabled);
        Assert.Equal(0.6, player.Opacity);
    }

    [Fact]
    public void The_poster_shows_when_there_is_one()
    {
        var (player, _) = Video();
        Assert.False(player.PosterImage.IsVisible);

        player.Poster = "https://x.test/poster.jpg";
        Assert.True(player.PosterImage.IsVisible);
        Assert.IsType<UriImageSource>(player.PosterImage.Source);
        Assert.Equal(Aspect.AspectFit, player.PosterImage.Aspect);

        player.Poster = "poster.png";
        Assert.IsType<FileImageSource>(player.PosterImage.Source);

        player.Poster = null;
        Assert.False(player.PosterImage.IsVisible);
    }

    [Fact]
    public void The_picture_of_the_playback_is_drawn_in_the_frame()
    {
        var surface = new BoxView();
        var fake = new FakePlayback { Surface = surface };

        var player = new KlyVideoPlayer { Src = "/a.mp4", Playback = fake };

        Assert.Contains(surface, TestSupport.All<BoxView>(player));

        player.Playback = new FakePlayback();
        Assert.DoesNotContain(surface, TestSupport.All<BoxView>(player));
    }

    [Fact]
    public void The_frame_is_as_high_as_its_width_and_aspect_ratio_allow()
    {
        var (player, _) = Video();
        player.HorizontalOptions = LayoutOptions.Fill;

        ((IView)player).Arrange(new Rect(0, 0, 320, 400));

        Assert.Equal(180, player.HeightRequest, 1);

        player.AspectRatio = 2;
        Assert.Equal(160, player.HeightRequest, 1);
    }

    [Fact]
    public void The_video_frame_is_black_with_rounded_corners_and_a_control_bar_over_it()
    {
        var (player, _) = Video();

        Assert.Equal(Colors.Black, player.BackgroundColor);
        Assert.Equal(KlyTheme.Radius * 0.75, ((Microsoft.Maui.Controls.Shapes.RoundRectangle)player.StrokeShape!).CornerRadius.TopLeft);
        Assert.Equal(40, player.Bar.Play.WidthRequest);
    }

    [Fact]
    public async Task The_video_bar_plays_pauses_and_seeks_like_the_audio_bar()
    {
        var (player, fake) = Video(p => { p.Start = 2; p.End = 12; });

        player.Bar.Play.SendClicked();
        await Task.Yield();
        Assert.True(player.IsPlaying);

        await player.ReceiveTimeAsync(7);
        Assert.Equal(5, player.Bar.Seek.Value);

        player.Bar.Seek.Value = 8;
        Assert.Equal([10d], fake.Seeks);
    }

    [Fact]
    public void A_video_player_with_the_default_playback_shows_the_poster_and_a_play_button_that_opens_the_file()
    {
        var player = new KlyVideoPlayer { Src = "https://x.test/a.mp4" };

        Assert.IsType<LauncherMediaPlayback>(player.EffectivePlayback);
        Assert.True(player.StartButton.IsVisible);
        Assert.False(player.Bar.Time.IsVisible);
        Assert.False(player.Bar.Captions.IsVisible);
    }

    // --- helpers -----------------------------------------------------------------------------------------------------------------------------

    private sealed class Source : INotifyPropertyChanged
    {
        private int _plays;
        private double _volume = 1;
        private bool _muted;
        private double _rate = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Plays { get => _plays; set => Set(ref _plays, value); }

        public double Volume { get => _volume; set => Set(ref _volume, value); }

        public bool Muted { get => _muted; set => Set(ref _muted, value); }

        public double Rate { get => _rate; set => Set(ref _rate, value); }

        private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
