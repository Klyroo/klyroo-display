namespace Klyroo.Display.Maui;

/// <summary>Describes the file of a <see cref="KlyFileList"/> that the user tapped.</summary>
public sealed class KlyFileSelectedEventArgs : EventArgs
{
    internal KlyFileSelectedEventArgs(FileListItem item, int index, IReadOnlyList<FileListItem> items)
    {
        Item = item;
        Index = index;
        Items = items;
    }

    /// <summary>The item that was tapped.</summary>
    public FileListItem Item { get; }

    /// <summary>The position of the item in <see cref="KlyFileList.Files"/>, counted from 0.</summary>
    public int Index { get; }

    /// <summary>All the items of the list, which open as a gallery in the file preview.</summary>
    public IReadOnlyList<FileListItem> Items { get; }
}
