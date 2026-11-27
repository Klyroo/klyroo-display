namespace Klyroo.Display.Maui;

/// <summary>The state a player control pushes to its <see cref="IMediaPlayback"/> whenever one of these values changes.</summary>
/// <param name="Start">The clip start, in seconds of the file.</param>
/// <param name="End">The clip end, in seconds of the file, set only for an explicit end. The control stops the play itself when the position reaches it.</param>
/// <param name="Loop">Whether the playback restarts at <paramref name="Start"/> when it reaches the end. It is off for a player restricted by plays or by a non-seekable timeline.</param>
/// <param name="Muted">Whether the sound is off.</param>
/// <param name="Rate">The playback rate, <c>1</c> for the normal speed.</param>
/// <param name="Volume">The volume, from 0 to 1.</param>
/// <param name="Track">The index of the caption track to show, or <c>-1</c> for none.</param>
public readonly record struct MediaPlaybackOptions(double Start, double? End, bool Loop, bool Muted, double Rate, double Volume, int Track);
