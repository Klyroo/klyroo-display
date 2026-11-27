using System.Globalization;

namespace Klyroo.Display.Maui.Tests;

public class FileTests
{
    static FileTests()
    {
        TestSupport.InstallDispatcher();
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static FilePreviewFile File(string src, string? name = null, long? size = null, string? mime = null, string? content = null) =>
        new() { Src = src, Name = name, Size = size, Mime = mime, Content = content };

    private static string RowTitle(Border row) => SemanticProperties.GetDescription(row);

    private static List<string> RowTexts(Border row) => TestSupport.Texts(row);

    // --- KlyFileList ------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void File_list_is_empty_until_it_has_files()
    {
        var list = new KlyFileList();

        Assert.Empty(list.Rows);
        Assert.Null(list.MoreButton);
        Assert.Null(list.Files);
        Assert.Null(list.Max);
        Assert.Null(list.Step);
        Assert.Null(list.Preview);
    }

    [Fact]
    public void File_list_row_shows_the_extension_badge_the_name_the_size_and_the_date()
    {
        var updated = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero);
        var list = new KlyFileList
        {
            Files = [new FilePreviewFile { Src = "https://x.test/docs/report.pdf", Size = 2_621_440, UpdatedAt = updated }],
        };

        var row = Assert.Single(list.Rows);
        Assert.Equal(["report.pdf", "2.5 MB", updated.ToString("d", CultureInfo.CurrentCulture)], RowTexts(row).Where(t => t != "PDF").ToArray());
        Assert.Contains("PDF", RowTexts(row));
        Assert.Equal("report.pdf", SemanticProperties.GetDescription(row));
        Assert.Equal("2.5 MB", SemanticProperties.GetHint(row));
    }

    [Fact]
    public void File_list_name_falls_back_to_the_last_segment_of_the_address_and_the_label_to_the_mime_type()
    {
        var list = new KlyFileList { Files = [File("https://x.test/files/a%20b", mime: "application/zip")] };

        var row = Assert.Single(list.Rows);
        Assert.Equal("a b", RowTitle(row));
        Assert.Contains("ZIP", RowTexts(row));
    }

    [Fact]
    public void File_list_description_goes_before_the_size_and_meta_replaces_the_date()
    {
        var item = new FileListItem(
            new FilePreviewFile { Src = "/a.csv", Size = 512, UpdatedAt = DateTimeOffset.UtcNow },
            Description: "Q1 export",
            Meta: "Yesterday");
        var list = new KlyFileList { Files = [item] };

        var texts = RowTexts(Assert.Single(list.Rows));
        Assert.Contains("Q1 export · 512 B", texts);
        Assert.Contains("Yesterday", texts);
        Assert.DoesNotContain(texts, t => t.Contains('/') && t != "/a.csv");
    }

    [Fact]
    public void File_list_icon_replaces_the_extension_and_the_tone_tints_the_badge()
    {
        var list = new KlyFileList { Files = [new FileListItem(File("/a.pdf"), Icon: KlyIconNames.Folder, Tone: KlyColor.Success)] };

        var row = Assert.Single(list.Rows);
        var icon = Assert.Single(TestSupport.All<KlyIcon>(row));
        Assert.Equal("folder", icon.Name);
        Assert.Equal(KlyTheme.Resolve(KlyColor.Success).Text, icon.IconColor);
        Assert.DoesNotContain("PDF", RowTexts(row));
    }

    [Fact]
    public void File_list_title_is_shown_above_the_rows()
    {
        var list = new KlyFileList { Title = "Attachments", Files = [File("/a.txt")] };

        Assert.Equal("Attachments", TestSupport.Texts(list).First());
        Assert.Single(list.Rows);
    }

    [Fact]
    public void A_file_converts_to_an_item_by_itself()
    {
        var file = File("/a.pdf");
        FileListItem item = file;

        Assert.Same(file, item.File);
        Assert.Same(file, FileListItem.FromFile(file).File);
        var list = new KlyFileList { Files = [file] };
        Assert.Single(list.Rows);
    }

