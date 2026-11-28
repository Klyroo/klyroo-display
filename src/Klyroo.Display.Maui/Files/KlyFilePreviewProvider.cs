using System.Globalization;

namespace Klyroo.Display.Maui;

/// <summary>
/// Shows the file preview that a <see cref="FilePreviewService"/> opens: a <see cref="KlyModal"/> (or a <see cref="KlySlideover"/> for the kinds that suit a drawer) with a header (name, kind, size, date),
/// a toolbar (previous and next, zoom and rotate for images, custom actions, details, download), a stage that draws the file, a rendition notice, a details panel and a strip of thumbnails for galleries.
/// Place one provider in the page, once, inside a <see cref="KlyOverlayHost"/>.
/// </summary>
/// <remarks>
/// <para>Images, video, audio, CSV, Markdown (as source, or through <see cref="MarkdownRenderer"/>) and text (highlighted by extension) are drawn natively. A PDF, an Office file, an archive and any other file have no
/// native renderer: the stage offers to open the file with the platform (<see cref="Launcher"/>). There is no <c>WebView</c>.</para>
/// <para>Video and audio play through <see cref="PlaybackFactory"/>, which gives the app's media player; without it the players open the file with another app.</para>
/// </remarks>
public class KlyFilePreviewProvider : ContentView
{
    /// <summary>Identifies <see cref="Service"/>.</summary>
    public static readonly BindableProperty ServiceProperty = BindableProperty.Create(nameof(Service), typeof(FilePreviewService), typeof(KlyFilePreviewProvider), null, propertyChanged: (b, o, n) => ((KlyFilePreviewProvider)b).OnServiceChanged((FilePreviewService?)o, (FilePreviewService?)n));
    /// <summary>Identifies <see cref="MarkdownRenderer"/>.</summary>
    public static readonly BindableProperty MarkdownRendererProperty = BindableProperty.Create(nameof(MarkdownRenderer), typeof(Func<string, View>), typeof(KlyFilePreviewProvider), null, propertyChanged: (b, _, _) => ((KlyFilePreviewProvider)b).RebuildStage());
    /// <summary>Identifies <see cref="PlaybackFactory"/>.</summary>
    public static readonly BindableProperty PlaybackFactoryProperty = BindableProperty.Create(nameof(PlaybackFactory), typeof(Func<FilePreviewFile, IMediaPlayback?>), typeof(KlyFilePreviewProvider), null);
    /// <summary>Identifies <see cref="CsvMaxRows"/>.</summary>
    public static readonly BindableProperty CsvMaxRowsProperty = BindableProperty.Create(nameof(CsvMaxRows), typeof(int), typeof(KlyFilePreviewProvider), 200, propertyChanged: (b, _, _) => ((KlyFilePreviewProvider)b).RebuildStage());

    private readonly KlyModal _modal = new() { Size = KlySize.Xl };
    private readonly KlySlideover _drawer = new() { Side = SlideoverSide.Right };
    private readonly VerticalStackLayout _body = new() { Spacing = 12 };
    private readonly FlexLayout _toolbar = new() { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center };
    private readonly Border _banner = new() { StrokeThickness = 0, Padding = new Thickness(12, 6), IsVisible = false };
    private readonly ContentView _stage = new();
    private readonly Border _details = new() { StrokeThickness = 0, Padding = new Thickness(16), IsVisible = false };
    private readonly ScrollView _strip = new() { Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, IsVisible = false };

    private KlyOverlayBase? _shown;
    private FilePreviewFile? _current;
    private IMediaPlayback? _ownedPlayback;
    private bool _syncing;
    private bool _showDetails;
    private bool _wrapLines;
    private bool _markdownSource;
    private double _zoom = 1;
    private int _rotation;
    private Image? _image;
    private DisplayTextAction? _zoomReset;
    private FilePreviewService? _subscribed;

    /// <summary>Creates a provider with its own <see cref="FilePreviewService"/>.</summary>
    public KlyFilePreviewProvider()
    {
        _banner.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
        _banner.SetRadius(KlyTheme.RadiusOf(1));
        _details.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundElevated);
        _details.SetRadius(KlyTheme.RadiusOf(1));
        _body.Children.Add(_toolbar);
        _body.Children.Add(_banner);
        _body.Children.Add(_stage);
        _body.Children.Add(_details);
        _body.Children.Add(_strip);

