namespace Klyroo.Display.Maui;

/// <summary>
/// A value that is masked until the user reveals it, such as an API key or a token, with a Reveal and Conceal toggle and a copy button. It is never highlighted.
/// </summary>
/// <remarks>While the value is masked the control holds the bullets and the last <see cref="VisibleChars"/> characters only, never the whole value. The copy button copies the whole secret, even while it is masked.</remarks>
public class KlySecretBlock : Border
{
    /// <summary>Identifies <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(KlySecretBlock), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(KlySecretBlock), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="VisibleChars"/>.</summary>
    public static readonly BindableProperty VisibleCharsProperty = BindableProperty.Create(nameof(VisibleChars), typeof(int), typeof(KlySecretBlock), 4, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Copy"/>.</summary>
    public static readonly BindableProperty CopyProperty = BindableProperty.Create(nameof(Copy), typeof(bool), typeof(KlySecretBlock), true, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="IsRevealed"/>.</summary>
    public static readonly BindableProperty IsRevealedProperty = BindableProperty.Create(nameof(IsRevealed), typeof(bool), typeof(KlySecretBlock), false, BindingMode.TwoWay, propertyChanged: Refresh);

    private readonly Microsoft.Maui.Controls.Label _title;
    private readonly Microsoft.Maui.Controls.Label _text;
    private readonly DisplayTextAction _toggle;
    private readonly DisplayIconButton _copy;

    /// <summary>Creates an empty secret block: set <see cref="Value"/>.</summary>
    public KlySecretBlock()
    {
        this.SetThemeColor(BackgroundColorProperty, KlyTheme.Background);
        this.SetStroke(KlyTheme.Border);
        StrokeThickness = 1;
        this.SetRadius(DisplayThemeExtensions.RadiusOf(1));
        Padding = new Thickness(16);
        HorizontalOptions = LayoutOptions.Fill;

        _title = DisplayUi.NewLabel(12, KlyTheme.TextMuted, FontAttributes.Bold, LineBreakMode.TailTruncation);
        _title.TextTransform = TextTransform.Uppercase;
        _title.VerticalOptions = LayoutOptions.Center;

        _toggle = new DisplayTextAction(KlyIconNames.Eye, DisplayTexts.Current.Reveal);
        _toggle.Clicked += (_, _) => IsRevealed = !IsRevealed;

        _copy = new DisplayIconButton(KlyIconNames.Copy, DisplayTexts.Current.CopyCode);
        _copy.Clicked += (_, _) => _ = DisplayClipboard.Copy(Value, Copier);

        var actions = new HorizontalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End, Children = { _toggle, _copy } };
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        header.Add(_title, 0);
        header.Add(actions, 1);

        _text = DisplayUi.NewLabel(12, KlyTheme.TextHighlighted, FontAttributes.None, LineBreakMode.CharacterWrap);
        _text.FontFamily = DisplayUi.MonoFont();
        var pre = new Border { StrokeThickness = 0, Padding = new Thickness(12), HorizontalOptions = LayoutOptions.Fill, Content = _text };
        pre.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
        pre.SetRadius(DisplayThemeExtensions.RadiusOf(1));

        Content = new VerticalStackLayout { Spacing = 8, Children = { header, pre } };
        Update();
    }

    /// <summary>The secret, such as an API key or a token. While it is masked the control shows only its last <see cref="VisibleChars"/> characters.</summary>
    public string? Value { get => (string?)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    /// <summary>The label above the value. Without it, the localized "Secret".</summary>
    public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>
    /// The number of trailing characters left visible while the value is masked (<c>4</c> by default). A longer value shows eight bullets followed by those characters.
    /// A value that is not longer than this number is fully masked, so a short secret is never shown whole.
    /// </summary>
    public int VisibleChars { get => (int)GetValue(VisibleCharsProperty); set => SetValue(VisibleCharsProperty, value); }

    /// <summary>Whether the block shows a copy button, which copies the whole secret even while it is masked (<see langword="true"/> by default).</summary>
    public bool Copy { get => (bool)GetValue(CopyProperty); set => SetValue(CopyProperty, value); }

    /// <summary>Whether the value is shown in full. The Reveal and Conceal button toggles it; two-way bindable. The Blazor block keeps this state inside.</summary>
    public bool IsRevealed { get => (bool)GetValue(IsRevealedProperty); set => SetValue(IsRevealedProperty, value); }

    /// <summary>The label above the value, for the tests.</summary>
    internal Microsoft.Maui.Controls.Label TitleLabel => _title;

    /// <summary>The text that is drawn: the masked or the revealed value.</summary>
    internal string? Shown => _text.Text;

    /// <summary>The Reveal and Conceal button, for the tests.</summary>
    internal DisplayTextAction ToggleButton => _toggle;

    /// <summary>The copy button, for the tests.</summary>
    internal DisplayIconButton CopyButton => _copy;

    /// <summary>Writes the text instead of the clipboard of the platform; for the tests.</summary>
    internal Func<string, Task>? Copier { get; set; }

    /// <summary>The text shown while a value is masked: eight bullets and its last <paramref name="visible"/> characters, or bullets only for a value that is not longer.</summary>
    internal static string Mask(string raw, int visible)
    {
        if (string.IsNullOrEmpty(raw)) return "••••••••";
        if (visible <= 0 || raw.Length <= visible) return new string('•', Math.Max(8, raw.Length));
        return new string('•', 8) + raw[^visible..];
    }

    private static void Refresh(BindableObject bindable, object oldValue, object newValue) => ((KlySecretBlock)bindable).Update();

    private void Update()
    {
        var texts = DisplayTexts.Current;
        _title.Text = Label ?? texts.Secret;
        _text.Text = IsRevealed ? Value ?? "" : Mask(Value ?? "", VisibleChars);
        _toggle.Icon.Name = IsRevealed ? KlyIconNames.EyeOff : KlyIconNames.Eye;
        _toggle.SetText(IsRevealed ? texts.Conceal : texts.Reveal);
        _copy.IsVisible = Copy;
        _copy.SetDescription(texts.CopyCode);
    }
}
