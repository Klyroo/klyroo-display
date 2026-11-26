using System.Globalization;
using Bunit;
using Klyroo.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Klyroo.Display.Tests;

// The schedule variant of the Timeline: events by day, a time column, a length, all-day events and the unscheduled group.
public sealed class TimelineScheduleTests : BunitContext
{
    private static readonly TimeSpan Paris = TimeSpan.FromHours(1);

    private static DateTimeOffset At(int day, int hour = 0, int minute = 0, TimeSpan? offset = null) =>
        new(2026, 3, day, hour, minute, 0, offset ?? Paris);

    private static readonly DateTimeOffset Reference = At(12, 8);

    public TimelineScheduleTests()
    {
        Services.AddKlyrooDisplay();
        JSInterop.SetupModule("./_content/Klyroo.Display/klyroo-display-media.js").Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<KlyDisplay> Schedule(IReadOnlyList<DisplayTimelineItem> items, Action<ComponentParameterCollectionBuilder<KlyDisplay>>? configure = null, string variant = "schedule", DateTimeOffset? now = null) =>
        Render<KlyDisplay>(p =>
        {
            p.Add(x => x.Kind, DisplayKind.Timeline).Add(x => x.Variant, variant).Add(x => x.TimelineItems, items).Add(x => x.Now, now ?? Reference);
            configure?.Invoke(p);
        });

    private IRenderedComponent<CascadingValue<KlyLocale>> ScheduleIn(KlyLocale locale, IReadOnlyList<DisplayTimelineItem> items) =>
        Render<CascadingValue<KlyLocale>>(p => p
            .Add(x => x.Value, locale)
            .AddChildContent<KlyDisplay>(d => d
                .Add(x => x.Kind, DisplayKind.Timeline)
                .Add(x => x.Variant, "schedule")
                .Add(x => x.Now, Reference)
                .Add(x => x.TimelineItems, items)));

    private static string[] Titles(IRenderedComponent<KlyDisplay> cut) =>
        cut.FindAll("[data-slot=schedule-event] [data-slot=timeline-title]").Select(t => t.TextContent).ToArray();

    private static string Clean(string text) => text.Replace(' ', ' ').Replace(' ', ' ').Trim();

    [Fact]
    public void Events_are_sorted_by_start_and_grouped_by_day()
    {
        var cut = Schedule(
        [
            new("Lunch", Date: At(13, 12)),
            new("Standup", Date: At(12, 9)),
            new("Review", Date: At(12, 15)),
            new("Retro", Date: At(13, 9)),
        ]);

        var days = cut.FindAll("[data-slot=schedule-day]");
        Assert.Equal(2, days.Count);
        Assert.Equal("2026-03-12", days[0].GetAttribute("data-date"));
        Assert.Equal("2026-03-13", days[1].GetAttribute("data-date"));
        Assert.Equal(["Standup", "Review"], days[0].QuerySelectorAll("[data-slot=timeline-title]").Select(t => t.TextContent));
        Assert.Equal(["Retro", "Lunch"], days[1].QuerySelectorAll("[data-slot=timeline-title]").Select(t => t.TextContent));
    }

    [Fact]
    public void The_variant_name_ignores_case_and_other_variants_are_unchanged()
    {
        var upper = Schedule([new("A", Date: At(12, 9))], variant: "SCHEDULE");
        Assert.Single(upper.FindAll("[data-slot=schedule-event]"));

        var pipeline = Schedule([new("A", Date: At(12, 9))], variant: "pipeline");
        Assert.Empty(pipeline.FindAll("[data-slot=schedule-event]"));
        Assert.Single(pipeline.FindAll("[data-slot=timeline-item]"));
    }

    [Fact]
    public void The_markup_is_a_list_per_day_under_a_dated_heading()
    {
        var cut = Schedule([new("Standup", Date: At(12, 9), EndDate: At(12, 10, 30))]);

        var day = cut.Find("[data-slot=schedule-day]");
        Assert.Equal("H4", day.QuerySelector("[data-slot=schedule-day-title]")!.TagName);
        Assert.Equal("2026-03-12", day.QuerySelector("[data-slot=schedule-day-title] time")!.GetAttribute("datetime"));
        var list = day.QuerySelector("ol")!;
        var item = Assert.Single(list.Children);
        Assert.Equal("LI", item.TagName);
        Assert.Equal(At(12, 9).ToString("O", CultureInfo.InvariantCulture), item.QuerySelector("[data-slot=schedule-time]")!.GetAttribute("datetime"));
        Assert.Equal("PT1H30M", item.QuerySelector("[data-slot=schedule-duration]")!.GetAttribute("datetime"));
    }

    [Fact]
    public void A_time_range_and_a_length_are_shown_in_the_culture_of_the_locale()
    {
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        var cut = Schedule([new("Standup", Date: At(12, 9), EndDate: At(12, 10, 30))]);

        var expected = $"{At(12, 9).ToString("t", culture)} – {At(12, 10, 30).ToString("t", culture)}";
        Assert.Equal(Clean(expected), Clean(cut.Find("[data-slot=schedule-time]").TextContent));
        Assert.Equal("1\u00a0h\u00a030\u00a0min", cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Theory]
    [InlineData(45, "45\u00a0min")]
    [InlineData(60, "1\u00a0h")]
    [InlineData(120, "2\u00a0h")]
    [InlineData(150, "2\u00a0h\u00a030\u00a0min")]
    public void The_length_is_in_hours_and_minutes(int minutes, string expected)
    {
        var cut = Schedule([new("Call", Date: At(12, 9), EndDate: At(12, 9).AddMinutes(minutes))]);

        Assert.Equal(expected, cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Fact]
    public void An_event_without_an_end_shows_only_its_start()
    {
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        var cut = Schedule([new("Standup", Date: At(12, 9, 15))]);

        Assert.Equal(Clean(At(12, 9, 15).ToString("t", culture)), Clean(cut.Find("[data-slot=schedule-time]").TextContent));
        Assert.Empty(cut.FindAll("[data-slot=schedule-duration]"));
        Assert.DoesNotContain(" – ", cut.Find("[data-slot=schedule-time-column]").TextContent);
    }

    [Fact]
    public void An_end_that_is_not_after_the_start_is_ignored()
    {
        var cut = Schedule(
        [
            new("Same", Date: At(12, 9), EndDate: At(12, 9)),
            new("Before", Date: At(12, 10), EndDate: At(12, 8)),
        ]);

        Assert.Empty(cut.FindAll("[data-slot=schedule-duration]"));
        Assert.All(cut.FindAll("[data-slot=schedule-time]"), t => Assert.DoesNotContain(" – ", t.TextContent));
    }

    [Fact]
    public void An_event_at_midnight_without_an_end_is_all_day_and_comes_first()
    {
        var cut = Schedule(
        [
            new("Standup", Date: At(12, 9)),
            new("Holiday", Date: At(12)),
        ]);

        Assert.Equal(["Holiday", "Standup"], Titles(cut));
        var holiday = cut.FindAll("[data-slot=schedule-event]")[0];
        Assert.Equal("All day", holiday.QuerySelector("[data-slot=schedule-time]")!.TextContent);
        Assert.Equal("2026-03-12", holiday.QuerySelector("[data-slot=schedule-time]")!.GetAttribute("datetime"));
        Assert.Null(holiday.QuerySelector("[data-slot=schedule-duration]"));
    }

    [Fact]
    public void A_multi_day_all_day_event_is_listed_once_with_its_number_of_days()
    {
        var cut = Schedule(
        [
            new("Conference", Date: At(12), EndDate: At(15)),
            new("Standup", Date: At(13, 9)),
        ]);

        Assert.Equal(2, cut.FindAll("[data-slot=schedule-day]").Count);
        Assert.Single(cut.FindAll("[data-slot=schedule-event]"), e => e.TextContent.Contains("Conference"));
        var conference = cut.FindAll("[data-slot=schedule-event]")[0];
        Assert.Equal("All day", conference.QuerySelector("[data-slot=schedule-time]")!.TextContent);
        Assert.Equal("3 days", conference.QuerySelector("[data-slot=schedule-duration]")!.TextContent);
        Assert.Equal("P3D", conference.QuerySelector("[data-slot=schedule-duration]")!.GetAttribute("datetime"));
        Assert.Equal("2026-03-12", cut.FindAll("[data-slot=schedule-day]")[0].GetAttribute("data-date"));
    }

    [Fact]
    public void An_end_at_midnight_is_excluded_as_in_calendar_files()
    {
        var cut = Schedule([new("Holiday", Date: At(12), EndDate: At(13))]);

        Assert.Equal("All day", cut.Find("[data-slot=schedule-time]").TextContent);
        // One day: no day count is shown.
        Assert.DoesNotContain(cut.FindAll("[data-slot=schedule-duration]"), d => d.TextContent.Length > 0);
    }

    [Theory]
    [InlineData(3, 28, 1, 3, 30, 2)]
    [InlineData(10, 24, 2, 10, 26, 1)]
    public void An_all_day_event_across_a_daylight_saving_change_stays_all_day(int startMonth, int startDay, int startOffset, int endMonth, int endDay, int endOffset)
    {
        var cut = Schedule([new("Trip", Date: new DateTimeOffset(2026, startMonth, startDay, 0, 0, 0, TimeSpan.FromHours(startOffset)), EndDate: new DateTimeOffset(2026, endMonth, endDay, 0, 0, 0, TimeSpan.FromHours(endOffset)))]);

        Assert.Equal("All day", cut.Find("[data-slot=schedule-time]").TextContent);
        Assert.Equal("2 days", cut.Find("[data-slot=schedule-duration]").TextContent);
        Assert.Equal("P2D", cut.Find("[data-slot=schedule-duration]").GetAttribute("datetime"));
    }

    [Fact]
    public void A_timed_event_across_a_daylight_saving_change_keeps_its_local_end_time()
    {
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        // Paris moves to summer time at 02:00 on March 29: 01:00 (+01:00) to 10:00 (+02:00) is 8 hours.
        var end = new DateTimeOffset(2026, 3, 29, 10, 0, 0, TimeSpan.FromHours(2));
        var cut = Schedule([new("Night", Date: new DateTimeOffset(2026, 3, 29, 1, 0, 0, Paris), EndDate: end)]);

        Assert.EndsWith(Clean(end.ToString("t", culture)), Clean(cut.Find("[data-slot=schedule-time]").TextContent));
        Assert.Equal("8\u00a0h", cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Fact]
    public void An_end_at_the_end_of_a_day_still_makes_an_all_day_event()
    {
        var cut = Schedule([new("Trip", Date: At(12), EndDate: new DateTimeOffset(2026, 3, 13, 23, 59, 59, Paris))]);

        Assert.Equal("All day", cut.Find("[data-slot=schedule-time]").TextContent);
        Assert.Equal("2 days", cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Fact]
    public void A_timed_event_that_ends_on_another_day_stays_on_its_start_day_and_shows_the_end_date()
    {
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        var cut = Schedule([new("Night shift", Date: At(12, 22), EndDate: At(13, 6))]);

        Assert.Single(cut.FindAll("[data-slot=schedule-day]"));
        Assert.Equal("2026-03-12", cut.Find("[data-slot=schedule-day]").GetAttribute("data-date"));
        var expected = $"{At(12, 22).ToString("t", culture)} – {At(13, 6).ToString("d", culture)} {At(13, 6).ToString("t", culture)}";
        Assert.Equal(Clean(expected), Clean(cut.Find("[data-slot=schedule-time]").TextContent));
        Assert.Equal("8\u00a0h", cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Fact]
    public void An_event_is_shown_in_its_own_offset()
    {
        var tokyo = TimeSpan.FromHours(9);
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        var cut = Schedule(
        [
            new("Late", Date: At(12, 23, 30, tokyo), EndDate: At(13, 0, 30, tokyo)),
            new("Paris", Date: At(12, 9)),
        ]);

        // 23:30 at +09:00 stays on the 12th, and its time is the time of its own offset.
        var days = cut.FindAll("[data-slot=schedule-day]");
        Assert.Single(days);
        Assert.StartsWith(Clean(At(12, 23, 30, tokyo).ToString("t", culture)), Clean(cut.FindAll("[data-slot=schedule-time]")[1].TextContent));
        Assert.Contains("+09:00", cut.FindAll("[data-slot=schedule-time]")[1].GetAttribute("datetime"));
    }

    [Fact]
    public void An_end_in_another_offset_is_shown_in_its_own_offset()
    {
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        // 10:00 UTC is 11:00 in Paris: the end reads 10:00, the time its source gave, and the length compares the instants.
        var end = new DateTimeOffset(2026, 3, 12, 10, 0, 0, TimeSpan.Zero);
        var cut = Schedule([new("Call", Date: At(12, 9), EndDate: end)]);

        Assert.EndsWith(Clean(end.ToString("t", culture)), Clean(cut.Find("[data-slot=schedule-time]").TextContent));
        Assert.Equal("2\u00a0h", cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Fact]
    public void Events_without_a_date_come_last_under_an_unscheduled_heading()
    {
        var cut = Schedule(
        [
            new("Someday", Description: "no date"),
            new("Standup", Date: At(12, 9)),
            new("Later", Time: "TBD"),
        ]);

        Assert.Equal(["Standup", "Someday", "Later"], Titles(cut));
        var days = cut.FindAll("[data-slot=schedule-day]");
        Assert.Equal(2, days.Count);
        Assert.Equal("Unscheduled", days[1].QuerySelector("[data-slot=schedule-unscheduled-title]")!.TextContent);
        Assert.Null(days[1].GetAttribute("data-date"));
        var untimed = Assert.Single(days[1].QuerySelectorAll("[data-slot=schedule-time]"));
        Assert.Equal("TBD", untimed.TextContent);
        Assert.Null(untimed.GetAttribute("datetime"));
    }

    [Fact]
    public void A_schedule_without_any_date_has_no_heading()
    {
        var cut = Schedule([new("A"), new("B")]);

        Assert.Equal(["A", "B"], Titles(cut));
        Assert.Empty(cut.FindAll("[data-slot=schedule-unscheduled-title]"));
        Assert.Empty(cut.FindAll("[data-slot=schedule-day-title]"));
    }

    [Fact]
    public void The_time_text_takes_precedence_over_the_computed_range()
    {
        var cut = Schedule([new("Standup", Time: "Early morning", Date: At(12, 9), EndDate: At(12, 10))]);

        Assert.Equal("Early morning", cut.Find("[data-slot=schedule-time]").TextContent);
        Assert.Equal("1\u00a0h", cut.Find("[data-slot=schedule-duration]").TextContent);
    }

    [Fact]
    public void The_day_title_says_today_and_tomorrow()
    {
        var culture = KlyDates.FormatCulture(KlyLocales.En);
        var cut = Schedule(
        [
            new("Today event", Date: At(12, 9)),
            new("Tomorrow event", Date: At(13, 9)),
            new("Later event", Date: At(20, 9)),
            new("Past event", Date: At(10, 9)),
        ]);

        var titles = cut.FindAll("[data-slot=schedule-day-title]").Select(t => Clean(t.TextContent)).ToArray();
        Assert.Equal(
        [
            Clean(At(10, 9).ToString("D", culture)),
            Clean($"Today · {At(12, 9).ToString("D", culture)}"),
            Clean($"Tomorrow · {At(13, 9).ToString("D", culture)}"),
            Clean(At(20, 9).ToString("D", culture)),
        ], titles);
    }

    [Fact]
    public void The_current_event_is_emphasized_and_marked()
    {
        var cut = Schedule(
        [
            new("Past", Date: At(12, 8), State: DisplayTimelineState.Done),
            new("Now", Date: At(12, 9), State: DisplayTimelineState.Current),
            new("Next", Date: At(12, 10), State: DisplayTimelineState.Upcoming),
            new("Plain", Date: At(12, 11)),
        ]);

        var events = cut.FindAll("[data-slot=schedule-event]");
        Assert.Equal([null, "step", null, null], events.Select(e => e.GetAttribute("aria-current")));
        Assert.Equal(["done", "current", "upcoming", null], events.Select(e => e.GetAttribute("data-state")));
        Assert.Contains("font-semibold", events[1].QuerySelector("[data-slot=timeline-title]")!.ClassList);
        Assert.Contains("bg-(--klyroo-bg-muted)", events[1].ClassList);
        Assert.Contains("text-(--klyroo-text-muted)", events[0].QuerySelector("[data-slot=timeline-title]")!.ClassList);
        // An event without a state is not muted.
        Assert.Contains("text-(--klyroo-text-highlighted)", events[3].QuerySelector("[data-slot=timeline-title]")!.ClassList);
    }

    [Fact]
    public void Title_description_actor_icon_and_tone_are_shown()
    {
        var cut = Schedule([new("Design review", "Bring the mockups", Date: At(12, 9), Icon: "calendar", ActorName: "Alice", Tone: KlyColor.Success)]);

        var e = cut.Find("[data-slot=schedule-event]");
        Assert.Equal("Design review", e.QuerySelector("[data-slot=timeline-title]")!.TextContent);
        Assert.Equal("Bring the mockups", e.QuerySelector("[data-slot=schedule-description]")!.TextContent);
        Assert.Equal("Alice", e.QuerySelector("[data-slot=schedule-actor]")!.TextContent);
        Assert.NotNull(e.QuerySelector("[data-slot=schedule-icon] svg"));
        Assert.Equal("true", e.QuerySelector("[data-slot=schedule-icon]")!.GetAttribute("aria-hidden"));
        Assert.Equal(KlyColor.Success.Value, e.GetAttribute("data-color"));
    }

    [Fact]
    public void Max_reveals_later_events_behind_a_show_more_button()
    {
        var cut = Schedule(
        [
            new("E4", Date: At(14, 9)),
            new("E1", Date: At(12, 9)),
            new("E3", Date: At(13, 9)),
            new("E2", Date: At(12, 15)),
        ], p => p.Add(x => x.Max, 2).Add(x => x.Step, 1));

        Assert.Equal(["E1", "E2"], Titles(cut));
        Assert.Equal("Show more · 1", cut.Find("[data-slot=display-more]").TextContent.Trim());

        cut.Find("[data-slot=display-more]").Click();
        Assert.Equal(["E1", "E2", "E3"], Titles(cut));

        cut.Find("[data-slot=display-more]").Click();
        Assert.Equal(["E1", "E2", "E3", "E4"], Titles(cut));
        Assert.Empty(cut.FindAll("[data-slot=display-more]"));
    }

    [Fact]
    public void An_empty_schedule_renders_an_empty_block()
    {
        var cut = Schedule([]);

        Assert.Empty(cut.FindAll("[data-slot=schedule-day]"));
        Assert.NotNull(cut.Find("[data-slot=display-timeline]"));
    }

    [Fact]
    public void French_texts_and_dates_are_used_in_the_french_locale()
    {
        var cut = ScheduleIn(KlyLocales.Fr,
        [
            new("Réunion", Date: At(12, 9), EndDate: At(12, 10, 30)),
            new("Férié", Date: At(13)),
            new("Séminaire", Date: At(14), EndDate: At(16)),
            new("À planifier"),
        ]);

        var days = cut.FindAll("[data-slot=schedule-day-title]").Select(t => Clean(t.TextContent)).ToArray();
        Assert.Equal("Aujourd’hui · jeudi 12 mars 2026", days[0]);
        Assert.Equal("Demain · vendredi 13 mars 2026", days[1]);
        Assert.Equal("samedi 14 mars 2026", days[2]);
        Assert.Equal("Non planifiés", Clean(cut.Find("[data-slot=schedule-unscheduled-title]").TextContent));

        var times = cut.FindAll("[data-slot=schedule-time]").Select(t => Clean(t.TextContent)).ToArray();
        Assert.Equal("09:00 – 10:30", times[0]);
        Assert.Equal("Toute la journée", times[1]);
        Assert.Equal("Toute la journée", times[2]);
        var durations = cut.FindAll("[data-slot=schedule-duration]").Select(t => Clean(t.TextContent)).ToArray();
        Assert.Equal(["1 h 30 min", "2 jours"], durations);
    }

    [Theory]
    [InlineData("de", "Ganztägig", "Nicht geplant", "1\u00a0Std.\u00a030\u00a0Min.")]
    [InlineData("ja", "終日", "未定", "1時間30分")]
    [InlineData("ru", "Весь день", "Без даты", "1\u00a0ч\u00a030\u00a0мин")]
    public void Every_locale_has_its_schedule_texts(string code, string allDay, string unscheduled, string duration)
    {
        var texts = DisplayLocales.Tables[code];

        Assert.Equal(allDay, texts.AllDay);
        Assert.Equal(unscheduled, texts.Unscheduled);
        Assert.Equal(duration, texts.Duration(90));
        Assert.False(string.IsNullOrWhiteSpace(texts.Duration(0)));
        Assert.Equal(texts.Duration(0), texts.Duration(-5));
    }

    // The data of the agenda recipe in docs/guides.md.
    [Fact]
    public void The_agenda_recipe_renders_as_the_guide_describes()
    {
        var paris = TimeSpan.FromHours(1);
        IReadOnlyList<DisplayTimelineItem> events =
        [
            new("Standup", Date: new(2026, 3, 12, 9, 0, 0, paris), EndDate: new(2026, 3, 12, 9, 30, 0, paris), State: DisplayTimelineState.Current),
            new("Design review", "Bring the mockups", Date: new(2026, 3, 12, 14, 0, 0, paris), EndDate: new(2026, 3, 12, 15, 30, 0, paris), ActorName: "Alice"),
            new("Conference", Date: new(2026, 3, 14, 0, 0, 0, paris), EndDate: new(2026, 3, 16, 0, 0, 0, paris)),
            new("Book the venue"),
        ];

        var cut = Schedule(events, now: new DateTimeOffset(2026, 3, 12, 9, 30, 0, paris));

        Assert.StartsWith("Today · ", Clean(cut.Find("[data-slot=schedule-day-title]").TextContent));
        Assert.Equal(["30\u00a0min", "1\u00a0h\u00a030\u00a0min", "2 days"], cut.FindAll("[data-slot=schedule-duration]").Select(d => d.TextContent));
        Assert.Equal("All day", cut.FindAll("[data-slot=schedule-time]")[2].TextContent);
        Assert.Equal("Unscheduled", cut.Find("[data-slot=schedule-unscheduled-title]").TextContent);
        Assert.Equal(["Standup", "Design review", "Conference", "Book the venue"], Titles(cut));
    }

    [Fact]
    public void The_timeline_item_keeps_its_positional_parameters_and_adds_the_end_last()
    {
        var item = new DisplayTimelineItem("T", "D", "9", At(1), "i", "a", DisplayTimelineState.Done, KlyColor.Error);
        Assert.Null(item.EndDate);

        var withEnd = item with { EndDate = At(2) };
        Assert.Equal(At(2), withEnd.EndDate);
        Assert.Equal(KlyColor.Error, withEnd.Tone);
    }
}