        _modal.Closed += OnDismissed;
        _drawer.Closed += OnDismissed;
        Content = new Grid { Children = { _modal, _drawer } };

        Loaded += (_, _) => Subscribe(Service);
        Unloaded += (_, _) => Unsubscribe();
        Service = new FilePreviewService();
    }

    /// <summary>The service that holds the open gallery. Share it with <see cref="KlyFileList.Preview"/> (register one <see cref="FilePreviewService"/> in the app) and call <see cref="FilePreviewService.Open"/> or <see cref="FilePreviewService.OpenGallery"/>. Each provider starts with a service of its own.</summary>
    public FilePreviewService Service { get => (FilePreviewService)GetValue(ServiceProperty); set => SetValue(ServiceProperty, value); }

    /// <summary>
    /// Draws the Markdown of a file as a view, for example with a Markdown to MAUI library. When it is set, a Markdown file gets a Preview and Source toggle; when it is <see langword="null"/>, Markdown files are shown as source.
    /// This replaces the <c>MarkdownRenderer</c> of the Blazor provider, which returns HTML.
    /// </summary>
    public Func<string, View>? MarkdownRenderer { get => (Func<string, View>?)GetValue(MarkdownRendererProperty); set => SetValue(MarkdownRendererProperty, value); }

    /// <summary>
    /// Gives the <see cref="IMediaPlayback"/> a video or audio file plays through, or <see langword="null"/> for the default playback that opens the file with another app.
    /// The provider disposes the playback when the file changes or the preview closes.
    /// </summary>
    public Func<FilePreviewFile, IMediaPlayback?>? PlaybackFactory { get => (Func<FilePreviewFile, IMediaPlayback?>?)GetValue(PlaybackFactoryProperty); set => SetValue(PlaybackFactoryProperty, value); }

    /// <summary>The largest number of data rows of a CSV file that the stage draws (<c>200</c> by default: every cell is a native view). The Blazor preview draws 1,000.</summary>
    public int CsvMaxRows { get => (int)GetValue(CsvMaxRowsProperty); set => SetValue(CsvMaxRowsProperty, value); }

    /// <summary>The overlay that shows the preview now, or <see langword="null"/>, for the tests.</summary>
    internal KlyOverlayBase? ShownOverlay => _shown;

    internal KlyModal Modal => _modal;

    internal KlySlideover Drawer => _drawer;

    internal VerticalStackLayout Body => _body;

    internal FlexLayout Toolbar => _toolbar;

    internal Border Banner => _banner;

    internal ContentView Stage => _stage;

    internal Border Details => _details;

    internal ScrollView Strip => _strip;

    internal double Zoom => _zoom;

    internal int ImageRotation => _rotation;

    internal bool ShowsDetails => _showDetails;

    internal Image? CurrentImage => _image;

    internal IMediaPlayback? OwnedPlayback => _ownedPlayback;

    /// <summary>Opens an address instead of the platform; for the tests.</summary>
    internal Func<string, Task>? Opener { get; set; }

    // --- the service -----------------------------------------------------------------------------------------------------------------------

    private void OnServiceChanged(FilePreviewService? oldService, FilePreviewService? newService)
    {
        Unsubscribe();
        Subscribe(newService);
        Sync();
    }

    private void Subscribe(FilePreviewService? service)
    {
        if (service is null || ReferenceEquals(_subscribed, service)) return;
        Unsubscribe();
        _subscribed = service;
        service.Changed += OnChanged;
        Sync();
    }

    private void Unsubscribe()
    {
        if (_subscribed is null) return;
        _subscribed.Changed -= OnChanged;
        _subscribed = null;
    }

    private void OnChanged() => DisplayUi.Post(this, () =>
    {
        ResetImageTransform();
        if (Service is { IsOpen: false }) _showDetails = false;
        Sync();
    });

    // The user dismissed the overlay (a tap outside, the close button, the back button): the service closes with it.
    private void OnDismissed(object? sender, EventArgs e)
    {
        if (_syncing) return;
        if (Service is { IsOpen: true } service) service.Close();
    }

    internal void Sync()
    {
        var service = Service;
        _syncing = true;
        try
        {
            if (service is null || !service.IsOpen || service.CurrentFile is not { } file)
            {
                _current = null;
                CloseShown();
                _stage.Content = null;
                ReleasePlayback();
                return;
            }

            var overlay = service.ResolveContainer() == FilePreviewContainer.Drawer ? (KlyOverlayBase)_drawer : _modal;
            if (!ReferenceEquals(_shown, overlay)) CloseShown();

            if (!ReferenceEquals(_current, file) || !ReferenceEquals(_shown, overlay))
            {
                _current = file;
                ReleasePlayback();
                BuildFor(service, file);
            }
            else
            {
                BuildToolbar(service, file);
            }

            var panel = (KlyPanelOverlay)overlay;
            var name = FileName(file);
            panel.Title = name;
            panel.Description = MetaLine(file);
            panel.CloseLabel = DisplayTexts.Current.ClosePreview;
            panel.Body = _body;
            if (!panel.IsOpen) panel.IsOpen = true;
            _shown = overlay;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void CloseShown()
    {
        if (_shown is null) return;
        _shown.IsOpen = false;
        if (_shown is KlyPanelOverlay panel) panel.Body = null;
        _shown = null;
    }

    // --- building the content --------------------------------------------------------------------------------------------------------

    private static string FileName(FilePreviewFile file) => file.Name ?? FilePreviewService.FileNameFromUrl(file.Src);

    /// <summary>The line under the title: the kind, the size and the date of the file, as the header of the Blazor preview shows them.</summary>
    internal static string MetaLine(FilePreviewFile file)
    {
        var parts = new List<string> { FilePreviewService.DetectKind(file.Rendition ?? file).ToString() };
        if (file.Size is { } size) parts.Add(FilePreviewService.FormatFileSize(size, CultureInfo.InvariantCulture));
        if (file.UpdatedAt is { } updated) parts.Add(updated.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        return string.Join(" · ", parts);
    }

    private void BuildFor(FilePreviewService service, FilePreviewFile file)
    {
        var texts = DisplayTexts.Current;
        var display = file.Rendition ?? file;
        var kind = FilePreviewService.DetectKind(display);
        var name = FileName(file);

        BuildToolbar(service, file);

        _banner.IsVisible = file.Rendition is not null;
        if (file.Rendition is not null)
        {
            var original = DisplayUi.NewLabel(12, KlyTheme.TextMuted);
            original.Text = texts.ConvertedPreview(FilePreviewService.DetectKind(file).ToString());
            var download = new DisplayTextAction(null, texts.DownloadOriginal, DisplayUi.PrimaryText);
            download.Clicked += (_, _) => _ = DisplayLauncher.OpenAsync(file.DownloadUrl ?? file.Src, Opener);
            _banner.Content = new VerticalStackLayout { Spacing = 0, Children = { original, download } };
        }
        else
        {
            _banner.Content = null;
        }

        _stage.Content = BuildStage(display, kind, name);
        BuildDetails(file, kind, name);
        BuildStrip(service);
        SemanticProperties.SetDescription(_body, service.Files.Count > 1 ? texts.PreviewPosition(service.CurrentIndex + 1, service.Files.Count, name) : null);
    }

    private void RebuildStage()
    {
        if (_current is not { } file) return;
        var display = file.Rendition ?? file;
        _stage.Content = BuildStage(display, FilePreviewService.DetectKind(display), FileName(file));
    }

    private void BuildToolbar(FilePreviewService service, FilePreviewFile file)
    {
        var texts = DisplayTexts.Current;
        var display = file.Rendition ?? file;
        var kind = FilePreviewService.DetectKind(display);
        _toolbar.Clear();
        _zoomReset = null;

        if (service.Files.Count > 1)
        {
            var previous = new DisplayIconButton(KlyIconNames.ChevronLeft, texts.PreviousFile, KlyTheme.Text);
            previous.IsEnabled = service.Options.Loop || service.CurrentIndex > 0;
            previous.Opacity = previous.IsEnabled ? 1 : 0.4;
            previous.Clicked += (_, _) => service.Previous();
            var counter = DisplayUi.NewLabel(12, KlyTheme.TextMuted, FontAttributes.Bold, LineBreakMode.NoWrap);
            counter.Text = $"{service.CurrentIndex + 1} / {service.Files.Count}";
            counter.VerticalOptions = LayoutOptions.Center;
            counter.Margin = new Thickness(4, 0);
            var next = new DisplayIconButton(KlyIconNames.ChevronRight, texts.NextFile, KlyTheme.Text);
            next.IsEnabled = service.Options.Loop || service.CurrentIndex < service.Files.Count - 1;
            next.Opacity = next.IsEnabled ? 1 : 0.4;
            next.Clicked += (_, _) => service.Next();
            _toolbar.Add(previous);
            _toolbar.Add(counter);
            _toolbar.Add(next);
        }

        if (kind == FilePreviewKind.Image)
        {
            var zoomOut = new DisplayTextAction(null, "−", KlyTheme.Text);
            zoomOut.SetDescription(texts.ZoomOut);
            zoomOut.Clicked += (_, _) => ZoomOut();
            _zoomReset = new DisplayTextAction(null, $"{Math.Round(_zoom * 100)}%", KlyTheme.Text);
            _zoomReset.SetDescription(texts.ResetZoom);
            _zoomReset.Clicked += (_, _) => ResetImageTransform();
            var zoomIn = new DisplayTextAction(null, "+", KlyTheme.Text);
            zoomIn.SetDescription(texts.ZoomIn);
            zoomIn.Clicked += (_, _) => ZoomIn();
            var rotate = new DisplayTextAction(null, "↻", KlyTheme.Text);
            rotate.SetDescription(texts.Rotate);
            rotate.Clicked += (_, _) => RotateImage();
            _toolbar.Add(zoomOut);
            _toolbar.Add(_zoomReset);
            _toolbar.Add(zoomIn);
            _toolbar.Add(rotate);
        }

        if (file.Actions is { Count: > 0 } actions)
        {
            foreach (var action in actions)
            {
                var button = new DisplayTextAction(action.Icon, action.Label, KlyTheme.Text);
                button.Clicked += (_, _) => _ = RunActionAsync(action, file);
                _toolbar.Add(button);
            }
        }

        var details = new DisplayTextAction(null, texts.Details, _showDetails ? KlyTheme.TextHighlighted : KlyTheme.TextMuted);
        details.Label.FontAttributes = _showDetails ? FontAttributes.Bold : FontAttributes.None;
        details.Clicked += (_, _) =>
        {
            _showDetails = !_showDetails;
            _details.IsVisible = _showDetails;
            if (_current is not null) BuildToolbar(service, _current);
        };
        _toolbar.Add(details);

        var downloadAction = new DisplayTextAction(DisplayIcons.Download, texts.Download);
        downloadAction.Clicked += (_, _) => _ = DisplayLauncher.OpenAsync(file.DownloadUrl ?? file.Src, Opener);
        _toolbar.Add(downloadAction);
    }

    private async Task RunActionAsync(FilePreviewAction action, FilePreviewFile file)
    {
        if (!string.IsNullOrEmpty(action.Href)) await DisplayLauncher.OpenAsync(action.Href, Opener);
        if (action.OnSelect is not null) await action.OnSelect(file);
    }

    private void BuildDetails(FilePreviewFile file, FilePreviewKind kind, string name)
    {
        var texts = DisplayTexts.Current;
        _details.IsVisible = _showDetails;
        var stack = new VerticalStackLayout { Spacing = 8 };
        var heading = DisplayUi.NewLabel(12, KlyTheme.TextMuted, FontAttributes.Bold, LineBreakMode.TailTruncation);
        heading.Text = texts.Details;
        heading.TextTransform = TextTransform.Uppercase;
        SemanticProperties.SetHeadingLevel(heading, SemanticHeadingLevel.Level2);
        stack.Add(heading);
        stack.Add(DetailRow(texts.DetailName, name));
        stack.Add(DetailRow(texts.DetailType, file.Mime ?? kind.ToString()));
        if (file.Size is { } size) stack.Add(DetailRow(texts.DetailSize, FilePreviewService.FormatFileSize(size, CultureInfo.InvariantCulture)));
        if (file.Details is { Count: > 0 } extra)
        {
            foreach (var detail in extra) stack.Add(DetailRow(detail.Label, detail.Value));
        }
        _details.Content = stack;
    }

    private static Grid DetailRow(string label, string value)
    {
        var row = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        var key = DisplayUi.NewLabel(12, KlyTheme.TextMuted);
        key.Text = label;
        var val = DisplayUi.NewLabel(12, KlyTheme.TextHighlighted, FontAttributes.Bold, LineBreakMode.TailTruncation);
        val.Text = value;
        val.HorizontalTextAlignment = TextAlignment.End;
        row.Add(key, 0);
        row.Add(val, 1);
        return row;
    }

    private void BuildStrip(FilePreviewService service)
    {
        _strip.IsVisible = service.Files.Count > 1;
        if (service.Files.Count <= 1)
        {
            _strip.Content = null;
            return;
        }
        var row = new HorizontalStackLayout { Spacing = 8 };
        for (var i = 0; i < service.Files.Count; i++)
        {
            var index = i;
            var file = service.Files[i];
            var fileName = FileName(file);
            var current = index == service.CurrentIndex;

            var ext = DisplayUi.NewLabel(12, current ? KlyTheme.TextHighlighted : KlyTheme.TextMuted, FontAttributes.Bold, LineBreakMode.NoWrap);
            ext.Text = FilePreviewService.ExtensionLabel(fileName, file.Mime);
            var title = DisplayUi.NewLabel(12, current ? KlyTheme.TextHighlighted : KlyTheme.TextMuted, FontAttributes.None, LineBreakMode.TailTruncation);
            title.Text = fileName;
            title.MaximumWidthRequest = 112;

            var chip = new Border { StrokeThickness = 1, Padding = new Thickness(10, 0), MinimumHeightRequest = 40, Content = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center, Children = { ext, title } } };
            chip.SetRadius(KlyTheme.RadiusOf(1));
            chip.SetStroke(current ? DisplayUi.PrimaryText : KlyTheme.Border);
            if (current) chip.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
            SemanticProperties.SetDescription(chip, fileName);
            SemanticProperties.SetHint(chip, current ? DisplayTexts.Current.PreviewPosition(index + 1, service.Files.Count, fileName) : null);
            DisplayUi.Tap(chip, () => service.GoTo(index));
            row.Add(chip);
        }
        _strip.Content = row;
    }

    // --- the stage -------------------------------------------------------------------------------------------------------------------

    private View BuildStage(FilePreviewFile file, FilePreviewKind kind, string name)
    {
        _image = null;
        return kind switch
        {
            FilePreviewKind.Image => ImageStage(file, name),
            FilePreviewKind.Video => VideoStage(file, name),
            FilePreviewKind.Audio => AudioStage(file, name),
            FilePreviewKind.Csv => CsvStage(file, name),
            FilePreviewKind.Markdown => MarkdownStage(file),
            FilePreviewKind.Text => TextStage(file),
            _ => FallbackStage(file, name, opens: kind == FilePreviewKind.Pdf),
        };
    }

    private View ImageStage(FilePreviewFile file, string name)
    {
        var image = new Image { Source = DisplayImages.From(file.Src), Aspect = Aspect.AspectFit, HeightRequest = 360, HorizontalOptions = LayoutOptions.Fill, Scale = _zoom, Rotation = _rotation };
        SemanticProperties.SetDescription(image, name);
        _image = image;
        return new Grid { IsClippedToBounds = true, Children = { image } };
    }

    private View VideoStage(FilePreviewFile file, string name)
    {
        var player = new KlyVideoPlayer { Src = file.Src, Poster = file.Poster, Title = name, Tracks = file.Tracks };
        player.Playback = TakePlayback(file);
        return player;
    }

    private View AudioStage(FilePreviewFile file, string name)
    {
        var card = new VerticalStackLayout { Spacing = 16, HorizontalOptions = LayoutOptions.Fill };
        if (!string.IsNullOrEmpty(file.Poster))
        {
            var cover = new Image { Source = DisplayImages.From(file.Poster), Aspect = Aspect.AspectFill, WidthRequest = 128, HeightRequest = 128, HorizontalOptions = LayoutOptions.Center };
            SemanticProperties.SetDescription(cover, name);
            card.Add(cover);
        }
        var title = DisplayUi.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
        title.Text = name;
        title.HorizontalTextAlignment = TextAlignment.Center;
        card.Add(title);
        var player = new KlyAudioPlayer { Src = file.Src, Title = name };
        player.Playback = TakePlayback(file);
        card.Add(player);
        return Card(card, 24);
    }

    private IMediaPlayback? TakePlayback(FilePreviewFile file)
    {
        var playback = PlaybackFactory?.Invoke(file);
        _ownedPlayback = playback;
        return playback;
    }

    private void ReleasePlayback()
    {
        if (_ownedPlayback is null) return;
        var playback = _ownedPlayback;
        _ownedPlayback = null;
        try
        {
            playback.Dispose();
        }
        catch (Exception)
        {
            // A playback that fails to dispose has nothing more to release here.
        }
    }

    private View CsvStage(FilePreviewFile file, string name)
    {
        var texts = DisplayTexts.Current;
        var parsed = FilePreviewService.ParseCsv(file.Content, name, Math.Max(1, CsvMaxRows));
        var grid = new Grid { ColumnSpacing = 0, RowSpacing = 0 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        foreach (var _ in parsed.Headers) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        Label Cell(string text, bool header, bool end, bool muted)
        {
            var label = DisplayUi.NewLabel(12, header || muted ? KlyTheme.TextMuted : KlyTheme.Text, header ? FontAttributes.Bold : FontAttributes.None, LineBreakMode.NoWrap);
            label.Text = text;
            label.Padding = new Thickness(header ? 12 : 12, 6);
            label.HorizontalTextAlignment = end ? TextAlignment.End : TextAlignment.Start;
            return label;
        }

        grid.Add(Cell("#", header: true, end: true, muted: true), 0, 0);
        for (var c = 0; c < parsed.Headers.Count; c++)
        {
            var numeric = c < parsed.NumericColumns.Count && parsed.NumericColumns[c];
            grid.Add(Cell(parsed.Headers[c], header: true, end: numeric, muted: true), c + 1, 0);
        }
        for (var r = 0; r < parsed.Rows.Count; r++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var row = parsed.Rows[r];
            grid.Add(Cell((r + 1).ToString(CultureInfo.InvariantCulture), header: false, end: true, muted: true), 0, r + 1);
            for (var c = 0; c < parsed.Headers.Count; c++)
            {
                var numeric = c < parsed.NumericColumns.Count && parsed.NumericColumns[c];
                grid.Add(Cell(c < row.Count ? row[c] : "", header: false, end: numeric, muted: false), c + 1, r + 1);
            }
        }

        var table = new Border { StrokeThickness = 1, Padding = 0, Content = new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = grid } };
        table.SetRadius(KlyTheme.RadiusOf(1));
        table.SetStroke(KlyTheme.Border);

        var summary = DisplayUi.NewLabel(12, KlyTheme.TextMuted);
        summary.Text = texts.CsvSummary(parsed.Rows.Count, parsed.Headers.Count, parsed.Delimiter == '\t' ? "tab" : parsed.Delimiter.ToString());
        var stack = new VerticalStackLayout { Spacing = 8, Children = { table, summary } };
        if (parsed.Truncated)
        {
            var truncated = DisplayUi.NewLabel(12, KlyTheme.TextMuted);
            truncated.Text = texts.CsvTruncated(parsed.Rows.Count);
            stack.Add(truncated);
        }
        return stack;
    }

    private View MarkdownStage(FilePreviewFile file)
    {
        var texts = DisplayTexts.Current;
        var raw = file.Content ?? "";
        var stack = new VerticalStackLayout { Spacing = 8 };
        var renderer = MarkdownRenderer;
        if (renderer is null)
        {
            stack.Add(SourceBlock(raw));
            return stack;
        }

        var preview = new DisplayTextAction(null, texts.MarkdownPreview, !_markdownSource ? KlyTheme.TextHighlighted : KlyTheme.TextMuted);
        var source = new DisplayTextAction(null, texts.MarkdownSource, _markdownSource ? KlyTheme.TextHighlighted : KlyTheme.TextMuted);
        preview.Clicked += (_, _) =>
        {
            _markdownSource = false;
            RebuildStage();
        };
        source.Clicked += (_, _) =>
        {
            _markdownSource = true;
            RebuildStage();
        };
        stack.Add(new HorizontalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.End, Children = { preview, source } });
        stack.Add(_markdownSource ? SourceBlock(raw) : Card(renderer(raw), 16));
        return stack;
    }

    private static View SourceBlock(string raw)
    {
        var label = DisplayUi.NewLabel(12, KlyTheme.TextHighlighted, FontAttributes.None, LineBreakMode.WordWrap);
        label.FontFamily = DisplayUi.MonoFont();
        label.Text = raw;
        var block = new Border { StrokeThickness = 0, Padding = new Thickness(16), Content = label };
        block.SetRadius(KlyTheme.RadiusOf(1));
        block.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
        return block;
    }

    private View TextStage(FilePreviewFile file)
    {
        var texts = DisplayTexts.Current;
        var raw = file.Content ?? "";
        var language = CodeHighlighter.ResolveFileName(file.Name ?? FilePreviewService.FileNameFromUrl(file.Src));
        var highlight = language is not null && CodeHighlighter.CanHighlight(raw, language);
        var tokens = CodeHighlighter.Tokenize(raw, highlight ? language : null);

        var count = DisplayUi.NewLabel(12, KlyTheme.TextMuted);
        count.Text = texts.LineCount(DisplayCodeLines.CountLines(raw));
        count.VerticalOptions = LayoutOptions.Center;
        var wrap = new DisplayTextAction(null, texts.WrapLines, _wrapLines ? KlyTheme.TextHighlighted : KlyTheme.TextMuted);
        wrap.Label.FontAttributes = _wrapLines ? FontAttributes.Bold : FontAttributes.None;
        wrap.Clicked += (_, _) =>
        {
            _wrapLines = !_wrapLines;
            RebuildStage();
        };
        var bar = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        bar.Add(count, 0);
        bar.Add(wrap, 1);

        var view = new DisplayCodeView();
        view.Set(tokens, lineNumbers: false, highlightLines: null, wrap: _wrapLines);
        var block = new Border { StrokeThickness = 0, Padding = new Thickness(16), Content = view };
        block.SetRadius(KlyTheme.RadiusOf(1));
        block.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
        return new VerticalStackLayout { Spacing = 8, Children = { bar, block } };
    }

    private View FallbackStage(FilePreviewFile file, string name, bool opens)
    {
        var texts = DisplayTexts.Current;
        var badge = new Border { StrokeThickness = 0, MinimumHeightRequest = 40, MinimumWidthRequest = 48, Padding = new Thickness(10, 0), HorizontalOptions = LayoutOptions.Center };
        badge.SetRadius(KlyTheme.RadiusOf(1));
        badge.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundMuted);
        var ext = DisplayUi.NewLabel(12, KlyTheme.TextHighlighted, FontAttributes.Bold, LineBreakMode.NoWrap);
        ext.Text = FilePreviewService.ExtensionLabel(name, file.Mime);
        ext.HorizontalOptions = LayoutOptions.Center;
        ext.VerticalOptions = LayoutOptions.Center;
        badge.Content = ext;

        var title = DisplayUi.NewLabel(14, KlyTheme.TextHighlighted, FontAttributes.Bold);
        title.Text = texts.NoPreview;
        title.HorizontalTextAlignment = TextAlignment.Center;
        var hint = DisplayUi.NewLabel(12, KlyTheme.TextMuted);
        hint.Text = texts.NoPreviewHint;
        hint.HorizontalTextAlignment = TextAlignment.Center;
        var button = new KlyButton { Text = opens ? texts.Open : texts.Download, Size = KlySize.Sm, HorizontalOptions = LayoutOptions.Center };
        button.Clicked += (_, _) => _ = DisplayLauncher.OpenAsync(file.DownloadUrl ?? file.Src, Opener);
        return Card(new VerticalStackLayout { Spacing = 12, Children = { badge, title, hint, button } }, 24);
    }

    private static Border Card(View content, double padding)
    {
        var card = new Border { StrokeThickness = 1, Padding = new Thickness(padding), Content = content, HorizontalOptions = LayoutOptions.Fill };
        card.SetRadius(KlyTheme.RadiusOf(0.75));
        card.SetStroke(KlyTheme.Border);
        card.SetThemeColor(BackgroundColorProperty, KlyTheme.BackgroundElevated);
        return card;
    }

    // --- the image -------------------------------------------------------------------------------------------------------------------

    private void ZoomIn()
    {
        _zoom = Math.Min(4.0, Math.Round(_zoom * 1.5, 2));
        ApplyImageTransform();
    }

    private void ZoomOut()
    {
        _zoom = Math.Max(0.5, Math.Round(_zoom / 1.5, 2));
        ApplyImageTransform();
    }

    private void RotateImage()
    {
        _zoom = 1.0;
        _rotation = (_rotation + 90) % 360;
        ApplyImageTransform();
    }

    private void ResetImageTransform()
    {
        _zoom = 1.0;
        _rotation = 0;
        ApplyImageTransform();
    }

    private void ApplyImageTransform()
    {
        if (_image is { } image)
        {
            image.Scale = _zoom;
            image.Rotation = _rotation;
        }
        _zoomReset?.SetText($"{Math.Round(_zoom * 100)}%");
    }

    // --- interaction hooks for the tests ----------------------------------------------------------------------------------------------

    internal void PressZoomIn() => ZoomIn();

    internal void PressZoomOut() => ZoomOut();

    internal void PressRotate() => RotateImage();
}
