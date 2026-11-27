namespace Klyroo.Display;

/// <summary>
/// A run of lines of a code, counted from 1 and inclusive at both ends, such as the lines a code block emphasizes. A single line converts
/// implicitly from an <see cref="int"/>, so <c>[3, new CodeLineRange(6, 9)]</c> is lines 3 and 6 to 9.
/// </summary>
/// <param name="Start">The first line, at least 1.</param>
/// <param name="End">The last line, not before <paramref name="Start"/>.</param>
public readonly record struct CodeLineRange(int Start, int End)
{
    /// <summary>Creates the range of one line.</summary>
    /// <param name="line">The line, counted from 1.</param>
    public CodeLineRange(int line) : this(line, line)
    {
    }

    /// <summary>Whether a line (counted from 1) is in the range. A range whose end is before its start contains no line.</summary>
    /// <param name="line">The line, counted from 1.</param>
    public bool Contains(int line) => line >= Start && line <= End;

    /// <summary>Converts a line number to the range of that line.</summary>
    /// <param name="line">The line, counted from 1.</param>
    public static implicit operator CodeLineRange(int line) => new(line, line);

    /// <summary>Creates a range from its two ends.</summary>
    /// <param name="start">The first line, counted from 1.</param>
    /// <param name="end">The last line.</param>
    public static CodeLineRange Of(int start, int end) => new(start, end);
}
