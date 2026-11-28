namespace Klyroo.Display.Maui;

/// <summary>
/// Draws the tokens of a code in a monospace font, each category in its color: one <see cref="Label"/> with a span per token, or, with line numbers or emphasized lines,
/// one row per line (a marker, a gutter and the text). It scrolls sideways, or wraps long lines when <see cref="Wrap"/> is on.
/// </summary>
internal sealed class DisplayCodeView : ContentView
{
    private IReadOnlyList<CodeToken> _tokens = [];
    private bool _lineNumbers;
    private IReadOnlyList<CodeLineRange>? _highlightLines;
    private bool _wrap;

    public DisplayCodeView()
    {
        HorizontalOptions = LayoutOptions.Fill;
    }

    public IReadOnlyList<CodeToken> Tokens => _tokens;

    public bool UsesRows { get; private set; }

    public bool Wrap => _wrap;

    /// <summary>The label of a code shown as a single block (no rows), for the tests.</summary>
    public Label? BlockLabel { get; private set; }

    /// <summary>The rows of the code, for the tests: one grid per line.</summary>
    public IReadOnlyList<Grid> Rows { get; private set; } = [];

    /// <summary>The numbers of the emphasized lines that are drawn, for the tests.</summary>
    public IReadOnlyList<int> HighlightedLines { get; private set; } = [];

    /// <summary>The label of the gutter of each row (empty without line numbers), for the tests.</summary>
    public IReadOnlyList<Label> Gutters { get; private set; } = [];

    public void Set(IReadOnlyList<CodeToken> tokens, bool lineNumbers, IReadOnlyList<CodeLineRange>? highlightLines, bool wrap)
    {
        _tokens = tokens;
        _lineNumbers = lineNumbers;
        _highlightLines = highlightLines;
        _wrap = wrap;
        Rebuild();
    }

    public static bool IsHighlighted(IReadOnlyList<CodeLineRange>? ranges, int line)
    {
        if (ranges is null) return false;
        foreach (var range in ranges)
        {
            if (range.Contains(line)) return true;
        }
        return false;
    }

    private void Rebuild()
    {
        var rows = _lineNumbers || (_highlightLines is { Count: > 0 });
        UsesRows = rows;
        BlockLabel = null;
        Rows = [];
        HighlightedLines = [];
        Gutters = [];
        View body;
        if (!rows)
        {
            var label = NewCodeLabel();
            label.FormattedText = Format(DisplayCodeLines.WithoutFinalBreak(_tokens));
            BlockLabel = label;
            body = label;
        }
        else
        {
            var stack = new VerticalStackLayout { Spacing = 0 };
            var built = new List<Grid>();
            var marked = new List<int>();
            var gutters = new List<Label>();
            var number = 0;
            var lines = DisplayCodeLines.Split(_tokens);
            var width = Math.Max(3, lines.Count.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);
            foreach (var line in lines)
            {
                number++;
                var highlighted = IsHighlighted(_highlightLines, number);
                var row = BuildRow(number, DisplayCodeLines.WithoutBreak(line), highlighted, width, out var gutter);
                built.Add(row);
                if (highlighted) marked.Add(number);
                if (gutter is not null) gutters.Add(gutter);
                stack.Add(row);
            }
            Rows = built;
            HighlightedLines = marked;
            Gutters = gutters;
            body = stack;
        }

        if (_wrap)
        {
            Content = body;
        }
        else
        {
            Content = new ScrollView { Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, Content = body };
        }
    }

    private Grid BuildRow(int number, IReadOnlyList<CodeToken> tokens, bool highlighted, int digits, out Label? gutter)
    {
        gutter = null;
        var row = new Grid { ColumnSpacing = 0, MinimumHeightRequest = 16, HorizontalOptions = LayoutOptions.Fill };
        row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2)));
        if (_lineNumbers) row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        if (highlighted) row.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundAccented);

        // The marker of the gutter: the primary color on an emphasized line, nothing on the others.
        var marker = new BoxView { WidthRequest = 2, Color = Colors.Transparent };
        if (highlighted) marker.SetThemeColor(BoxView.ColorProperty, DisplayUi.PrimaryFill);
        row.Add(marker, 0);

        var column = 1;
        if (_lineNumbers)
        {
            gutter = NewCodeLabel(KlyTheme.TextDimmed);
            gutter.Text = number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            gutter.HorizontalTextAlignment = TextAlignment.End;
            gutter.WidthRequest = digits * 8 + 10;
            gutter.Margin = new Thickness(10, 0, 16, 0);
            gutter.LineBreakMode = LineBreakMode.NoWrap;
            AutomationProperties.SetIsInAccessibleTree(gutter, false);
            row.Add(gutter, column++);
        }

        var text = NewCodeLabel();
        text.FormattedText = Format(tokens);
        text.Margin = new Thickness(_lineNumbers ? 0 : 10, 0, 12, 0);
        row.Add(text, column);
        return row;
    }

    private Label NewCodeLabel(KlyThemeColor? color = null)
    {
        var label = DisplayUi.NewLabel(12, color ?? KlyTheme.TextHighlighted, FontAttributes.None, _wrap ? LineBreakMode.WordWrap : LineBreakMode.NoWrap);
        label.FontFamily = DisplayUi.MonoFont();
        return label;
    }

    /// <summary>The spans of a line: a span per token, the colored ones with a color that follows the app theme.</summary>
    public static FormattedString Format(IReadOnlyList<CodeToken> tokens)
    {
        var text = new FormattedString();
        foreach (var token in tokens)
        {
            var span = new Span { Text = token.Text };
            if (DisplayCodeLines.ColorOf(token.Kind) is { } color) span.SetThemeColor(Span.TextColorProperty, color);
            if (token.Kind == CodeTokenKind.Comment) span.FontAttributes = FontAttributes.Italic;
            text.Spans.Add(span);
        }
        return text;
    }
}
