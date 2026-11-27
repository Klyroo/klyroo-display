namespace Klyroo.Display.Maui;

/// <summary>A tap target with an icon in the middle: the copy button of the blocks. It is 40 units wide and high, as a touch target must be.</summary>
internal sealed class DisplayIconButton : Border
{
    public DisplayIconButton(string iconName, string description, KlyThemeColor? color = null)
    {
        StrokeThickness = 0;
        Padding = 0;
        WidthRequest = 40;
        HeightRequest = 40;
        BackgroundColor = Colors.Transparent;
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Center;
        Icon = new KlyIcon { Name = iconName, Size = KlySize.Sm, IconColor = color ?? KlyTheme.TextMuted, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        Content = Icon;
        SemanticProperties.SetDescription(this, description);
        DisplayUi.Tap(this, () => Clicked?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? Clicked;

    public KlyIcon Icon { get; }

    public void SendClicked()
    {
        if (IsEnabled) Clicked?.Invoke(this, EventArgs.Empty);
    }

    public void SetDescription(string description) => SemanticProperties.SetDescription(this, description);
}
