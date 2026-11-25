namespace Klyroo.Display;

/// <summary>
/// A video frame with an overlay play button and a control bar (play and pause, timeline, mute, speed, captions and an optional
/// download link), that can restrict playback to a clip (<see cref="Start"/> and <see cref="End"/>), to a number of plays
/// (<see cref="MaxPlays"/>) and to a non-seekable timeline.
/// </summary>
/// <remarks>
/// In this version the component keeps the clip, the play count and the position in its own state and does not call
/// <c>play()</c> on the <c>video</c> element, nor does the position advance by itself. Use it to present and enforce the
/// rules, and drive the position from your own code with <see cref="SeekAsync"/>.
/// </remarks>
public partial class KlyVideoPlayer
{
}
