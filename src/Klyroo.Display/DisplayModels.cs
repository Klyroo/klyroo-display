using Microsoft.AspNetCore.Components;

namespace Klyroo.Display;

/// <summary>
/// The kind of block rendered by <see cref="KlyDisplay"/>. Each kind reads its own set of parameters and a few of them
/// accept a <see cref="KlyDisplay.Variant"/>. The summaries list what each kind renders.
/// </summary>
public enum DisplayKind
{
    /// <summary>A section heading with an optional icon, count pill, description and action, rendered as the heading element chosen by <see cref="KlyDisplay.HeadingLevel"/>. The <c>eyebrow</c> variant renders small uppercase text instead of a heading element.</summary>
    Heading,

    /// <summary>One or more paragraphs, or a single value. The <c>quote</c> variant adds a start border and a footer with the author and meta text. <see cref="KlyDisplay.MaxLines"/> clamps the value and adds a Read more toggle.</summary>
    Text,

    /// <summary>A message with an icon chosen from <see cref="KlyDisplay.Tone"/>, an optional collapsible details text and action buttons.</summary>
    Alert,

    /// <summary>A list of label and value rows, flat from <see cref="KlyDisplay.Facts"/> or grouped from <see cref="KlyDisplay.FactGroups"/>. Rows can show a chip, a link, a copy button and a loading placeholder.</summary>
    Facts,

    /// <summary>A highlighted value with a title, unit, delta badge, sparkline and caption. The <c>row</c> variant shows the title above several small metrics side by side from <see cref="KlyDisplay.StatItems"/>, and ignores the value, unit, delta, trend and caption.</summary>
    Stat,

    /// <summary>A list of labelled amounts with subtotal rows and a total row. The <c>collapsible</c> variant hides the lines behind a toggle. With <see cref="KlyDisplay.CompareColumns"/>, or when a line has a <see cref="DisplayBreakdownLine.CompareValue"/>, the amounts are shown in two columns.</summary>
    Breakdown,

    /// <summary>A horizontal meter driven by <see cref="KlyDisplay.MeterValue"/> and <see cref="KlyDisplay.MeterMax"/>, with warning and error thresholds, or stacked parts with an optional legend. The <c>ring</c> variant draws the same value or parts as a ring with the value in its center, and the <c>scale</c> variant shows discrete levels instead.</summary>
    Meter,

    /// <summary>A small bar chart from <see cref="KlyDisplay.ChartData"/>. The <c>line</c> variant draws a line through the points, and the <c>funnel</c> variant draws one bar per step with the conversion rate from the previous step.</summary>
    Chart,

    /// <summary>A plain table of text cells, defined by columns and rows keyed by column, with an optional footer row. The <c>sticky</c> variant scrolls the rows in a box at most 20rem high and keeps the header row visible.</summary>
    Table,

    /// <summary>Before and after values per field, with unchanged fields hidden unless <see cref="KlyDisplay.ShowUnchanged"/> is set.</summary>
    Diff,

    /// <summary>A list of entity rows, each with an image, avatar or icon, a reference, a title, a secondary line, badges and a trailing value with a caption.</summary>
    Record,

    /// <summary>A person with an avatar (image or initials), name, description and chip and optional contacts, or with the <c>group</c> variant a stack of avatars with a +N overflow, whose tooltips give each name, description and chip.</summary>
    Person,

    /// <summary>A list of items. The <c>steps</c> and <c>checks</c> variants number the items and mark done ones, and the <c>files</c> variant shows an extension badge and file size and can open the items in the file preview.</summary>
    List,

    /// <summary>A numbered list of events with a done, current or upcoming state. The <c>pipeline</c> variant lays the events out in a row, and the <c>log</c> variant shows compact lines that start with the date and time of each event.</summary>
    Timeline,

    /// <summary>A calendar tile with a title. The <c>countdown</c> variant shows the days until or since the date, and the <c>range</c> variant shows a start and end date with the number of days; both show the title as a caption below.</summary>
    Date,

    /// <summary>A single image, or with the <c>gallery</c> variant a grid of images with a +N overflow tile.</summary>
    Media,

