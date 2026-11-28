namespace Klyroo.Display.Maui;

/// <summary>
/// A block of source code with a label, syntax highlighting from <see cref="CodeHighlighter"/>, a copy button, optional line numbers and emphasized lines.
/// The code is drawn in a monospace font, each category of token in its color (the same tokens as the web block, in light and dark). Show a value that must stay hidden with <see cref="KlySecretBlock"/> instead.
/// </summary>
/// <remarks>The copy button writes <see cref="Code"/>, as given, to the clipboard of the device. The block scrolls sideways when a line is wider than the screen.</remarks>
public class KlyCodeBlock : Border
{
    /// <summary>Identifies <see cref="Code"/>.</summary>
    public static readonly BindableProperty CodeProperty = BindableProperty.Create(nameof(Code), typeof(string), typeof(KlyCodeBlock), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Language"/>.</summary>
    public static readonly BindableProperty LanguageProperty = BindableProperty.Create(nameof(Language), typeof(CodeLanguage?), typeof(KlyCodeBlock), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(KlyCodeBlock), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Highlight"/>.</summary>
    public static readonly BindableProperty HighlightProperty = BindableProperty.Create(nameof(Highlight), typeof(bool), typeof(KlyCodeBlock), true, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Copy"/>.</summary>
    public static readonly BindableProperty CopyProperty = BindableProperty.Create(nameof(Copy), typeof(bool), typeof(KlyCodeBlock), true, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="LineNumbers"/>.</summary>
    public static readonly BindableProperty LineNumbersProperty = BindableProperty.Create(nameof(LineNumbers), typeof(bool), typeof(KlyCodeBlock), false, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="HighlightLines"/>.</summary>
    public static readonly BindableProperty HighlightLinesProperty = BindableProperty.Create(nameof(HighlightLines), typeof(IReadOnlyList<CodeLineRange>), typeof(KlyCodeBlock), null, propertyChanged: Refresh);

    private readonly Microsoft.Maui.Controls.Label _title;
    private readonly DisplayIconButton _copy;
    private readonly DisplayCodeView _view = new();

    // The tokens of the last code, language and highlight setting: a change of another property does not scan a large code again.
    private string? _tokensSource;
    private CodeLanguage? _tokensLanguage;
    private bool _tokensHighlighted;
    private IReadOnlyList<CodeToken> _tokens = [];
    private bool _highlighted;

    /// <summary>Creates an empty code block: set <see cref="Code"/>.</summary>
    public KlyCodeBlock()
    {
        this.SetThemeColor(BackgroundColorProperty, KlyTheme.Background);
        this.SetStroke(KlyTheme.Border);
        StrokeThickness = 1;
        this.SetRadius(KlyTheme.RadiusOf(1));
        Padding = new Thickness(16);
        HorizontalOptions = LayoutOptions.Fill;

        _title = DisplayUi.NewLabel(12, KlyTheme.TextMuted, FontAttributes.Bold, LineBreakMode.TailTruncation);
        _title.TextTransform = TextTransform.Uppercase;
        _title.VerticalOptions = LayoutOptions.Center;
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.None);

        _copy = new DisplayIconButton(KlyIconNames.Copy, DisplayTexts.Current.CopyCode);
        _copy.Clicked += (_, _) => _ = DisplayClipboard.Copy(Code, Copier);

        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        header.Add(_title, 0);
        header.Add(_copy, 1);

        var pre = new Border { StrokeThickness = 0, Padding = new Thickness(12), HorizontalOptions = LayoutOptions.Fill };
        pre.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
        pre.SetRadius(KlyTheme.RadiusOf(1));
        pre.Content = _view;

        Content = new VerticalStackLayout { Spacing = 8, Children = { header, pre } };
        Update();
    }

    /// <summary>The source code to show. It is plain text: nothing in it is interpreted.</summary>
    public string? Code { get => (string?)GetValue(CodeProperty); set => SetValue(CodeProperty, value); }

    /// <summary>
    /// The language of the code: it selects the syntax highlighting, and it is the label of the block when <see cref="Label"/> is not set. <see langword="null"/> (the default) shows plain text.
    /// Get it from a name, an alias or a file extension with <see cref="CodeHighlighter.ResolveLanguage"/> or <see cref="CodeHighlighter.ResolveFileName"/>.
    /// </summary>
    public CodeLanguage? Language { get => (CodeLanguage?)GetValue(LanguageProperty); set => SetValue(LanguageProperty, value); }

    /// <summary>The label above the code. Without it, the name of <see cref="Language"/>, or the localized "Code".</summary>
    public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>Whether the code is highlighted according to <see cref="Language"/> (<see langword="true"/> by default). A code longer than <see cref="CodeHighlighter.MaxLength"/> characters or <see cref="CodeHighlighter.MaxLines"/> lines is always plain text.</summary>
    public bool Highlight { get => (bool)GetValue(HighlightProperty); set => SetValue(HighlightProperty, value); }

    /// <summary>Whether the block shows a copy button, which copies the code as given (<see langword="true"/> by default).</summary>
    public bool Copy { get => (bool)GetValue(CopyProperty); set => SetValue(CopyProperty, value); }

    /// <summary>Shows the number of each line in a gutter. The numbers are not part of the text that the copy button copies.</summary>
    public bool LineNumbers { get => (bool)GetValue(LineNumbersProperty); set => SetValue(LineNumbersProperty, value); }

    /// <summary>The lines to emphasize, counted from 1: a neutral fill and a marker of the primary color in the gutter. Lines past the end of the code are ignored.</summary>
    public IReadOnlyList<CodeLineRange>? HighlightLines { get => (IReadOnlyList<CodeLineRange>?)GetValue(HighlightLinesProperty); set => SetValue(HighlightLinesProperty, value); }

    /// <summary>The tokens that are drawn, for the tests.</summary>
    internal IReadOnlyList<CodeToken> Tokens => _tokens;

    /// <summary>Whether the code is drawn highlighted: a language is set, <see cref="Highlight"/> is on and the code is within the size caps.</summary>
    internal bool IsHighlighted => _highlighted;

    /// <summary>The label above the code, for the tests.</summary>
    internal Microsoft.Maui.Controls.Label TitleLabel => _title;

    /// <summary>The copy button, for the tests.</summary>
    internal DisplayIconButton CopyButton => _copy;

    /// <summary>The drawing of the code, for the tests.</summary>
    internal DisplayCodeView CodeView => _view;

    /// <summary>Writes the text instead of the clipboard of the platform; for the tests.</summary>
    internal Func<string, Task>? Copier { get; set; }

    private static void Refresh(BindableObject bindable, object oldValue, object newValue) => ((KlyCodeBlock)bindable).Update();

    private void Update()
    {
        var texts = DisplayTexts.Current;
        _title.Text = Label ?? (Language is { } language ? CodeHighlighter.Name(language) : texts.Code);
        _copy.IsVisible = Copy;
        _copy.SetDescription(texts.CopyCode);

        var raw = Code ?? "";
        var language2 = Language;
        var wantsHighlight = Highlight && CodeHighlighter.CanHighlight(raw, language2);
        if (!string.Equals(_tokensSource, raw, StringComparison.Ordinal) || _tokensLanguage != language2 || _tokensHighlighted != wantsHighlight)
        {
            _tokensSource = raw;
            _tokensLanguage = language2;
            _tokensHighlighted = wantsHighlight;
            _tokens = CodeHighlighter.Tokenize(raw, wantsHighlight ? language2 : null);
        }
        _highlighted = wantsHighlight;
        _view.Set(_tokens, LineNumbers, HighlightLines, wrap: false);
    }
}
