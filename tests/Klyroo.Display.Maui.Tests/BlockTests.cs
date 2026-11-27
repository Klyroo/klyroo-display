namespace Klyroo.Display.Maui.Tests;

public class BlockTests
{
    static BlockTests() => TestSupport.InstallDispatcher();

    // --- KlyCodeBlock ---------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Code_block_defaults_to_plain_text_with_a_copy_button_and_the_label_Code()
    {
        var block = new KlyCodeBlock { Code = "hello" };

        Assert.Null(block.Language);
        Assert.True(block.Highlight);
        Assert.True(block.Copy);
        Assert.False(block.LineNumbers);
        Assert.Equal("Code", block.TitleLabel.Text);
        Assert.True(block.CopyButton.IsVisible);
        Assert.False(block.IsHighlighted);
        Assert.Equal(TextTransform.Uppercase, block.TitleLabel.TextTransform);
    }

    [Fact]
    public void Code_block_label_is_the_name_of_the_language_unless_a_label_is_given()
    {
        var block = new KlyCodeBlock { Code = "select 1", Language = CodeLanguage.Sql };
        Assert.Equal("sql", block.TitleLabel.Text);

        block.Label = "Query";
        Assert.Equal("Query", block.TitleLabel.Text);

        block.Label = null;
        block.Language = CodeLanguage.CSharp;
        Assert.Equal("csharp", block.TitleLabel.Text);
    }

    [Fact]
    public void Code_block_draws_the_tokens_of_the_highlighter_and_their_texts_make_the_code()
    {
        const string code = "var total = 42; // sum";
        var block = new KlyCodeBlock { Code = code, Language = CodeLanguage.CSharp };

        Assert.True(block.IsHighlighted);
        Assert.Equal(CodeHighlighter.Tokenize(code, CodeLanguage.CSharp), block.Tokens);
        Assert.Contains(block.Tokens, t => t.Kind == CodeTokenKind.Keyword && t.Text == "var");
        Assert.Contains(block.Tokens, t => t.Kind == CodeTokenKind.Number && t.Text == "42");
        Assert.Contains(block.Tokens, t => t.Kind == CodeTokenKind.Comment);

        var label = block.CodeView.BlockLabel!;
        Assert.Equal(block.Tokens.Count, label.FormattedText!.Spans.Count);
        Assert.Equal(code, string.Concat(label.FormattedText.Spans.Select(s => s.Text)));
        Assert.Equal(FontAttributes.Italic, label.FormattedText.Spans.Last().FontAttributes);
    }

    [Fact]
    public void Code_block_without_highlight_or_language_is_one_plain_token()
    {
        var plain = new KlyCodeBlock { Code = "var x = 1;" };
        Assert.Equal([new CodeToken(CodeTokenKind.Plain, "var x = 1;")], plain.Tokens);

        var off = new KlyCodeBlock { Code = "var x = 1;", Language = CodeLanguage.CSharp, Highlight = false };
        Assert.False(off.IsHighlighted);
        Assert.Equal([new CodeToken(CodeTokenKind.Plain, "var x = 1;")], off.Tokens);

        off.Highlight = true;
        Assert.True(off.IsHighlighted);
        Assert.True(off.Tokens.Count > 1);
    }

    [Fact]
    public void Code_block_does_not_highlight_a_code_over_the_line_cap()
    {
        var code = string.Join('\n', Enumerable.Repeat("var x = 1;", CodeHighlighter.MaxLines + 1));
        var block = new KlyCodeBlock { Code = code, Language = CodeLanguage.CSharp };

        Assert.False(block.IsHighlighted);
        Assert.Single(block.Tokens);
    }