    /// <summary>A preformatted value with a label and a copy button. The <c>secret</c> variant masks the value and adds a Reveal toggle.</summary>
    Code,

    /// <summary>An empty state with an icon, a title, a description and action buttons.</summary>
    Empty,

    /// <summary>Collapsible sections, one open at a time unless <see cref="KlyDisplay.Multiple"/> is set.</summary>
    Accordion,
}

/// <summary>The surface treatment of a <see cref="KlyDisplay"/> block: border, background and padding.</summary>
public enum DisplaySurface
{
    /// <summary>No border, background or padding.</summary>
    Plain,

    /// <summary>A muted background with padding.</summary>
    Muted,

    /// <summary>A border and padding.</summary>
    Bordered,

    /// <summary>A border, an elevated background, padding and a shadow.</summary>
    Elevated,
}

/// <summary>A small badge shown in a block.</summary>
/// <param name="Label">The badge text.</param>
/// <param name="Color">The badge color, or <see langword="null"/> for neutral.</param>
/// <param name="Icon">An icon name shown before the label.</param>
public sealed record DisplayChip(
    string Label,
    KlyColor? Color = null,
    string? Icon = null);

/// <summary>A button or link shown in a block, rendered as a small <see cref="KlyButton"/>.</summary>
public sealed class DisplayAction
{
    /// <summary>Creates an action. Set <see cref="Label"/> before use.</summary>
    public DisplayAction()
    {
    }

    /// <summary>The button text.</summary>
    public required string Label { get; init; }

    /// <summary>An icon name shown before the label.</summary>
    public string? Icon { get; init; }

    /// <summary>The link target. When set, the button navigates instead of only raising <see cref="OnClick"/>.</summary>
    public string? Href { get; init; }

    /// <summary>The <c>target</c> of the link, such as <c>"_blank"</c>. It applies only with <see cref="Href"/>.</summary>
    public string? Target { get; init; }

    /// <summary>The callback invoked when the button is clicked. It only runs once the page is interactive.</summary>
    public EventCallback OnClick { get; init; }

    /// <summary>The button color (neutral by default).</summary>
    public KlyColor Color { get; init; } = KlyColor.Neutral;

    /// <summary>The button style (outline by default).</summary>
    public ButtonVariant Variant { get; init; } = ButtonVariant.Outline;
}

/// <summary>One label and value row of a <see cref="DisplayKind.Facts"/> block.</summary>
public sealed class DisplayFact
{
    /// <summary>Creates a row. Set <see cref="Label"/> before use.</summary>
    public DisplayFact()
    {
    }

    /// <summary>The row label.</summary>
    public required string Label { get; init; }

    /// <summary>The row value. An empty value with no <see cref="Chip"/> renders a dash.</summary>
    public string? Value { get; init; }

    /// <summary>A short line of help shown under the label.</summary>
    public string? Hint { get; init; }

    /// <summary>A badge shown instead of the value.</summary>
    public DisplayChip? Chip { get; init; }

    /// <summary>A color for the value, such as <see cref="KlyColor.Success"/>. It applies to plain values; a link keeps the link color and a chip its own.</summary>
    public KlyColor? Tone { get; init; }

    /// <summary>Shows a copy button that copies <see cref="CopyText"/>, or <see cref="Value"/> when that is not set.</summary>
    public bool Copy { get; init; }

    /// <summary>The text copied by the copy button. Setting it also shows the button.</summary>
    public string? CopyText { get; init; }

    /// <summary>Renders the value in a monospace font.</summary>
    public bool Mono { get; init; }

    /// <summary>Renders the value in bold.</summary>
    public bool Emphasis { get; init; }

    /// <summary>An icon name shown before the label.</summary>
    public string? Icon { get; init; }

    /// <summary>Renders the value as a link to this URL.</summary>
    public string? Href { get; init; }

    /// <summary>Shows a placeholder instead of the value.</summary>
    public bool Loading { get; init; }
}

