namespace Klyroo.Display.Maui;

/// <summary>A borderless action with an optional icon and a text, such as "Reveal" or "Show more": at least 40 units high, as a touch target must be.</summary>
internal sealed class DisplayTextAction : Border
{
    private readonly KlyIcon _icon;
    private readonly Label _label;
    private bool _described;

    public DisplayTextAction(string? iconName, string? text, KlyThemeColor? color = null, bool iconAfter = false)
    {
        var tone = color ?? KlyTheme.TextMuted;
        StrokeThickness = 0;
        Padding = new Thickness(8, 0);
        MinimumHeightRequest = 40;
        BackgroundColor = Colors.Transparent;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        this.SetRadius(KlyTheme.RadiusOf(1));

        _icon = new KlyIcon { Name = iconName ?? "", Size = KlySize.Sm, IconColor = tone, VerticalOptions = LayoutOptions.Center, IsVisible = iconName is not null };
        _label = DisplayUi.NewLabel(12, tone, FontAttributes.None, LineBreakMode.TailTruncation);
        _label.VerticalOptions = LayoutOptions.Center;
        _label.Text = text;
        var row = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        if (iconAfter)
        {
            row.Add(_label);
            row.Add(_icon);
        }
        else
        {
            row.Add(_icon);
            row.Add(_label);
        }
        Content = row;
        SemanticProperties.SetDescription(this, text);
        DisplayUi.Tap(this, () => Clicked?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? Clicked;

    public KlyIcon Icon => _icon;

    public Label Label => _label;

    public string? Text => _label.Text;

    /// <summary>Sets the text. A screen reader says the text too, unless <see cref="SetDescription"/> gave the action its own name.</summary>
    public void SetText(string? text)
    {
        _label.Text = text;
        if (!_described) SemanticProperties.SetDescription(this, text);
    }

    /// <summary>Names the action for a screen reader, whatever its text is.</summary>
    public void SetDescription(string? description)
    {
        _described = true;
        SemanticProperties.SetDescription(this, description);
    }

    public void SendClicked()
    {
        if (IsEnabled) Clicked?.Invoke(this, EventArgs.Empty);
    }
}