    [Fact]
    public void File_list_shows_max_files_then_reveals_step_more_with_each_press()
    {
        var list = new KlyFileList { Files = Enumerable.Range(1, 5).Select(i => (FileListItem)File($"/f{i}.txt")).ToList(), Max = 2, Step = 2 };

        Assert.Equal(2, list.Rows.Count);
        Assert.Equal("Show more · 2", list.MoreButton!.Text);

        list.MoreButton.SendClicked();
        Assert.Equal(4, list.Rows.Count);
        Assert.Equal("Show more · 1", list.MoreButton!.Text);

        list.MoreButton.SendClicked();
        Assert.Equal(5, list.Rows.Count);
        Assert.Null(list.MoreButton);
    }

    [Fact]
    public void File_list_without_a_step_reveals_everything_at_once()
    {
        var list = new KlyFileList { Files = Enumerable.Range(1, 6).Select(i => (FileListItem)File($"/f{i}.txt")).ToList(), Max = 2 };

        Assert.Equal("Show more · 4", list.MoreButton!.Text);
        list.MoreButton.SendClicked();
        Assert.Equal(6, list.Rows.Count);
        Assert.Null(list.MoreButton);
    }

    [Fact]
    public void File_list_with_no_max_or_a_max_that_covers_the_files_has_no_more_button()
    {
        var files = Enumerable.Range(1, 3).Select(i => (FileListItem)File($"/f{i}.txt")).ToList();

        Assert.Null(new KlyFileList { Files = files }.MoreButton);
        Assert.Null(new KlyFileList { Files = files, Max = 3 }.MoreButton);
        Assert.Null(new KlyFileList { Files = files, Max = 0 }.MoreButton);
    }

    [Fact]
    public void File_list_raising_max_keeps_what_was_revealed()
    {
        var list = new KlyFileList { Files = Enumerable.Range(1, 6).Select(i => (FileListItem)File($"/f{i}.txt")).ToList(), Max = 2, Step = 3 };
        list.MoreButton!.SendClicked();
        Assert.Equal(5, list.Rows.Count);

        list.Max = 1;
        Assert.Equal(5, list.Rows.Count);
    }

    [Fact]
    public void File_list_more_button_uses_the_texts_and_shows_a_chevron_after_them()
    {
        TestSupport.WithTexts(new DisplayTexts { ShowMore = n => $"Voir {n} de plus" }, () =>
        {
            var list = new KlyFileList { Files = Enumerable.Range(1, 3).Select(i => (FileListItem)File($"/f{i}.txt")).ToList(), Max = 1 };
            Assert.Equal("Voir 2 de plus", list.MoreButton!.Text);
            Assert.Equal("chevron-down", list.MoreButton.Icon.Name);
            Assert.True(list.MoreButton.MinimumHeightRequest >= 40);
        });
    }

    [Fact]
    public void File_list_tap_raises_file_selected_once_with_the_item_and_its_index()
    {
        var files = new List<FileListItem> { File("/a.txt"), File("/b.txt"), File("/c.txt") };
        var list = new KlyFileList { Files = files };
        var raised = new List<KlyFileSelectedEventArgs>();
        list.FileSelected += (_, e) => raised.Add(e);

        TestSupport.Tap(list.Rows[1]);

        var e = Assert.Single(raised);
        Assert.Same(files[1], e.Item);
        Assert.Equal(1, e.Index);
        Assert.Same(files, e.Items);
    }

    [Fact]
    public void File_list_tap_opens_the_whole_list_as_a_gallery_in_the_preview_service()
    {
        var service = new FilePreviewService();
        var list = new KlyFileList { Files = [File("/a.png"), File("/b.png"), File("/c.png")], Preview = service };

        TestSupport.Tap(list.Rows[2]);

        Assert.True(service.IsOpen);
        Assert.Equal(3, service.Files.Count);
        Assert.Equal(2, service.CurrentIndex);
        Assert.Equal("/c.png", service.CurrentFile!.Src);
    }

