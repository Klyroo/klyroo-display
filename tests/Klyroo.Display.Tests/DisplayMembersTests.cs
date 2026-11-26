using System.Globalization;
using Bunit;
using Klyroo.UI;
using Xunit;

namespace Klyroo.Display.Tests;

// Members that were declared but not rendered, and the layout fixes of KlyDisplay.
public sealed class DisplayMembersTests : BunitContext
{
    public DisplayMembersTests()
    {
        Services.AddKlyrooDisplay();
    }

    [Theory]
    [InlineData(2, "H2")]
    [InlineData(3, "H3")]
    [InlineData(0, "H1")]
    [InlineData(9, "H6")]
    public void HeadingLevel_selects_the_heading_element(int level, string tag)
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Heading)
            .Add(x => x.Title, "Billing")
            .Add(x => x.HeadingLevel, level));

        var title = cut.Find("[data-slot=heading-title]");
        Assert.Equal(tag, title.TagName);
        Assert.Equal("Billing", title.TextContent);
    }

    [Fact]
    public void Breakdown_compare_columns_line_up_both_amounts_with_detail_tone_and_chip()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Breakdown)
            .Add(x => x.CompareColumns, ("2026", "2025"))
            .Add(x => x.BreakdownLines, [
                new DisplayBreakdownLine("Licenses", "€900", CompareValue: "€800", Detail: "12 seats", Chip: new DisplayChip("New")),
                new DisplayBreakdownLine("Discount", "-€100", Tone: KlyColor.Success),
            ])
            .Add(x => x.BreakdownTotal, new DisplayBreakdownLine("Total", "€800", CompareValue: "€800")));

        var header = cut.Find("[data-slot=breakdown-columns]");
        Assert.Contains("2026", header.TextContent);
        Assert.Contains("2025", header.TextContent);
        var lines = cut.FindAll("[data-slot=breakdown-line]");
        Assert.Equal("€800", lines[0].QuerySelector("[data-slot=breakdown-compare]")!.TextContent);
        Assert.Equal("—", lines[1].QuerySelector("[data-slot=breakdown-compare]")!.TextContent);
        Assert.Equal("12 seats", lines[0].QuerySelector("[data-slot=breakdown-detail]")!.TextContent);
        Assert.Contains("New", lines[0].QuerySelector("[data-slot=display-chip]")!.TextContent);
        Assert.Equal("success", lines[1].QuerySelector("[data-slot=breakdown-value]")!.GetAttribute("data-color"));
        Assert.NotNull(cut.Find("[data-slot=breakdown-total] [data-slot=breakdown-compare]"));

        // A second amount alone also switches to two columns, without headers.
        var plain = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Breakdown)
            .Add(x => x.BreakdownLines, [new DisplayBreakdownLine("Fees", "€10", CompareValue: "€12")]));
        Assert.Equal("true", plain.Find("[data-slot=display-breakdown]").GetAttribute("data-compare"));
        Assert.Empty(plain.FindAll("[data-slot=breakdown-columns]"));
    }

    [Fact]
    public void Chip_icon_hint_and_tone_of_facts_are_rendered()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Facts)
            .Add(x => x.Facts, [
                new DisplayFact { Label = "Status", Chip = new DisplayChip("Paid", KlyColor.Success, Icon: "check") },
                new DisplayFact { Label = "Balance", Value = "-€40", Hint = "Due on the 5th", Tone = KlyColor.Error },
            ]));

        Assert.NotNull(cut.Find("[data-slot=display-chip] svg"));
        Assert.Equal("Due on the 5th", cut.Find("[data-slot=fact-hint]").TextContent);
        var value = cut.FindAll("[data-slot=fact-value]")[1];
        Assert.Equal("error", value.GetAttribute("data-color"));
        Assert.Contains("text-(--kly-color-text)", value.ClassList);
    }

    [Fact]
    public void Stat_row_shows_its_title_and_item_tones()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Stat)
            .Add(x => x.Variant, DisplayVariant.Row)
            .Add(x => x.Title, "This week")
            .Add(x => x.StatItems, [new DisplayStatItem("Errors", "12", Tone: KlyColor.Error), new DisplayStatItem("Users", "340")]));

        Assert.Equal("This week", cut.Find("[data-slot=stat-row-title]").TextContent);
        var values = cut.FindAll("[data-slot=stat-item-value]");
        Assert.Equal("error", values[0].GetAttribute("data-color"));
        Assert.Null(values[1].GetAttribute("data-color"));
    }

    [Fact]
    public void Stat_with_a_variant_of_another_kind_keeps_the_muted_surface()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Stat)
            .Add(x => x.Variant, DisplayVariant.Gallery)
            .Add(x => x.Value, "42"));

        Assert.Equal("muted", cut.Find("[data-slot=display]").GetAttribute("data-surface"));
    }

    [Fact]
    public void Meter_parts_have_tones_a_legend_and_a_cap()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Meter)
            .Add(x => x.MeterMax, 10)
            .Add(x => x.MeterParts, [
                new DisplayMeterPart("Photos", 3, ValueLabel: "3 GB", Tone: KlyColor.Info, Icon: "image"),
                new DisplayMeterPart("Video", 9, Max: 5),
            ]));

        var parts = cut.FindAll("[data-slot=meter-part]");
        Assert.Equal("info", parts[0].GetAttribute("data-color"));
        Assert.Equal("width:50%", parts[1].GetAttribute("style"));
        var legend = cut.FindAll("[data-slot=meter-legend-item]");
        Assert.Contains("3 GB", legend[0].TextContent);
        Assert.NotNull(legend[0].QuerySelector("svg"));
        Assert.Contains("9 / 5", legend[1].TextContent);
    }

    [Fact]
    public void Ring_meter_draws_the_value_and_follows_the_thresholds()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Meter)
            .Add(x => x.Variant, DisplayVariant.Ring)
            .Add(x => x.Title, "Quota")
            .Add(x => x.MeterValue, 92)
            .Add(x => x.ErrorThreshold, 90));

        Assert.NotNull(cut.Find("[data-slot=meter-ring] [role=meter]"));
        var arc = cut.Find("[data-slot=meter-ring-value]");
        Assert.Equal("92 8", arc.GetAttribute("stroke-dasharray"));
        Assert.Contains("text-(--klyroo-error)", arc.ClassList);
        Assert.Equal("92%", cut.Find("[data-slot=meter-value]").TextContent);
        Assert.Empty(cut.FindAll("[data-slot=meter-track]"));
    }

    [Fact]
    public void Sticky_table_keeps_its_header_in_a_scrolling_box()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Table)
            .Add(x => x.Variant, DisplayVariant.Sticky)
            .Add(x => x.TableColumns, [new DisplayTableColumn("id", "Id")])
            .Add(x => x.TableRows, [new Dictionary<string, string?> { ["id"] = "1" }]));

        Assert.Contains("sticky", cut.Find("[data-slot=display-table-head]").ClassList);
        Assert.Contains("max-h-80", cut.Find("[data-slot=display-table-scroll]").ClassList);

        var plain = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Table)
            .Add(x => x.TableColumns, [new DisplayTableColumn("id", "Id")]));
        Assert.DoesNotContain("sticky", plain.Find("[data-slot=display-table-head]").ClassList);
    }

    [Fact]
    public void Diff_note_is_shown_beside_the_label()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Diff)
            .Add(x => x.DiffItems, [new DisplayDiffItem("Plan", "Pro", "Team", Note: new DisplayChip("Needs approval"))]));

        Assert.Contains("Needs approval", cut.Find("[data-slot=diff-item] [data-slot=display-chip]").TextContent);
    }

    [Fact]
    public async Task Record_rows_render_image_meta_icon_badges_caption_tone_and_click()
    {
        var clicks = 0;
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Record)
            .Add(x => x.Records, [
                new DisplayRecordItem
                {
                    Title = "Order 1042",
                    Meta = "",
                    Description = "Shipped",
                    MetaIcon = "clock",
                    ImageSrc = "/img/box.png",
                    Icon = "inbox",
                    Tone = KlyColor.Success,
                    Badges = [new DisplayChip("Gift"), new DisplayChip("Express")],
                    Trailing = "€120",
                    TrailingCaption = "Nov 20",
                    OnClick = Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => clicks++),
                },
            ]));

        Assert.Equal("/img/box.png", cut.Find("[data-slot=record-image]").GetAttribute("src"));
        Assert.Empty(cut.FindAll("[data-slot=record-avatar]"));
        var meta = cut.Find("[data-slot=record-meta]");
        Assert.Equal("Shipped", meta.TextContent.Trim());
        Assert.NotNull(meta.QuerySelector("svg"));
        Assert.Equal(2, cut.FindAll("[data-slot=display-chip]").Count);
        Assert.Equal("Nov 20", cut.Find("[data-slot=record-trailing-caption]").TextContent);
        Assert.Equal("success", cut.Find("[data-slot=record-item]").GetAttribute("data-color"));

        var title = cut.Find("[data-slot=record-title]");
        Assert.Equal("BUTTON", title.TagName);
        await title.ClickAsync();
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void Person_shows_avatar_image_tone_and_contact_links()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Person)
            .Add(x => x.People, [new DisplayPersonItem("Ada Lovelace", "Engineer", AvatarSrc: "/img/ada.png")])
            .Add(x => x.Contacts, [new DisplayContactItem("ada@example.com", Icon: "mail", Href: "mailto:ada@example.com")]));

        Assert.Equal("/img/ada.png", cut.Find("[data-slot=person-image]").GetAttribute("src"));
        var contact = cut.Find("[data-slot=person-contact]");
        Assert.Equal("mailto:ada@example.com", contact.QuerySelector("a")!.GetAttribute("href"));
        Assert.NotNull(contact.QuerySelector("svg"));

        var toned = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Person)
            .Add(x => x.People, [new DisplayPersonItem("Grace Hopper", Tone: KlyColor.Info)]));
        Assert.Equal("info", toned.Find("[data-slot=person-avatar]").GetAttribute("data-color"));
        Assert.Equal("GH", toned.Find("[data-slot=person-avatar]").TextContent.Trim());
    }

    [Fact]
    public void Person_group_puts_description_and_chip_in_the_tooltip()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Person)
            .Add(x => x.Variant, DisplayVariant.Group)
            .Add(x => x.People, [new DisplayPersonItem("Ada Lovelace", "Engineer", Chip: new DisplayChip("Owner"))]));

        Assert.Equal("Ada Lovelace · Engineer · Owner", cut.Find("[data-slot=person-avatar]").GetAttribute("title"));
    }

    [Fact]
    public void Person_without_people_or_title_shows_no_name()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Person)
            .Add(x => x.Description, "Unassigned"));

        Assert.Empty(cut.FindAll("[data-slot=person-name]"));
        Assert.DoesNotContain("User", cut.Markup);
        Assert.NotNull(cut.Find("[data-slot=person-avatar] svg"));
        Assert.Equal("Unassigned", cut.Find("[data-slot=person-description]").TextContent);
    }

    [Fact]
    public void List_items_show_their_icon_and_tone()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.List)
            .Add(x => x.ListItems, [
                new DisplayListItem { Text = "Backups", Icon = "check", Tone = KlyColor.Success },
                new DisplayListItem { Text = "Alerts", Tone = KlyColor.Warning },
            ]));

        var icon = cut.Find("[data-slot=list-icon]");
        Assert.Equal("success", icon.GetAttribute("data-color"));
        Assert.NotNull(icon.QuerySelector("svg"));
        Assert.Equal("warning", cut.Find("[data-slot=list-bullet]").GetAttribute("data-color"));
    }

    [Fact]
    public void Timeline_uses_date_icon_and_tone_and_the_log_variant_shows_timestamps()
    {
        var date = new DateTimeOffset(2026, 11, 20, 14, 30, 0, TimeSpan.Zero);
        var culture = CultureInfo.GetCultureInfo("en-US");
        var (previous, previousUi) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            AssertTimeline(date, culture);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private void AssertTimeline(DateTimeOffset date, CultureInfo culture)
    {
        IReadOnlyList<DisplayTimelineItem> items =
        [
            new DisplayTimelineItem("Deployed", Date: date, Icon: "zap", Tone: KlyColor.Success),
            new DisplayTimelineItem("Rolled back", Time: "15:02", Date: date),
        ];

        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Timeline)
            .Add(x => x.TimelineItems, items));
        var time = cut.Find("[data-slot=timeline-time]");
        Assert.Equal(date.ToString("O", CultureInfo.InvariantCulture), time.GetAttribute("datetime"));
        Assert.Equal(date.ToString("g", culture), time.TextContent);
        var marker = cut.Find("[data-slot=timeline-marker]");
        Assert.Equal("success", marker.GetAttribute("data-color"));
        Assert.NotNull(marker.QuerySelector("svg"));
        Assert.Equal("15:02", cut.FindAll("[data-slot=timeline-time]")[1].TextContent);

        var log = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Timeline)
            .Add(x => x.Variant, DisplayVariant.Log)
            .Add(x => x.TimelineItems, items));
        Assert.NotNull(log.Find("[data-slot=timeline-log]"));
        var stamps = log.FindAll("[data-slot=timeline-timestamp]");
        Assert.Equal($"{date.ToString("d", culture)} {date.ToString("t", culture)}", stamps[0].TextContent);
        Assert.Equal("11/20/2026 15:02", stamps[1].TextContent);
    }

    [Fact]
    public void Action_target_reaches_the_link()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Heading)
            .Add(x => x.Title, "Docs")
            .Add(x => x.Action, new DisplayAction { Label = "Open", Href = "https://example.com", Target = "_blank" }));

        Assert.Equal("_blank", cut.Find("a[data-slot=display-action]").GetAttribute("target"));
    }

    [Fact]
    public void Alert_falls_back_to_the_description_when_the_value_is_empty()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Alert)
            .Add(x => x.Value, "")
            .Add(x => x.Description, "Check your billing details"));

        Assert.Equal("Check your billing details", cut.Find("[data-slot=alert-message]").TextContent);
    }

    [Theory]
    [InlineData(DisplayVariant.Countdown)]
    [InlineData(DisplayVariant.Range)]
    public void Countdown_and_range_dates_show_the_title_as_a_caption(DisplayVariant variant)
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Date)
            .Add(x => x.Variant, variant)
            .Add(x => x.Title, "Renewal")
            .Add(x => x.DateValue, new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero))
            .Add(x => x.EndDateValue, new DateTimeOffset(2026, 12, 7, 0, 0, 0, TimeSpan.Zero))
            .Add(x => x.Now, new DateTimeOffset(2026, 11, 26, 0, 0, 0, TimeSpan.Zero)));

        Assert.Equal("Renewal", cut.Find("[data-slot=date-caption]").TextContent);
        Assert.DoesNotContain("Renewal", cut.Find("[data-slot=date-title]").TextContent);
    }

    [Fact]
    public void MaxLines_sets_the_number_of_clamped_lines()
    {
        var cut = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Text)
            .Add(x => x.Value, "A long text")
            .Add(x => x.MaxLines, 4));

        Assert.Equal("-webkit-line-clamp:4", cut.Find("[data-slot=text-body]").GetAttribute("style"));
    }

    [Fact]
    public void Display_texts_come_from_DisplayTexts()
    {
        Services.AddKlyrooDisplay(d => d.Texts("en", t => t with
        {
            ShowMore = n => $"Afficher {n} de plus",
            DayCount = n => $"{n} jours",
        }));

        var list = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.List)
            .Add(x => x.Max, 1)
            .Add(x => x.ListItems, [new DisplayListItem { Text = "a" }, new DisplayListItem { Text = "b" }]));
        Assert.Equal("Afficher 1 de plus", list.Find("[data-slot=display-more]").TextContent.Trim());

        var range = Render<KlyDisplay>(p => p
            .Add(x => x.Kind, DisplayKind.Date)
            .Add(x => x.Variant, DisplayVariant.Range)
            .Add(x => x.DateValue, new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero))
            .Add(x => x.EndDateValue, new DateTimeOffset(2026, 12, 7, 0, 0, 0, TimeSpan.Zero)));
        Assert.Contains("(7 jours)", range.Find("[data-slot=date-title]").TextContent);
    }
}
