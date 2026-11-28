namespace Klyroo.Display.Maui;

/// <summary>Turns the address or the path of an image into an <see cref="ImageSource"/>.</summary>
internal static class DisplayImages
{
    /// <summary>An address with a scheme (<c>https:</c>) downloads; anything else is a path or the name of a file of the app.</summary>
    public static ImageSource? From(string? src)
    {
        if (string.IsNullOrWhiteSpace(src)) return null;
        if (Uri.TryCreate(src, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https") return ImageSource.FromUri(uri);
        return ImageSource.FromFile(src);
    }
}
