namespace Klyroo.Display;

/// <summary>
/// Renders a read-only block of information: a heading, a text, an alert, a list of facts, a statistic, a breakdown,
/// a meter, a chart, a table, a diff, records, people, a list, a timeline, a date, an image, a code value, an empty state
/// or an accordion. Set <see cref="Kind"/> to choose the block, and fill the parameters that kind reads.
/// </summary>
/// <remarks>
/// Every kind is one component with the union of all parameters, so a parameter only has an effect on the kinds its summary names.
/// Copy buttons and expanding toggles need an interactive render mode; in static server rendering the block is shown but they do nothing.
/// </remarks>
public partial class KlyDisplay
{
}