    [Theory]
    [InlineData(CodeTokenKind.Keyword, "primary")]
    [InlineData(CodeTokenKind.String, "success")]
    [InlineData(CodeTokenKind.Number, "warning")]
    [InlineData(CodeTokenKind.Type, "secondary")]
    [InlineData(CodeTokenKind.Function, "info")]
    [InlineData(CodeTokenKind.Property, "info")]
    [InlineData(CodeTokenKind.Tag, "error")]
    public void Token_colors_are_the_text_tokens_of_the_semantic_colors(CodeTokenKind kind, string color)
    {
        Assert.Equal(KlyTheme.Resolve(color).Text, DisplayCodeLines.ColorOf(kind));
    }

    [Fact]
    public void Token_colors_for_comment_punctuation_and_plain_text()
    {
        Assert.Equal(KlyTheme.TextMuted, DisplayCodeLines.ColorOf(CodeTokenKind.Comment));
        Assert.Equal(DisplayUi.TextToned, DisplayCodeLines.ColorOf(CodeTokenKind.Punctuation));
        Assert.Null(DisplayCodeLines.ColorOf(CodeTokenKind.Plain));
    }

    [Fact]
    public void Code_block_with_line_numbers_draws_one_row_per_line_and_ignores_the_final_line_break()
    {
        var block = new KlyCodeBlock { Code = "a\nb\nc\n", LineNumbers = true };

        var view = block.CodeView;
        Assert.True(view.UsesRows);
        Assert.Equal(3, view.Rows.Count);
        Assert.Equal(["1", "2", "3"], view.Gutters.Select(g => g.Text).ToArray());
        Assert.Empty(view.HighlightedLines);
        var texts = view.Rows.Select(r => string.Concat(TestSupport.All<Label>(r).Where(l => l.FormattedText is not null).SelectMany(l => l.FormattedText!.Spans).Select(s => s.Text))).ToArray();
        Assert.Equal(["a", "b", "c"], texts);
    }

    [Fact]
    public void Code_block_emphasizes_single_lines_and_runs_and_ignores_lines_past_the_end()
    {
        var block = new KlyCodeBlock
        {
            Code = "1\n2\n3\n4\n5\n6",
            HighlightLines = [3, new CodeLineRange(5, 6), 99],
        };

        Assert.True(block.CodeView.UsesRows);
        Assert.Equal([3, 5, 6], block.CodeView.HighlightedLines);
        Assert.Equal(6, block.CodeView.Rows.Count);
        Assert.Empty(block.CodeView.Gutters);
    }

    [Fact]
    public void Code_block_without_numbers_or_emphasis_is_one_label_in_a_sideways_scroll()
    {
        var block = new KlyCodeBlock { Code = "one\ntwo" };

        Assert.False(block.CodeView.UsesRows);
        var scroll = Assert.IsType<ScrollView>(block.CodeView.Content);
        Assert.Equal(ScrollOrientation.Horizontal, scroll.Orientation);
        Assert.Same(block.CodeView.BlockLabel, scroll.Content);
        Assert.Equal(LineBreakMode.NoWrap, block.CodeView.BlockLabel!.LineBreakMode);
        Assert.Equal(12, block.CodeView.BlockLabel.FontSize);
        Assert.Equal("one\ntwo", string.Concat(block.CodeView.BlockLabel.FormattedText!.Spans.Select(s => s.Text)));
    }

    [Fact]
    public void Code_block_copy_button_copies_the_code_as_given_and_describes_itself()
    {
        var copied = new List<string>();
        var block = new KlyCodeBlock { Code = "line 1\nline 2 <b>", Language = CodeLanguage.Html, LineNumbers = true };
        block.Copier = text =>
        {
            copied.Add(text);
            return Task.CompletedTask;
        };

        block.CopyButton.SendClicked();

        Assert.Equal(["line 1\nline 2 <b>"], copied);
        Assert.Equal("Copy code", SemanticProperties.GetDescription(block.CopyButton));
        Assert.Equal(40, block.CopyButton.WidthRequest);
        Assert.Equal(40, block.CopyButton.HeightRequest);
    }

