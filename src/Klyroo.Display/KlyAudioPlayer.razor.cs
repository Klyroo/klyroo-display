namespace Klyroo.Display;

/// <summary>
/// A compact audio control bar with play and pause, a timeline, a mute button and a volume slider, a speed button and an optional download link,
/// that can restrict playback to a clip (<see cref="Start"/> and <see cref="End"/>), to a number of plays (<see cref="MaxPlays"/>)
/// and to a non-seekable timeline.
/// </summary>
/// <remarks>
/// The component drives its <c>audio</c> element through a small JavaScript module once it is interactive: the buttons play,
/// pause and seek the media, and the position, the duration and the end of the clip come back from the element. The play
/// count and the lock are enforced by the component. When the player itself has the focus, Space or K toggles playback and
/// M toggles mute (see <see cref="Keyboard"/>). Under static server rendering nothing is interactive, so the
/// control bar shows but does not play.
/// </remarks>
public partial class KlyAudioPlayer
{
}
