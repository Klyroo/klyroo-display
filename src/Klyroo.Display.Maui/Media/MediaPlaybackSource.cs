namespace Klyroo.Display.Maui;

/// <summary>The file a player control asks an <see cref="IMediaPlayback"/> to load.</summary>
/// <param name="Src">The address or the path of the file.</param>
/// <param name="IsVideo"><see langword="true"/> for a video, <see langword="false"/> for an audio file.</param>
/// <param name="Poster">The poster image of a video, or the cover image of an audio file.</param>
/// <param name="Preload">How much of the file to load before the user plays it.</param>
/// <param name="Tracks">The caption tracks of a video.</param>
public sealed record MediaPlaybackSource(
    string Src,
    bool IsVideo,
    string? Poster = null,
    MediaPreload Preload = MediaPreload.Metadata,
    IReadOnlyList<VideoPlayerTrack>? Tracks = null);