    [Fact]
    public void Code_block_copy_is_hidden_on_request_and_does_nothing_for_an_empty_code()
    {
        var copied = 0;
        var block = new KlyCodeBlock { Copy = false };
        block.Copier = _ =>
        {
            copied++;
            return Task.CompletedTask;
        };
        Assert.False(block.CopyButton.IsVisible);

        block.Copy = true;
        block.CopyButton.SendClicked();
        Assert.Equal(0, copied);
    }

    [Fact]
    public async Task Copy_swallows_a_clipboard_that_refuses()
    {
        await DisplayClipboard.Copy("x", _ => throw new InvalidOperationException("no clipboard"));
    }

    [Fact]
    public void Code_block_changes_of_the_code_and_the_language_update_what_is_drawn()
    {
        var block = new KlyCodeBlock { Code = "select 1", Language = CodeLanguage.Sql };
        var first = block.Tokens;
        Assert.Contains(first, t => t.Kind == CodeTokenKind.Keyword);

        block.Code = "x";
        Assert.Equal([new CodeToken(CodeTokenKind.Plain, "x")], block.Tokens);

        block.Language = null;
        Assert.False(block.IsHighlighted);
    }

    [Fact]
    public void Code_block_is_a_bordered_panel_with_a_muted_code_area()
    {
        var block = new KlyCodeBlock { Code = "x" };

        Assert.Equal(1, block.StrokeThickness);
        Assert.Equal(KlyTheme.Radius, ((Microsoft.Maui.Controls.Shapes.RoundRectangle)block.StrokeShape!).CornerRadius.TopLeft);
        Assert.Equal(new Thickness(16), block.Padding);
        Assert.Equal(LayoutOptions.Fill, block.HorizontalOptions);
    }

    [Fact]
    public void Code_block_texts_follow_DisplayTexts()
    {
        TestSupport.WithTexts(new DisplayTexts { Code = "Extrait", CopyCode = "Copier le code" }, () =>
        {
            var block = new KlyCodeBlock { Code = "x" };
            Assert.Equal("Extrait", block.TitleLabel.Text);
            Assert.Equal("Copier le code", SemanticProperties.GetDescription(block.CopyButton));
        });
    }

    // --- KlySecretBlock -------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("", 4, "••••••••")]
    [InlineData("abc", 4, "••••••••")]
    [InlineData("abcd", 4, "••••••••")]
    [InlineData("sk-1234567890abcdef", 4, "••••••••cdef")]
    [InlineData("sk-1234567890abcdef", 0, "•••••••••••••••••••")]
    [InlineData("sk-1234567890abcdef", 2, "••••••••ef")]
    public void Mask_leaves_the_last_characters_and_never_shows_a_short_secret_whole(string raw, int visible, string expected)
    {
        Assert.Equal(expected, KlySecretBlock.Mask(raw, visible));
    }

    [Fact]
    public void Secret_block_is_masked_by_default_and_never_holds_the_whole_value()
    {
        var block = new KlySecretBlock { Value = "sk-1234567890abcdef" };

        Assert.Equal("••••••••cdef", block.Shown);
        Assert.DoesNotContain("1234567890", string.Join(' ', TestSupport.Texts(block)));
        Assert.False(block.IsRevealed);
        Assert.Equal("Secret", block.TitleLabel.Text);
        Assert.Equal("Reveal", block.ToggleButton.Text);
        Assert.Equal("eye", block.ToggleButton.Icon.Name);
        Assert.True(block.CopyButton.IsVisible);
    }

