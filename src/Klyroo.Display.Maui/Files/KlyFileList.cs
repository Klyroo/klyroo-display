using System.Globalization;

namespace Klyroo.Display.Maui;

/// <summary>
/// A list of files, each with an extension badge (or an icon), its name, its size and its date. Tapping a file raises <see cref="FileSelected"/>, and opens the whole list as a gallery,
/// at that file, in <see cref="Preview"/> when it is set: put a <see cref="KlyFilePreviewProvider"/> that shares the same service in the page to show it.
/// </summary>
public class KlyFileList : Border
{
    /// <summary>Identifies <see cref="Files"/>.</summary>
    public static readonly BindableProperty FilesProperty = BindableProperty.Create(nameof(Files), typeof(IReadOnlyList<FileListItem>), typeof(KlyFileList), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(KlyFileList), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Max"/>.</summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(int?), typeof(KlyFileList), null, propertyChanged: OnMaxChanged);
    /// <summary>Identifies <see cref="Step"/>.</summary>
    public static readonly BindableProperty StepProperty = BindableProperty.Create(nameof(Step), typeof(int?), typeof(KlyFileList), null, propertyChanged: Refresh);
    /// <summary>Identifies <see cref="Preview"/>.</summary>
    public static readonly BindableProperty PreviewProperty = BindableProperty.Create(nameof(Preview), typeof(FilePreviewService), typeof(KlyFileList), null);

    private readonly VerticalStackLayout _body = new() { Spacing = 8 };
    private int _revealedCount;
    private readonly List<(Border Row, FileListItem Item)> _rows = [];
    private DisplayTextAction? _more;

    /// <summary>Creates an empty list: set <see cref="Files"/>.</summary>
    public KlyFileList()
    {
        StrokeThickness = 0;
        BackgroundColor = Colors.Transparent;
        Padding = 0;
        HorizontalOptions = LayoutOptions.Fill;
        Content = _body;
        Rebuild();
    }

    /// <summary>Raised when the user taps a file, before the gallery opens in <see cref="Preview"/>.</summary>
    public event EventHandler<KlyFileSelectedEventArgs>? FileSelected;

    /// <summary>The files to list, in order. A <see cref="FilePreviewFile"/> converts to an item by itself. A row shows the badge (the extension, or the item <c>Icon</c>), the name (or the last segment of <c>Src</c>), the description and the size, and the item <c>Meta</c> or the date of the file.</summary>
    public IReadOnlyList<FileListItem>? Files { get => (IReadOnlyList<FileListItem>?)GetValue(FilesProperty); set => SetValue(FilesProperty, value); }

    /// <summary>A title above the list.</summary>
    public string? Title { get => (string?)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>The number of files shown before a Show more button (all files when <see langword="null"/>).</summary>
    public int? Max { get => (int?)GetValue(MaxProperty); set => SetValue(MaxProperty, value); }

    /// <summary>The number of files each press on Show more reveals (all remaining files when <see langword="null"/>).</summary>
    public int? Step { get => (int?)GetValue(StepProperty); set => SetValue(StepProperty, value); }

    /// <summary>The preview a tap opens the list in, as a gallery at the tapped file. Without it a tap only raises <see cref="FileSelected"/>.</summary>
    public FilePreviewService? Preview { get => (FilePreviewService?)GetValue(PreviewProperty); set => SetValue(PreviewProperty, value); }

    /// <summary>The rows that show, for the tests.</summary>
    internal IReadOnlyList<Border> Rows => _rows.Select(r => r.Row).ToList();

    /// <summary>The Show more button, or <see langword="null"/> when every file shows, for the tests.</summary>
    internal DisplayTextAction? MoreButton => _more;

    /// <summary>Taps the row of an item, as the user does.</summary>
    internal void Select(int rowIndex)
    {
        var (_, item) = _rows[rowIndex];
        OnTapped(item);
    }

    private static void Refresh(BindableObject bindable, object oldValue, object newValue) => ((KlyFileList)bindable).Rebuild();

    private static void OnMaxChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var list = (KlyFileList)bindable;
        if (newValue is int max && list._revealedCount < max) list._revealedCount = max;
        list.Rebuild();
    }

    private (IReadOnlyList<FileListItem> Visible, int Remaining) ApplyReveal()
    {
        var source = Files;
        if (source is null || source.Count == 0) return ([], 0);
        if (Max is not { } max || max <= 0) return (source, 0);
        var limit = Math.Max(max, _revealedCount);
        if (source.Count <= limit) return (source, 0);
        var next = Step is > 0 ? Math.Min(Step.Value, source.Count - limit) : source.Count - limit;
        return (source.Take(limit).ToList(), next);
    }

    private void RevealMore()
    {
        var total = Files?.Count ?? 0;
        var current = Math.Max(Max ?? 0, _revealedCount);
        _revealedCount = Step is > 0 ? Math.Min(total, current + Step.Value) : total;
        Rebuild();
    }

    private void OnTapped(FileListItem item)
    {
        var files = Files;
        if (files is null) return;
        var index = 0;
        for (var i = 0; i < files.Count; i++)
        {
            if (ReferenceEquals(files[i], item)) { index = i; break; }
        }
        FileSelected?.Invoke(this, new KlyFileSelectedEventArgs(item, index, files));
        Preview?.OpenGallery(files.Select(f => f.File).ToList(), new FilePreviewOptions { Index = index });
    }

    private void Rebuild()
    {
        _body.Clear();
        _rows.Clear();
        _more = null;
        var texts = DisplayTexts.Current;

        if (!string.IsNullOrEmpty(Title))
        {
            var heading = DisplayUi.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
            heading.Text = Title;
            _body.Add(heading);
        }

        var (visible, remaining) = ApplyReveal();
        var culture = CultureInfo.CurrentCulture;
        foreach (var item in visible)
        {
            var row = BuildRow(item, culture);
            _rows.Add((row, item));
            _body.Add(row);
        }

        if (remaining > 0)
        {
            _more = new DisplayTextAction(KlyIconNames.ChevronDown, texts.ShowMore(remaining), iconAfter: true) { HorizontalOptions = LayoutOptions.Start };
            _more.Clicked += (_, _) => RevealMore();
            _body.Add(_more);
        }
    }

    private Border BuildRow(FileListItem item, CultureInfo culture)
    {
        var file = item.File;
        var name = file.Name ?? FilePreviewService.FileNameFromUrl(file.Src);
        var meta = item.Meta ?? (file.UpdatedAt is { } updated ? updated.ToString("d", culture) : null);
        var detail = item.Description is { Length: > 0 } text
            ? (file.Size is { } bytes ? $"{text} · {FilePreviewService.FormatFileSize(bytes, culture)}" : text)
            : (file.Size is { } only ? FilePreviewService.FormatFileSize(only, culture) : null);

        var badge = BuildBadge(item, name);
        var title = DisplayUi.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.None, LineBreakMode.TailTruncation);
        title.Text = name;
        var description = DisplayUi.NewLabel(12, KlyTheme.TextMuted, FontAttributes.None, LineBreakMode.TailTruncation);
        DisplayUi.Show(description, detail);
        var body = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { title, description } };