/// <summary>A titled group of rows in a <see cref="DisplayKind.Facts"/> block.</summary>
/// <param name="Title">The group title.</param>
/// <param name="Items">The rows of the group.</param>
/// <param name="Aside">Text shown opposite the title.</param>
public sealed record DisplayFactGroup(
    string? Title,
    IReadOnlyList<DisplayFact> Items,
    string? Aside = null);

/// <summary>One metric of the <c>row</c> variant of a <see cref="DisplayKind.Stat"/> block.</summary>
/// <param name="Label">The metric label.</param>
/// <param name="Value">The metric value.</param>
/// <param name="Caption">A line of text below the value.</param>
/// <param name="Tone">A color for the value, such as <see cref="KlyColor.Error"/> for a metric that went wrong.</param>
public sealed record DisplayStatItem(
    string Label,
    string Value,
    string? Caption = null,
    KlyColor? Tone = null);

/// <summary>One line of a <see cref="DisplayKind.Breakdown"/> block, also used for its total.</summary>
/// <param name="Label">The line label.</param>
/// <param name="Value">The amount, already formatted. A missing value renders a dash.</param>
/// <param name="CompareValue">A second amount, such as the previous period, shown in a second column. Setting it on any line, or setting <see cref="KlyDisplay.CompareColumns"/>, switches the block to two columns, with a dash where a line has no second amount.</param>
/// <param name="Detail">A line of smaller text under the label.</param>
/// <param name="IsSubtotal">Renders the line as a subtotal, with a top border and bold text.</param>
/// <param name="IsMuted">Renders the line in a dimmed color.</param>
/// <param name="Tone">A color for the amount, such as <see cref="KlyColor.Success"/> for a discount.</param>
/// <param name="Chip">A badge shown after the label.</param>
public sealed record DisplayBreakdownLine(
    string Label,
    string? Value = null,
    string? CompareValue = null,
    string? Detail = null,
    bool IsSubtotal = false,
    bool IsMuted = false,
    KlyColor? Tone = null,
    DisplayChip? Chip = null);

/// <summary>One segment of a <see cref="DisplayKind.Meter"/> block.</summary>
/// <param name="Label">The segment label, shown as a tooltip and in the legend.</param>
/// <param name="Value">The segment size, in the same unit as <see cref="KlyDisplay.MeterMax"/>.</param>
/// <param name="ValueLabel">The value as displayed in the tooltip and the legend, such as <c>"3.2 GB"</c>. When <see langword="null"/>, the number is shown.</param>
/// <param name="Tone">The segment color (primary when <see langword="null"/>).</param>
/// <param name="Icon">An icon name shown in the legend instead of the color dot.</param>
/// <param name="Max">A maximum for this segment: the segment never grows past it, and the legend shows the value out of it.</param>
public sealed record DisplayMeterPart(
    string Label,
    double Value,
    string? ValueLabel = null,
    KlyColor? Tone = null,
    string? Icon = null,
    double? Max = null);

/// <summary>One bar or step of a <see cref="DisplayKind.Chart"/> block.</summary>
/// <param name="Label">The label under the bar.</param>
/// <param name="Value">The value. Bar heights are relative to the largest value.</param>
/// <param name="Tone">The color of the bar, step or point (primary when <see langword="null"/>), for example to highlight one value.</param>
public sealed record DisplayChartPoint(
    string Label,
    double Value,
    KlyColor? Tone = null);

/// <summary>One column of a <see cref="DisplayKind.Table"/> block.</summary>
/// <param name="Key">The key that selects this column's cell in each row.</param>
/// <param name="Label">The header text.</param>
/// <param name="Numeric">Aligns the column to the end and uses tabular figures.</param>
/// <param name="Align">Set to <c>"end"</c> to align the column to the end.</param>
/// <param name="Mono">Renders the cells in a monospace font.</param>
public sealed record DisplayTableColumn(
    string Key,
    string Label,
    bool Numeric = false,
    string? Align = null,
    bool Mono = false);