    [Fact]
    public void File_list_tap_without_a_preview_service_only_raises_the_event()
    {
        var list = new KlyFileList { Files = [File("/a.png")] };
        var raised = 0;
        list.FileSelected += (_, _) => raised++;

        TestSupport.Tap(list.Rows[0]);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void File_list_rows_are_touch_targets()
    {
        var list = new KlyFileList { Files = [File("/a.png")] };
        var grid = Assert.IsType<Grid>(Assert.Single(list.Rows).Content);

        Assert.True(grid.MinimumHeightRequest >= 40);
    }

    // --- KlyFilePreviewProvider -------------------------------------------------------------------------------------------------------------

    private static (KlyOverlayHost Host, KlyFilePreviewProvider Provider) Hosted(Action<KlyFilePreviewProvider>? configure = null)
    {
        var provider = new KlyFilePreviewProvider();
        configure?.Invoke(provider);
        var host = new KlyOverlayHost { Content = new VerticalStackLayout { Children = { provider } } };
        return (host, provider);
    }

    [Fact]
    public void Provider_starts_with_a_service_of_its_own_and_shows_nothing()
    {
        var (host, provider) = Hosted();

        Assert.NotNull(provider.Service);
        Assert.False(provider.Service.IsOpen);
        Assert.Empty(host.Layers);
        Assert.Null(provider.ShownOverlay);
    }

    [Fact]
    public void Provider_opens_a_modal_for_an_image_and_closes_with_the_service()
    {
        var (host, provider) = Hosted();

        provider.Service.Open(File("/photos/cat.png", size: 2048, mime: "image/png"));

        Assert.Single(host.Layers);
        Assert.Same(provider.Modal, provider.ShownOverlay);
        Assert.Equal("cat.png", provider.Modal.Title);
        Assert.Equal("Image · 2.0 KB", provider.Modal.Description);
        Assert.Equal(KlySize.Xl, provider.Modal.Size);
        Assert.Equal("Close preview", provider.Modal.CloseLabel);

        provider.Service.Close();
        Assert.Empty(host.Layers);
        Assert.Null(provider.ShownOverlay);
    }

    [Theory]
    [InlineData("/a.pdf", FilePreviewKind.Pdf)]
    [InlineData("/a.csv", FilePreviewKind.Csv)]
    [InlineData("/a.md", FilePreviewKind.Markdown)]
    [InlineData("/a.cs", FilePreviewKind.Text)]
    public void Provider_uses_a_slideover_for_the_kinds_that_suit_a_drawer(string src, FilePreviewKind kind)
    {
        var (host, provider) = Hosted();

        provider.Service.Open(File(src, content: "x"));

        Assert.Equal(kind, FilePreviewService.DetectKind(src));
        Assert.Same(provider.Drawer, provider.ShownOverlay);
        Assert.Single(host.Layers);
    }

    [Fact]
    public void Provider_honors_an_explicit_container()
    {
        var (_, provider) = Hosted();

        provider.Service.Open(File("/a.pdf"), new FilePreviewOptions { Mode = FilePreviewContainer.Modal });
        Assert.Same(provider.Modal, provider.ShownOverlay);

        provider.Service.Close();
        provider.Service.Open(File("/a.png"), new FilePreviewOptions { Mode = FilePreviewContainer.Drawer });
        Assert.Same(provider.Drawer, provider.ShownOverlay);
    }

    [Fact]
    public void Provider_dismissing_the_overlay_closes_the_service()
    {
        var (host, provider) = Hosted();
        provider.Service.Open(File("/a.png"));
        Assert.True(provider.Service.IsOpen);

        host.DismissTop();

        Assert.False(provider.Service.IsOpen);
        Assert.Empty(host.Layers);
    }

    [Fact]
    public void Provider_throws_a_clear_error_when_there_is_no_overlay_host()
    {
        var provider = new KlyFilePreviewProvider();
        var page = new VerticalStackLayout { Children = { provider } };
        Assert.NotNull(page);

        var error = Assert.Throws<InvalidOperationException>(() => provider.Service.Open(File("/a.png")));

        Assert.Contains("KlyOverlayHost", error.Message);
    }

    [Fact]
    public void Provider_swaps_the_service_and_follows_the_new_one()
    {
        var (host, provider) = Hosted();
        var first = provider.Service;
        var second = new FilePreviewService();

        provider.Service = second;
        first.Open(File("/ignored.png"));
        Assert.Empty(host.Layers);

        second.Open(File("/a.png"));
        Assert.Single(host.Layers);
    }

    [Fact]
    public void Provider_gallery_has_a_counter_previous_and_next_and_a_strip()
    {
        var (_, provider) = Hosted();
        provider.Service.OpenGallery([File("/1.png"), File("/2.png"), File("/3.png")]);

        var texts = TestSupport.Texts(provider.Toolbar);
        Assert.Contains("1 / 3", texts);
        var buttons = TestSupport.All<DisplayIconButton>(provider.Toolbar);
        Assert.Equal(["Previous file", "Next file"], buttons.Select(b => SemanticProperties.GetDescription(b)).ToArray());
        Assert.False(buttons[0].IsEnabled);
        Assert.True(buttons[1].IsEnabled);
        Assert.True(provider.Strip.IsVisible);
        Assert.Equal("1 of 3: 1.png", SemanticProperties.GetDescription(provider.Body));

        buttons[1].SendClicked();

        Assert.Equal("2.png", provider.Modal.Title);
        Assert.Contains("2 / 3", TestSupport.Texts(provider.Toolbar));
        Assert.True(TestSupport.All<DisplayIconButton>(provider.Toolbar).All(b => b.IsEnabled));
        Assert.Equal("2 of 3: 2.png", SemanticProperties.GetDescription(provider.Body));
    }

    [Fact]
    public void Provider_gallery_ends_disable_the_buttons_unless_it_loops()
    {
        var (_, provider) = Hosted();
        provider.Service.OpenGallery([File("/1.png"), File("/2.png")], new FilePreviewOptions { Index = 1 });

        var buttons = TestSupport.All<DisplayIconButton>(provider.Toolbar);
        Assert.True(buttons[0].IsEnabled);
        Assert.False(buttons[1].IsEnabled);

        provider.Service.Close();
        provider.Service.OpenGallery([File("/1.png"), File("/2.png")], new FilePreviewOptions { Index = 1, Loop = true });
        Assert.True(TestSupport.All<DisplayIconButton>(provider.Toolbar).All(b => b.IsEnabled));
    }

    [Fact]
    public void Provider_strip_thumbnails_jump_to_a_file_and_mark_the_current_one()
    {
        var (_, provider) = Hosted();
        provider.Service.OpenGallery([File("/1.png"), File("/2.png"), File("/3.png")]);

        var chips = TestSupport.All<Border>(provider.Strip).Where(b => b.GestureRecognizers.Count > 0).ToList();
        Assert.Equal(3, chips.Count);
        Assert.Equal("1 of 3: 1.png", SemanticProperties.GetHint(chips[0]));
        Assert.Null(SemanticProperties.GetHint(chips[1]));

        TestSupport.Tap(chips[2]);

        Assert.Equal(2, provider.Service.CurrentIndex);
        Assert.Equal("3.png", provider.Modal.Title);
    }

    [Fact]
    public void Provider_single_file_has_no_strip_and_no_counter()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/1.png"));

        Assert.False(provider.Strip.IsVisible);
        Assert.Empty(TestSupport.All<DisplayIconButton>(provider.Toolbar));
        Assert.Null(SemanticProperties.GetDescription(provider.Body));
    }