    [Fact]
    public void Secret_block_toggle_reveals_and_conceals()
    {
        var block = new KlySecretBlock { Value = "sk-1234567890abcdef" };

        block.ToggleButton.SendClicked();
        Assert.True(block.IsRevealed);
        Assert.Equal("sk-1234567890abcdef", block.Shown);
        Assert.Equal("Conceal", block.ToggleButton.Text);
        Assert.Equal("eye-off", block.ToggleButton.Icon.Name);
        Assert.Equal("Conceal", SemanticProperties.GetDescription(block.ToggleButton));

        block.ToggleButton.SendClicked();
        Assert.False(block.IsRevealed);
        Assert.Equal("••••••••cdef", block.Shown);
        Assert.Equal("Reveal", block.ToggleButton.Text);
    }

    [Fact]
    public void Secret_block_visible_chars_and_value_changes_update_the_mask()
    {
        var block = new KlySecretBlock { Value = "abcdefghij", VisibleChars = 2 };
        Assert.Equal("••••••••ij", block.Shown);

        block.VisibleChars = 0;
        Assert.Equal("••••••••••", block.Shown);

        block.Value = "xy";
        Assert.Equal("••••••••", block.Shown);
    }

    [Fact]
    public void Secret_block_copy_copies_the_whole_secret_even_while_masked()
    {
        var copied = new List<string>();
        var block = new KlySecretBlock { Value = "sk-1234567890abcdef" };
        block.Copier = text =>
        {
            copied.Add(text);
            return Task.CompletedTask;
        };

        block.CopyButton.SendClicked();

        Assert.Equal(["sk-1234567890abcdef"], copied);
        Assert.False(block.IsRevealed);
        Assert.Equal("Copy code", SemanticProperties.GetDescription(block.CopyButton));
    }

    [Fact]
    public void Secret_block_copy_can_be_hidden_and_the_label_set()
    {
        var block = new KlySecretBlock { Value = "x", Copy = false, Label = "API key" };

        Assert.False(block.CopyButton.IsVisible);
        Assert.Equal("API key", block.TitleLabel.Text);
    }

    [Fact]
    public void Secret_block_is_revealed_two_way()
    {
        var source = new RevealSource();
        var block = new KlySecretBlock { Value = "sk-1234567890abcdef", BindingContext = source };
        block.SetBinding(KlySecretBlock.IsRevealedProperty, nameof(RevealSource.Revealed), BindingMode.TwoWay);

        source.Revealed = true;
        Assert.Equal("sk-1234567890abcdef", block.Shown);

        block.ToggleButton.SendClicked();
        Assert.False(source.Revealed);
        Assert.Equal("••••••••cdef", block.Shown);
    }

    [Fact]
    public void Secret_block_texts_follow_DisplayTexts()
    {
        TestSupport.WithTexts(new DisplayTexts { Secret = "Secret", Reveal = "Afficher", Conceal = "Masquer", CopyCode = "Copier" }, () =>
        {
            var block = new KlySecretBlock { Value = "sk-1234567890abcdef" };
            Assert.Equal("Afficher", block.ToggleButton.Text);
            block.IsRevealed = true;
            Assert.Equal("Masquer", block.ToggleButton.Text);
            Assert.Equal("Copier", SemanticProperties.GetDescription(block.CopyButton));
        });
    }

    [Fact]
    public void DisplayTexts_has_english_plurals_and_rejects_null()
    {
        var texts = new DisplayTexts();
        Assert.Equal("1 play left", texts.PlaysLeft(1));
        Assert.Equal("2 plays left", texts.PlaysLeft(2));
        Assert.Equal("Show more · 3", texts.ShowMore(3));
        Assert.Equal("1 line", texts.LineCount(1));
        Assert.Equal("12 lines", texts.LineCount(12));
        Assert.Equal("12 rows · 4 columns · delimiter ','", texts.CsvSummary(12, 4, ","));
        Assert.Equal("Showing first 1,000 rows", texts.CsvTruncated(1000));
        Assert.Throws<ArgumentNullException>(() => DisplayTexts.Current = null!);
    }

    private sealed class RevealSource : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _revealed;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public bool Revealed
        {
            get => _revealed;
            set
            {
                _revealed = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Revealed)));
            }
        }
    }
}