/// <summary>One field of a <see cref="DisplayKind.Diff"/> block.</summary>
/// <param name="Label">The field label.</param>
/// <param name="Before">The previous value, shown struck through.</param>
/// <param name="After">The new value.</param>
/// <param name="Changed">Whether the field changed. Unchanged fields are hidden unless <see cref="KlyDisplay.ShowUnchanged"/> is set.</param>
/// <param name="Note">A badge shown after the label, such as <c>"Requires approval"</c>.</param>
public sealed record DisplayDiffItem(
    string Label,
    string? Before,
    string? After,
    bool Changed = true,
    DisplayChip? Note = null);

/// <summary>One row of a <see cref="DisplayKind.Record"/> block.</summary>
public sealed class DisplayRecordItem
{
    /// <summary>Creates a row. Set <see cref="Title"/> before use.</summary>
    public DisplayRecordItem()
    {
    }

    /// <summary>A reference such as an order number, shown above the title in a monospace font.</summary>
    public string? Reference { get; init; }

    /// <summary>The row title. It is a link when <see cref="Href"/> is set.</summary>
    public required string Title { get; init; }

    /// <summary>The secondary line. It takes precedence over <see cref="Description"/> unless it is empty.</summary>
    public string? Meta { get; init; }

    /// <summary>An icon shown before the secondary line (<see cref="Meta"/> or <see cref="Description"/>).</summary>
    public string? MetaIcon { get; init; }

    /// <summary>The secondary line when <see cref="Meta"/> is not set.</summary>
    public string? Description { get; init; }

    /// <summary>An icon name shown in the leading tile. It takes precedence over <see cref="AvatarText"/>, and <see cref="ImageSrc"/> takes precedence over it.</summary>
    public string? Icon { get; init; }

    /// <summary>Short text, such as initials, shown in the leading tile.</summary>
    public string? AvatarText { get; init; }

    /// <summary>The URL of an image shown in the leading tile instead of the icon or avatar text. It is decorative (empty <c>alt</c>), since the title names the row.</summary>
    public string? ImageSrc { get; init; }

    /// <summary>A color for the leading tile and the trailing value.</summary>
    public KlyColor? Tone { get; init; }

    /// <summary>Badges shown at the end of the row, before <see cref="Chip"/>.</summary>
    public IReadOnlyList<DisplayChip>? Badges { get; init; }

    /// <summary>A badge shown at the end of the row.</summary>
    public DisplayChip? Chip { get; init; }

    /// <summary>A value shown at the end of the row, such as an amount.</summary>
    public string? Trailing { get; init; }

    /// <summary>A line of smaller text under the trailing value, such as a date.</summary>
    public string? TrailingCaption { get; init; }

    /// <summary>The URL the title links to.</summary>
    public string? Href { get; init; }

    /// <summary>A callback invoked when the title is clicked. Without <see cref="Href"/> the title becomes a button; with it, the link is followed and the callback runs too. It only runs once the page is interactive.</summary>
    public EventCallback OnClick { get; init; }
}

/// <summary>One person of a <see cref="DisplayKind.Person"/> block.</summary>
/// <param name="Name">The person's name. The avatar shows its initials.</param>
/// <param name="Description">A line under the name, such as a role. In the <c>group</c> variant it goes in the avatar tooltip.</param>
/// <param name="AvatarSrc">The URL of an avatar image, shown instead of the initials. It is decorative (empty <c>alt</c>), since the name is shown or in the tooltip.</param>
/// <param name="Tone">A color that tints the initials avatar.</param>
/// <param name="Chip">A badge shown at the end of the row. In the <c>group</c> variant its label goes in the avatar tooltip.</param>
public sealed record DisplayPersonItem(
    string Name,
    string? Description = null,
    string? AvatarSrc = null,
    KlyColor? Tone = null,
    DisplayChip? Chip = null);

/// <summary>One contact line of a <see cref="DisplayKind.Person"/> block.</summary>
/// <param name="Value">The text shown and copied, such as an email address.</param>
/// <param name="Icon">An icon name shown before the value, such as <c>"mail"</c>.</param>
/// <param name="Href">A link for the value, such as a <c>mailto:</c> or <c>tel:</c> URL.</param>
/// <param name="Copy">Whether to show a copy button.</param>
public sealed record DisplayContactItem(
    string Value,
    string? Icon = null,
    string? Href = null,
    bool Copy = true);

