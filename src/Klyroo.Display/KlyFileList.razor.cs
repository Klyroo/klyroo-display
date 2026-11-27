namespace Klyroo.Display;

/// <summary>
/// A list of files, each with an extension badge, its name, its size and its date. Clicking a file opens the whole list as a gallery in the file preview,
/// at that file, when a <see cref="KlyFilePreviewProvider"/> is mounted and the page is interactive. Otherwise the name is a plain link to the file.
/// </summary>
/// <example>
/// <code>
/// &lt;KlyFilePreviewProvider /&gt;
/// &lt;KlyFileList Title="Attachments" Files="@files" Max="5" /&gt;
/// </code>
/// </example>
public partial class KlyFileList
{
}