        var main = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        main.Add(badge, 0);
        main.Add(body, 1);

        var metaLabel = DisplayUi.NewLabel(12, KlyTheme.TextMuted, FontAttributes.None, LineBreakMode.NoWrap);
        metaLabel.VerticalOptions = LayoutOptions.Center;
        DisplayUi.Show(metaLabel, meta);

        var grid = new Grid { ColumnSpacing = 12, MinimumHeightRequest = 40, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        grid.Add(main, 0);
        grid.Add(metaLabel, 1);

        var row = new Border { StrokeThickness = 0, Padding = 0, BackgroundColor = Colors.Transparent, Content = grid };
        SemanticProperties.SetDescription(row, name);
        SemanticProperties.SetHint(row, detail);
        DisplayUi.Tap(row, () => OnTapped(item));
        return row;
    }

    private static Border BuildBadge(FileListItem item, string name)
    {
        var badge = new Border { StrokeThickness = 0, HeightRequest = 24, MinimumWidthRequest = 32, Padding = new Thickness(6, 0), VerticalOptions = LayoutOptions.Center };
        badge.SetRadius(KlyTheme.RadiusOf(1));
        KlyThemeColor text;
        if (item.Tone is { } tone)
        {
            var set = KlyTheme.Resolve(tone);
            badge.SetThemeColor(VisualElement.BackgroundColorProperty, set.Fill.WithAlpha(0.10f));
            text = set.Text;
        }
        else
        {
            badge.SetThemeColor(VisualElement.BackgroundColorProperty, KlyTheme.BackgroundMuted);
            text = KlyTheme.TextHighlighted;
        }

        if (!string.IsNullOrEmpty(item.Icon))
        {
            badge.Content = new KlyIcon { Name = item.Icon, Size = KlySize.Sm, IconColor = text, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }
        else
        {
            var label = DisplayUi.NewLabel(10, text, FontAttributes.Bold, LineBreakMode.NoWrap);
            label.Text = FilePreviewService.ExtensionLabel(name, item.File.Mime);
            label.HorizontalOptions = LayoutOptions.Center;
            label.VerticalOptions = LayoutOptions.Center;
            badge.Content = label;
        }
        AutomationProperties.SetIsInAccessibleTree(badge, false);
        return badge;
    }
}
