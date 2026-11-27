namespace Klyroo.Display;

/// <summary>
/// A block of source code with a label, syntax highlighting from <see cref="CodeHighlighter"/>, a copy button, optional line numbers and emphasized lines.
/// Show a value that must stay hidden with <see cref="KlySecretBlock"/> instead.
/// </summary>
/// <remarks>
/// The code is highlighted lexically, with the colors of the Klyroo.UI theme, so it follows light and dark mode and the palette. The text content of the block
/// is exactly the code, and markup in it is escaped. The copy button needs an interactive render mode: in static server rendering the block is shown and the button does nothing.
/// </remarks>
/// <example>
/// <code>
/// &lt;KlyCodeBlock Code="@sql" Language="CodeLanguage.Sql" Label="Query" LineNumbers HighlightLines="@([2, new CodeLineRange(4, 5)])" /&gt;
/// </code>
/// </example>
public partial class KlyCodeBlock
{
}