/// <summary>One item of a <see cref="DisplayKind.List"/> block.</summary>
public sealed class DisplayListItem
{
    /// <summary>Creates an item. Set <see cref="Text"/> before use.</summary>
    public DisplayListItem()
    {
    }

    /// <summary>The item text. It is a link when <see cref="Href"/> is set.</summary>
    public required string Text { get; init; }

    /// <summary>A line under the text. When <see cref="FileSize"/> is set, the formatted size is shown instead.</summary>
    public string? Description { get; init; }

    /// <summary>An icon name shown instead of the bullet in the default variant. The <c>steps</c>, <c>checks</c> and <c>files</c> variants keep their own marker.</summary>
    public string? Icon { get; init; }

    /// <summary>A color for the bullet or <see cref="Icon"/> in the default variant.</summary>
    public KlyColor? Tone { get; init; }

    /// <summary>Marks the item as done in the <c>steps</c> and <c>checks</c> variants.</summary>
    public bool Done { get; init; }

    /// <summary>Marks the item as the current step in the <c>steps</c> and <c>checks</c> variants.</summary>
    public bool Current { get; init; }

    /// <summary>Text shown at the end of the row.</summary>
    public string? Meta { get; init; }

    /// <summary>The URL of the item. In the <c>files</c> variant, a click opens the item in the file preview when a <c>KlyFilePreviewProvider</c> is mounted.</summary>
    public string? Href { get; init; }

    /// <summary>The file name of a file item, used for the extension badge and the preview. It defaults to <see cref="Text"/>.</summary>
    public string? FileName { get; init; }

    /// <summary>The size of a file item in bytes.</summary>
    public long? FileSize { get; init; }

    /// <summary>The MIME type of a file item, used for the extension badge and to detect the preview kind.</summary>
    public string? MimeType { get; init; }
}

/// <summary>The state of an event of a <see cref="DisplayKind.Timeline"/> block.</summary>
public enum DisplayTimelineState
{
    /// <summary>The event happened. This is the state of an event whose state is not set.</summary>
    Done,

    /// <summary>The event is happening now.</summary>
    Current,

    /// <summary>The event has not happened yet.</summary>
    Upcoming,
}

/// <summary>One event of a <see cref="DisplayKind.Timeline"/> block.</summary>
/// <param name="Title">The event title.</param>
/// <param name="Description">A line under the title.</param>
/// <param name="Time">The time text shown beside the title. It is displayed as given and takes precedence over <paramref name="Date"/> for the text; in the <c>log</c> variant it replaces the time part of the date.</param>
/// <param name="Date">When the event happened. It is shown in the short date and time format of the resolved culture when <paramref name="Time"/> is not set, set as the <c>datetime</c> of the <c>time</c> element, and starts each line of the <c>log</c> variant. It is shown in its own offset.</param>
/// <param name="Icon">An icon name shown in the marker instead of the event number (or the dot of the <c>log</c> variant).</param>
/// <param name="ActorName">The name of the person behind the event.</param>
/// <param name="State">The event state. When <see langword="null"/>, <see cref="DisplayTimelineState.Done"/> is used.</param>
/// <param name="Tone">A color for the marker, replacing the primary color of done and current events and the border of upcoming ones.</param>
public sealed record DisplayTimelineItem(
    string Title,
    string? Description = null,
    string? Time = null,
    DateTimeOffset? Date = null,
    string? Icon = null,
    string? ActorName = null,
    DisplayTimelineState? State = null,
    KlyColor? Tone = null);

/// <summary>One section of a <see cref="DisplayKind.Accordion"/> block.</summary>
/// <param name="Label">The section header.</param>
/// <param name="Content">The section text, used when <paramref name="Template"/> is not set.</param>
/// <param name="Icon">An icon name shown before the label.</param>
/// <param name="DefaultOpen">Whether the section starts open.</param>
/// <param name="Template">Markup for the section body. It takes precedence over <paramref name="Content"/>.</param>
public sealed record DisplayAccordionItem(
    string Label,
    string? Content = null,
    string? Icon = null,
    bool DefaultOpen = false,
    RenderFragment? Template = null);
