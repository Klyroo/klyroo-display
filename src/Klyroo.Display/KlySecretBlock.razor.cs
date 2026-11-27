namespace Klyroo.Display;

/// <summary>
/// A value that is masked until the user reveals it, such as an API key or a token, with a Reveal and Conceal toggle and a copy button.
/// It is never highlighted.
/// </summary>
/// <remarks>
/// While the value is masked, the page contains the bullets and the last <see cref="VisibleChars"/> characters only. The toggle and the copy button need
/// an interactive render mode: in static server rendering the secret stays masked.
/// </remarks>
/// <example>
/// <code>
/// &lt;KlySecretBlock Label="API key" Value="@key" VisibleChars="4" /&gt;
/// </code>
/// </example>
public partial class KlySecretBlock
{
}
