namespace Klyroo.Display.Maui;

/// <summary>
/// A round button of a player bar: a 40 by 40 touch target with an icon in the middle, drawn on a filled circle when <c>circle</c> is set.
/// </summary>
internal sealed class MediaButton : Border
{
    private readonly KlyIcon _icon;

    public MediaButton(string iconName, string description, KlyThemeColor iconColor, double iconScale = 1, double circleSize = 0, KlyThemeColor? circleFill = null)
    {
        StrokeThickness = 0;
        Padding = 0;
        WidthRequest = 40;
        HeightRequest = 40;
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        HorizontalOptions = LayoutOptions.Start;

        _icon = new KlyIcon { Name = iconName, Size = KlySize.Sm, IconColor = iconColor, Scale = iconScale, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        if (circleSize > 0 && circleFill is { } fill)
        {
            var circle = new Border { StrokeThickness = 0, Padding = 0, WidthRequest = circleSize, HeightRequest = circleSize, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Content = _icon };
            circle.SetRadius(circleSize / 2);
            circle.SetThemeColor(BackgroundColorProperty, fill);
            Circle = circle;
            Content = circle;
        }
        else
        {
            Content = _icon;
        }
        SemanticProperties.SetDescription(this, description);
        DisplayUi.Tap(this, () => Clicked?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? Clicked;

    public KlyIcon Icon => _icon;

    /// <summary>The filled circle behind the icon, when there is one.</summary>
    public Border? Circle { get; }

    public void SetDescription(string description) => SemanticProperties.SetDescription(this, description);

    public void SendClicked()
    {
        if (IsEnabled) Clicked?.Invoke(this, EventArgs.Empty);
    }
}
