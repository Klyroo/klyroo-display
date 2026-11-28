namespace Klyroo.Display.Maui;

/// <summary>The icons of the players that the built-in set of <see cref="KlyIcons"/> lacks, registered once under their Lucide names.</summary>
internal static class MediaIcons
{
    public const string VolumeOn = "volume-2";
    public const string VolumeOff = "volume-x";

    private static bool _registered;

    public static void Ensure()
    {
        if (_registered) return;
        _registered = true;
        if (!KlyIcons.TryGet(VolumeOn, out _))
        {
            KlyIcons.Register(VolumeOn, "M11 5L6 9H2v6h4l5 4V5Z", "M15.54 8.46a5 5 0 0 1 0 7.07");
        }
        if (!KlyIcons.TryGet(VolumeOff, out _))
        {
            KlyIcons.Register(VolumeOff, "M11 5L6 9H2v6h4l5 4V5Z", "M22 9l-6 6", "M16 9l6 6");
        }
    }
}