    [Fact]
    public void Provider_image_zoom_stays_between_half_and_four_times_and_rotating_resets_it()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/cat.png"));
        var image = provider.CurrentImage!;
        Assert.Equal(1, image.Scale);
        Assert.Equal("cat.png", SemanticProperties.GetDescription(image));

        provider.PressZoomIn();
        Assert.Equal(1.5, provider.Zoom);
        Assert.Equal(1.5, image.Scale);
        Assert.Contains("150%", TestSupport.Texts(provider.Toolbar));

        for (var i = 0; i < 10; i++) provider.PressZoomIn();
        Assert.Equal(4, provider.Zoom);

        for (var i = 0; i < 20; i++) provider.PressZoomOut();
        Assert.Equal(0.5, provider.Zoom);

        provider.PressRotate();
        Assert.Equal(1, provider.Zoom);
        Assert.Equal(90, provider.ImageRotation);
        Assert.Equal(90, image.Rotation);
    }

    [Fact]
    public void Provider_image_toolbar_has_accessible_zoom_and_rotate_controls_and_reset_restores_them()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/cat.png"));

        var actions = TestSupport.All<DisplayTextAction>(provider.Toolbar);
        var names = actions.Select(a => SemanticProperties.GetDescription(a)).ToArray();
        Assert.Contains("Zoom out", names);
        Assert.Contains("Reset zoom", names);
        Assert.Contains("Zoom in", names);
        Assert.Contains("Rotate 90 degrees", names);

        provider.PressZoomIn();
        provider.PressRotate();
        actions.Single(a => SemanticProperties.GetDescription(a) == "Reset zoom").SendClicked();

        Assert.Equal(1, provider.Zoom);
        Assert.Equal(0, provider.ImageRotation);
        Assert.Contains("100%", TestSupport.Texts(provider.Toolbar));
    }

    [Fact]
    public void Provider_navigation_resets_the_image_transform()
    {
        var (_, provider) = Hosted();
        provider.Service.OpenGallery([File("/1.png"), File("/2.png")]);
        provider.PressZoomIn();
        provider.PressRotate();

        provider.Service.Next();

        Assert.Equal(1, provider.Zoom);
        Assert.Equal(0, provider.ImageRotation);
        Assert.Equal(1, provider.CurrentImage!.Scale);
    }

    [Fact]
    public void Provider_toolbar_has_no_image_controls_for_other_kinds()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/a.txt", content: "x"));

        var names = TestSupport.All<DisplayTextAction>(provider.Toolbar).Select(a => SemanticProperties.GetDescription(a)).ToArray();
        Assert.DoesNotContain("Zoom in", names);
        Assert.Null(provider.CurrentImage);
    }

    [Fact]
    public void Provider_details_toggle_shows_name_type_size_and_the_extra_rows()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(new FilePreviewFile
        {
            Src = "/a.png",
            Name = "photo.png",
            Mime = "image/png",
            Size = 1024,
            Details = [new FilePreviewDetail("Uploaded by", "Ana")],
        });
        Assert.False(provider.Details.IsVisible);
        Assert.False(provider.ShowsDetails);

        TestSupport.All<DisplayTextAction>(provider.Toolbar).Single(a => a.Text == "Details").SendClicked();

        Assert.True(provider.Details.IsVisible);
        var texts = TestSupport.Texts(provider.Details);
        Assert.Equal(["Details", "Name", "photo.png", "Type", "image/png", "Size", "1.0 KB", "Uploaded by", "Ana"], texts.Select(t => t == "DETAILS" ? "Details" : t).ToArray());
        Assert.Equal(SemanticHeadingLevel.Level2, SemanticProperties.GetHeadingLevel(TestSupport.All<Label>(provider.Details).First()));

        TestSupport.All<DisplayTextAction>(provider.Toolbar).Single(a => a.Text == "Details").SendClicked();
        Assert.False(provider.Details.IsVisible);
    }

    [Fact]
    public void Provider_details_type_falls_back_to_the_kind_when_the_mime_type_is_unknown()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/a.png", name: "a.png"));
        TestSupport.All<DisplayTextAction>(provider.Toolbar).Single(a => a.Text == "Details").SendClicked();

        Assert.Contains("Image", TestSupport.Texts(provider.Details));
    }

    [Fact]
    public void Provider_custom_actions_open_their_link_and_call_back_with_the_file()
    {
        var (_, provider) = Hosted();
        var opened = new List<string>();
        provider.Opener = src =>
        {
            opened.Add(src);
            return Task.CompletedTask;
        };
        FilePreviewFile? received = null;
        var file = new FilePreviewFile
        {
            Src = "/a.png",
            Actions =
            [
                new FilePreviewAction { Label = "Share", Icon = KlyIconNames.External, Href = "https://x.test/share", OnSelect = f => { received = f; return Task.CompletedTask; } },
                new FilePreviewAction { Label = "Plain" },
            ],
        };
        provider.Service.Open(file);

        var share = TestSupport.All<DisplayTextAction>(provider.Toolbar).Single(a => a.Text == "Share");
        Assert.Equal("arrow-up-right", share.Icon.Name);
        share.SendClicked();

        Assert.Equal(["https://x.test/share"], opened);
        Assert.Same(file, received);
        Assert.Contains(TestSupport.All<DisplayTextAction>(provider.Toolbar), a => a.Text == "Plain");
    }

    [Fact]
    public void Provider_download_opens_the_download_address_or_the_source()
    {
        var (_, provider) = Hosted();
        var opened = new List<string>();
        provider.Opener = src =>
        {
            opened.Add(src);
            return Task.CompletedTask;
        };

        provider.Service.Open(new FilePreviewFile { Src = "/a.png", DownloadUrl = "https://x.test/dl/a.png" });
        TestSupport.All<DisplayTextAction>(provider.Toolbar).Single(a => a.Text == "Download").SendClicked();
        provider.Service.Close();
        provider.Service.Open(File("https://x.test/b.png"));
        TestSupport.All<DisplayTextAction>(provider.Toolbar).Single(a => a.Text == "Download").SendClicked();

        Assert.Equal(["https://x.test/dl/a.png", "https://x.test/b.png"], opened);
    }

    [Fact]
    public void Provider_rendition_shows_a_notice_and_draws_the_rendition()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(new FilePreviewFile { Src = "/a.docx", Rendition = File("/a.png") });

        Assert.True(provider.Banner.IsVisible);
        Assert.Contains("Converted preview · The original is a Office file.", TestSupport.Texts(provider.Banner));
        Assert.Contains("Download original", TestSupport.Texts(provider.Banner));
        Assert.NotNull(provider.CurrentImage);
        Assert.Equal("Image", provider.Modal.Description);
    }

    [Fact]
    public void Provider_without_a_rendition_has_no_notice()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/a.png"));

        Assert.False(provider.Banner.IsVisible);
        Assert.Null(provider.Banner.Content);
    }

    [Theory]
    [InlineData("/a.docx")]
    [InlineData("/a.zip")]
    [InlineData("/a.unknown")]
    public void Provider_offers_a_download_for_a_file_without_a_renderer(string src)
    {
        var (_, provider) = Hosted();
        var opened = new List<string>();
        provider.Opener = s =>
        {
            opened.Add(s);
            return Task.CompletedTask;
        };
        provider.Service.Open(new FilePreviewFile { Src = src, DownloadUrl = "https://x.test/dl" });

        var texts = TestSupport.Texts(provider.Stage);
        Assert.Contains("No preview available", texts);
        Assert.Contains("Download this file to open it in another application.", texts);
        var button = Assert.Single(TestSupport.All<KlyButton>(provider.Stage));
        Assert.Equal("Download", button.Text);
        TestSupport.Click(button);

        Assert.Equal(["https://x.test/dl"], opened);
    }

    [Fact]
    public void Provider_pdf_has_no_native_renderer_and_offers_to_open_it()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/a.pdf"));

        var button = Assert.Single(TestSupport.All<KlyButton>(provider.Stage));
        Assert.Equal("Open", button.Text);
        Assert.Contains("PDF", TestSupport.Texts(provider.Stage));
    }

    [Fact]
    public void Provider_csv_stage_draws_the_table_and_the_summary()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/data.csv", content: "Item;Price\nDesk;299.90\nChair;149.00"));

        var texts = TestSupport.Texts(provider.Stage);
        Assert.Contains("Item", texts);
        Assert.Contains("Price", texts);
        Assert.Contains("Desk", texts);
        Assert.Contains("149.00", texts);
        Assert.Contains("2 rows · 2 columns · delimiter ';'", texts);
        var price = TestSupport.All<Label>(provider.Stage).Single(l => l.Text == "299.90");
        Assert.Equal(TextAlignment.End, price.HorizontalTextAlignment);
        var item = TestSupport.All<Label>(provider.Stage).Single(l => l.Text == "Desk");
        Assert.Equal(TextAlignment.Start, item.HorizontalTextAlignment);
    }

    [Fact]
    public void Provider_csv_stage_keeps_the_first_rows_and_says_so()
    {
        var (_, provider) = Hosted(p => p.CsvMaxRows = 3);
        var rows = string.Join('\n', Enumerable.Range(1, 10).Select(i => $"r{i},{i}"));
        provider.Service.Open(File("/data.csv", content: "name,n\n" + rows));

        var texts = TestSupport.Texts(provider.Stage);
        Assert.Contains("3 rows · 2 columns · delimiter ','", texts);
        Assert.Contains("Showing first 3 rows", texts);
        Assert.DoesNotContain("r4", texts);
    }

    [Fact]
    public void Provider_text_stage_shows_the_line_count_and_highlights_known_extensions()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/Program.cs", content: "var x = 1;\nreturn;\n"));

        Assert.Contains("2 lines", TestSupport.Texts(provider.Stage));
        var view = TestSupport.All<DisplayCodeView>(provider.Stage).Single();
        Assert.Contains(view.Tokens, t => t.Kind == CodeTokenKind.Keyword);
        Assert.Equal("var x = 1;\nreturn;\n", string.Concat(view.Tokens.Select(t => t.Text)));
        Assert.False(view.Wrap);
    }

    [Fact]
    public void Provider_text_stage_is_plain_for_an_unknown_extension_and_wrap_toggles()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/notes.txt", content: "just text"));

        var view = TestSupport.All<DisplayCodeView>(provider.Stage).Single();
        Assert.Equal([new CodeToken(CodeTokenKind.Plain, "just text")], view.Tokens);
        Assert.Contains("1 line", TestSupport.Texts(provider.Stage));

        TestSupport.All<DisplayTextAction>(provider.Stage).Single(a => a.Text == "Wrap lines").SendClicked();

        Assert.True(TestSupport.All<DisplayCodeView>(provider.Stage).Single().Wrap);
    }

    [Fact]
    public void Provider_markdown_is_source_without_a_renderer()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(File("/a.md", content: "# Title"));

        Assert.Contains("# Title", TestSupport.Texts(provider.Stage));
        Assert.Empty(TestSupport.All<DisplayTextAction>(provider.Stage));
    }

    [Fact]
    public void Provider_markdown_with_a_renderer_has_a_preview_and_a_source_tab()
    {
        var (_, provider) = Hosted(p => p.MarkdownRenderer = raw => new Label { Text = "rendered:" + raw });
        provider.Service.Open(File("/a.md", content: "# Title"));

        Assert.Contains("rendered:# Title", TestSupport.Texts(provider.Stage));
        var tabs = TestSupport.All<DisplayTextAction>(provider.Stage);
        Assert.Equal(["Preview", "Source"], tabs.Select(t => t.Text).ToArray());

        tabs[1].SendClicked();
        var texts = TestSupport.Texts(provider.Stage);
        Assert.Contains("# Title", texts);
        Assert.DoesNotContain("rendered:# Title", texts);

        TestSupport.All<DisplayTextAction>(provider.Stage)[0].SendClicked();
        Assert.Contains("rendered:# Title", TestSupport.Texts(provider.Stage));
    }

    [Fact]
    public void Provider_video_and_audio_play_through_the_playback_of_the_app_and_release_it()
    {
        var (_, provider) = Hosted();
        var made = new List<FakePlayback>();
        provider.PlaybackFactory = file =>
        {
            var playback = new FakePlayback();
            made.Add(playback);
            return playback;
        };

        provider.Service.Open(File("/a.mp4"));
        var video = Assert.IsType<KlyVideoPlayer>(provider.Stage.Content);
        Assert.Same(made[0], video.Playback);
        Assert.Equal("a.mp4", video.Title);
        Assert.Equal("/a.mp4", video.Src);
        Assert.Same(made[0], provider.OwnedPlayback);

        provider.Service.Close();
        Assert.True(made[0].Disposed);
        Assert.Null(provider.OwnedPlayback);

        provider.Service.Open(File("/a.mp3"));
        var audioCard = Assert.IsType<Border>(provider.Stage.Content);
        var audio = Assert.Single(TestSupport.All<KlyAudioPlayer>(audioCard));
        Assert.Same(made[1], audio.Playback);
        Assert.Contains("a.mp3", TestSupport.Texts(audioCard));
    }

    [Fact]
    public void Provider_disposes_the_playback_of_the_file_it_leaves()
    {
        var (_, provider) = Hosted();
        var made = new List<FakePlayback>();
        provider.PlaybackFactory = _ =>
        {
            var playback = new FakePlayback();
            made.Add(playback);
            return playback;
        };
        provider.Service.OpenGallery([File("/a.mp4"), File("/b.mp4")]);

        provider.Service.Next();

        Assert.True(made[0].Disposed);
        Assert.False(made[1].Disposed);
    }

    [Fact]
    public void Provider_video_without_a_factory_uses_the_default_playback()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(new FilePreviewFile { Src = "/a.mp4", Poster = "/p.png", Tracks = [new VideoPlayerTrack("/fr.vtt", "fr", "Français")] });

        var video = Assert.IsType<KlyVideoPlayer>(provider.Stage.Content);
        Assert.Null(video.Playback);
        Assert.Equal("/p.png", video.Poster);
        Assert.Single(video.Tracks!);
    }

    [Fact]
    public void Provider_audio_stage_shows_the_cover()
    {
        var (_, provider) = Hosted();
        provider.Service.Open(new FilePreviewFile { Src = "/a.mp3", Poster = "/cover.png" });

        var images = TestSupport.All<Image>(provider.Stage);
        Assert.Single(images);
        Assert.Equal(128, images[0].WidthRequest);
    }

    [Fact]
    public void Provider_texts_follow_DisplayTexts()
    {
        TestSupport.WithTexts(new DisplayTexts { Details = "Détails", ClosePreview = "Fermer", PreviousFile = "Précédent", NextFile = "Suivant" }, () =>
        {
            var (_, provider) = Hosted();
            provider.Service.OpenGallery([File("/1.png"), File("/2.png")]);

            Assert.Equal("Fermer", provider.Modal.CloseLabel);
            Assert.Contains(TestSupport.All<DisplayTextAction>(provider.Toolbar), a => a.Text == "Détails");
            Assert.Equal(["Précédent", "Suivant"], TestSupport.All<DisplayIconButton>(provider.Toolbar).Select(b => SemanticProperties.GetDescription(b)).ToArray());
        });
    }

    [Fact]
    public void Provider_meta_line_has_the_kind_the_size_and_the_date()
    {
        var file = new FilePreviewFile { Src = "/a.pdf", Size = 1_572_864, UpdatedAt = new DateTimeOffset(2026, 1, 9, 0, 0, 0, TimeSpan.Zero) };

        Assert.Equal("Pdf · 1.5 MB · 2026-01-09", KlyFilePreviewProvider.MetaLine(file));
        Assert.Equal("Other", KlyFilePreviewProvider.MetaLine(File("/a.zzz")));
    }

    [Fact]
    public void Provider_closing_the_service_while_closed_does_nothing()
    {
        var (host, provider) = Hosted();

        provider.Service.Close();

        Assert.Empty(host.Layers);
    }
}
