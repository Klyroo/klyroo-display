namespace Klyroo.Display;

/// <summary>
/// A video frame with an overlay play button and a control bar (play and pause, timeline, mute and volume, speed, captions,
/// picture-in-picture, fullscreen and an optional download link), that can restrict playback to a clip (<see cref="Start"/> and <see cref="End"/>), to a number of plays
/// (<see cref="MaxPlays"/>) and to a non-seekable timeline.
/// </summary>
/// <remarks>
/// The component drives its <c>video</c> element through a small JavaScript module once it is interactive: the buttons play,
/// pause and seek the media, and the position, the duration and the end of the clip come back from the element. The play
/// count and the lock are enforced by the component. When the player itself has the focus, Space or K toggles playback and
/// M toggles mute (see <see cref="Keyboard"/>). The picture-in-picture and fullscreen buttons appear once the module reports that
/// the browser supports them. Under static server rendering nothing is interactive, so the control bar shows but does not play.
/// </remarks>
public partial class KlyVideoPlayer
{
}
