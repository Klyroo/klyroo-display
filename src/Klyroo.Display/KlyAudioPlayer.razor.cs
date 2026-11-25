namespace Klyroo.Display;

/// <summary>
/// A compact audio control bar with play and pause, a timeline, a mute button, a speed button and an optional download link,
/// that can restrict playback to a clip (<see cref="Start"/> and <see cref="End"/>), to a number of plays (<see cref="MaxPlays"/>)
/// and to a non-seekable timeline.
/// </summary>
/// <remarks>
/// In this version the component keeps the clip, the play count and the position in its own state and does not call
/// <c>play()</c> on the <c>audio</c> element, nor does the position advance by itself. Use it to present and enforce
/// the rules, and drive the position from your own code with <see cref="SeekAsync"/> and <see cref="CompletePlayAsync"/>.
/// </remarks>
public partial class KlyAudioPlayer
{
}
