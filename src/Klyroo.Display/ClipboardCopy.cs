using Microsoft.JSInterop;

namespace Klyroo.Display;

internal static class ClipboardCopy
{
    /// <summary>Writes the text to the clipboard. Nothing happens in static server rendering, where no script runs, and a browser that refuses the write is ignored.</summary>
    public static async Task WriteAsync(IJSRuntime js, bool interactive, string? text)
    {
        if (string.IsNullOrEmpty(text) || !interactive) return;
        try
        {
            await js.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
        catch (JSException)
        {
        }
    }
}
