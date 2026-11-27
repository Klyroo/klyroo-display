namespace Klyroo.Display.Maui;

/// <summary>Opens a file or an address with the platform: <see cref="Launcher"/> hands it to the browser or to the app registered for it.</summary>
internal static class DisplayLauncher
{
    /// <summary>
    /// Opens <paramref name="src"/>: an absolute address (<c>https:</c>, <c>mailto:</c>...) as it is, a path or <c>file:</c> address of the device as a file.
    /// Nothing happens for an empty value, and when the platform cannot open it: the caller has nowhere to report it, so the page that holds the control keeps working.
    /// </summary>
    /// <param name="src">The address or path.</param>
    /// <param name="opener">Opens the address instead of the platform; for tests.</param>
    /// <returns>True when the platform accepted it.</returns>
    public static async Task<bool> OpenAsync(string? src, Func<string, Task>? opener = null)
    {
        if (string.IsNullOrWhiteSpace(src)) return false;
        try
        {
            await (opener ?? OpenWithPlatform)(src);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Whether the value is an address that is not a file of the device.</summary>
    public static bool IsRemote(string? src) =>
        Uri.TryCreate(src, UriKind.Absolute, out var uri) && uri.Scheme != Uri.UriSchemeFile;

    private static async Task OpenWithPlatform(string src)
    {
        if (IsRemote(src))
        {
            await Launcher.Default.OpenAsync(new Uri(src));
            return;
        }
        var path = Uri.TryCreate(src, UriKind.Absolute, out var file) && file.IsFile ? file.LocalPath : src;
        await Launcher.Default.OpenAsync(new OpenFileRequest(Path.GetFileName(path), new ReadOnlyFile(path)));
    }
}
