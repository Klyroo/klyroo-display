namespace Klyroo.Display.Maui;

/// <summary>Small building blocks shared by the controls of this library.</summary>
internal static class DisplayUi
{
    /// <summary>The text color of punctuation in code: the <c>text-toned</c> token of Klyroo.UI, which <see cref="KlyTheme"/> does not expose.</summary>
    public static KlyThemeColor TextToned { get; } = KlyThemeColor.FromHex("#374151", "#cbd5e1");

    /// <summary>The fill of the primary color.</summary>
    public static KlyThemeColor PrimaryFill => KlyTheme.Resolve(KlyColor.Primary).Fill;

    /// <summary>The text color of the primary color (<c>--klyroo-primary-text</c> on the web).</summary>
    public static KlyThemeColor PrimaryText => KlyTheme.Resolve(KlyColor.Primary).Text;

    /// <summary>The text color on the primary fill (<c>--klyroo-primary-contrast</c> on the web).</summary>
    public static KlyThemeColor PrimaryContrast => KlyTheme.Resolve(KlyColor.Primary).Contrast;

    /// <summary>A monospace font family of the platform. The generic family name when the platform cannot be told.</summary>
    public static string MonoFont()
    {
        try
        {
            var platform = DeviceInfo.Platform;
            if (platform == DevicePlatform.iOS || platform == DevicePlatform.MacCatalyst) return "Menlo";
            if (platform == DevicePlatform.WinUI) return "Consolas";
        }
        catch (Exception)
        {
            // Not running on a platform (unit tests): the generic family name.
        }
        return "monospace";
    }

    /// <summary>A label with a size and a color that follows the app theme.</summary>
    public static Label NewLabel(double size, KlyThemeColor color, FontAttributes attributes = FontAttributes.None, LineBreakMode lineBreak = LineBreakMode.WordWrap)
    {
        var label = new Label { FontSize = size, FontAttributes = attributes, LineBreakMode = lineBreak };
        label.SetThemeColor(Label.TextColorProperty, color);
        return label;
    }

    /// <summary>Sets a text and hides the label when there is none.</summary>
    public static void Show(Label label, string? text)
    {
        label.Text = text;
        label.IsVisible = !string.IsNullOrEmpty(text);
    }

    /// <summary>Calls <paramref name="action"/> when the view is tapped while it is enabled.</summary>
    public static void Tap(View view, Action action)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (view.IsEnabled) action();
        };
        view.GestureRecognizers.Add(tap);
    }

    /// <summary>Takes a view out of the view that holds it, so that it can be put in another one.</summary>
    public static void Detach(View? view)
    {
        switch (view?.Parent)
        {
            case Layout layout:
                layout.Remove(view);
                break;
            case Border border:
                border.Content = null;
                break;
            case ContentView contentView:
                contentView.Content = null;
                break;
        }
    }

    /// <summary>The dimming of a disabled control: <c>disabled:opacity-60</c> on the web.</summary>
    public static double OpacityOf(bool enabled) => enabled ? 1 : 0.6;

    /// <summary>A line of one device-independent unit, such as the dividers of a list.</summary>
    public static Border Divider(KlyThemeColor color)
    {
        var line = new Border { StrokeThickness = 0, HeightRequest = 1, HorizontalOptions = LayoutOptions.Fill };
        line.SetThemeColor(VisualElement.BackgroundColorProperty, color);
        AutomationProperties.SetIsInAccessibleTree(line, false);
        return line;
    }

    /// <summary>Runs an action on the UI thread: at once when the caller already is on it, or when no dispatcher can be found (unit tests).</summary>
    public static void Post(BindableObject owner, Action action)
    {
        IDispatcher? dispatcher = null;
        try
        {
            dispatcher = (owner as Element)?.Dispatcher;
        }
        catch (Exception)
        {
            // No application, no dispatcher: run inline.
        }
        if (dispatcher is { IsDispatchRequired: true }) dispatcher.Dispatch(action);
        else action();
    }
}
