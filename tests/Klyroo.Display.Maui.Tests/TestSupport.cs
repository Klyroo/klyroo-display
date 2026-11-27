using System.Reflection;

namespace Klyroo.Display.Maui.Tests;

internal static class TestSupport
{
    private static readonly object Gate = new();
    private static bool _installed;

    /// <summary>A binding needs a dispatcher to update its source; a test has no UI thread, so this one runs the work where it is called.</summary>
    public static void InstallDispatcher()
    {
        lock (Gate)
        {
            if (_installed) return;
            _installed = true;
            DispatcherProvider.SetCurrent(new InlineDispatcherProvider());
        }
    }

    public static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static IEnumerable<object> Walk(object root)
    {
        yield return root;
        IEnumerable<object?> kids = root switch
        {
            Layout layout => layout.Children.Cast<object?>(),
            Border border => [border.Content],
            ContentView content => [content.Content],
            ScrollView scroll => [scroll.Content],
            _ => [],
        };
        foreach (var kid in kids)
        {
            if (kid is null) continue;
            foreach (var descendant in Walk(kid)) yield return descendant;
        }
    }

    public static List<T> All<T>(object root) => Walk(root).OfType<T>().ToList();

    public static List<string> Texts(object root) => All<Label>(root).Where(l => l.IsVisible && !string.IsNullOrEmpty(l.Text)).Select(l => l.Text!).ToList();

    public static void Click(Button button) => ((IButtonController)button).SendClicked();

    // TapGestureRecognizer.SendTapped is internal: this is what a platform tap calls.
    public static void Tap(View view)
    {
        var tap = view.GestureRecognizers.OfType<TapGestureRecognizer>().First();
        var send = typeof(TapGestureRecognizer).GetMethod("SendTapped", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
        send.Invoke(tap, send.GetParameters().Select(p => p.ParameterType == typeof(View) ? view : (object?)null).ToArray());
    }

    /// <summary>Runs a test body with other texts, and puts the English ones back.</summary>
    public static void WithTexts(DisplayTexts texts, Action body)
    {
        var previous = DisplayTexts.Current;
        DisplayTexts.Current = texts;
        try
        {
            body();
        }
        finally
        {
            DisplayTexts.Current = previous;
        }
    }

    private sealed class InlineDispatcherProvider : IDispatcherProvider
    {
        public IDispatcher GetForCurrentThread() => InlineDispatcher.Instance;
    }

    private sealed class InlineDispatcher : IDispatcher
    {
        public static InlineDispatcher Instance { get; } = new();

        public bool IsDispatchRequired => false;

        public bool Dispatch(Action action)
        {
            action();
            return true;
        }

        public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException();

        public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
    }
}
