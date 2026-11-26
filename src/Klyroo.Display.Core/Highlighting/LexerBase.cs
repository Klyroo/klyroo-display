namespace Klyroo.Display;

/// <summary>Collects the tokens of a scan: plain and punctuation runs are merged, and the gaps are filled so the tokens always cover the input.</summary>
internal sealed class TokenWriter(string source)
{
    private readonly List<(CodeTokenKind Kind, int Start, int End)> _tokens = [];
    private int _position;

    public int Position => _position;

    public void Emit(CodeTokenKind kind, int start, int end)
    {
        if (end > source.Length) end = source.Length;
        if (start < _position) start = _position;
        if (start > _position) Add(CodeTokenKind.Plain, _position, start);
        if (end > start) Add(kind, start, end);
    }

    public List<CodeToken> Finish()
    {
        if (_position < source.Length) Add(CodeTokenKind.Plain, _position, source.Length);
        var result = new List<CodeToken>(_tokens.Count);
        foreach (var (kind, start, end) in _tokens)
        {
            result.Add(new CodeToken(kind, source.Substring(start, end - start)));
        }
        return result;
    }

    private void Add(CodeTokenKind kind, int start, int end)
    {
        if (_tokens.Count > 0 && kind is CodeTokenKind.Plain or CodeTokenKind.Punctuation)
        {
            var last = _tokens[^1];
            if (last.Kind == kind && last.End == start)
            {
                _tokens[^1] = (kind, last.Start, end);
                _position = end;
                return;
            }
        }
        _tokens.Add((kind, start, end));
        _position = end;
    }
}

/// <summary>A set of words that can be looked up by a slice of the source, without allocating.</summary>
internal sealed class WordSet
{
    private readonly string _text;
    private readonly bool _ignoreCase;
    private readonly HashSet<string> _words;
    private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _lookup;

    public WordSet(string words, bool ignoreCase = false)
    {
        _text = words;
        _ignoreCase = ignoreCase;
        _words = new HashSet<string>(words.Split(' ', StringSplitOptions.RemoveEmptyEntries), ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        _lookup = _words.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public bool Contains(ReadOnlySpan<char> word) => word.Length > 0 && _lookup.Contains(word);

    public WordSet Union(string moreWords) => new(_text + " " + moreWords, _ignoreCase);
}

/// <summary>
/// A scanner of a range of the source. <see cref="Next"/> consumes one token (it advances <see cref="Pos"/> and returns the kind); the loop
/// guarantees progress, and emits every character exactly once, so an error in a scanner can lose a color but never text.
/// </summary>
internal abstract class Lexer
{
    protected const CodeTokenKind AlreadyEmitted = (CodeTokenKind)(-1);

    protected readonly string S;
    protected readonly int End;
    protected readonly TokenWriter W;
    protected int Pos;

    // The last token that was not whitespace or a comment.
    protected bool HasLast;
    protected CodeTokenKind LastKind;
    protected int LastStart;
    protected int LastEnd;

    protected Lexer(string source, int start, int end, TokenWriter writer)
    {
        S = source;
        Pos = start;
        End = end;
        W = writer;
    }

    public void Run()
    {
        while (Pos < End)
        {
            var start = Pos;
            var kind = Next();
            if (kind == AlreadyEmitted)
            {
                if (Pos <= start) Pos = start + 1;
                continue;
            }
            if (Pos <= start) Pos = start + 1;
            if (Pos > End) Pos = End;
            W.Emit(kind, start, Pos);
            if (kind != CodeTokenKind.Comment && !char.IsWhiteSpace(S[start]))
            {
                HasLast = true;
                LastKind = kind;
                LastStart = start;
                LastEnd = Pos;
            }
        }
    }

    protected abstract CodeTokenKind Next();

    protected char At(int index) => index >= 0 && index < End ? S[index] : '\0';

    protected ReadOnlySpan<char> Slice(int start, int end) => S.AsSpan(start, Math.Max(0, end - start));

    protected bool StartsWith(string text, int index) => index >= 0 && index + text.Length <= End && string.CompareOrdinal(S, index, text, 0, text.Length) == 0;

    protected bool LastIs(string text) => HasLast && Slice(LastStart, LastEnd).SequenceEqual(text);

    protected bool LastIsKeywordIn(params string[] words)
    {
        if (!HasLast || LastKind != CodeTokenKind.Keyword) return false;
        var last = Slice(LastStart, LastEnd);
        foreach (var word in words)
        {
            if (last.SequenceEqual(word)) return true;
        }
        return false;
    }

    protected int LineEnd(int index)
    {
        var i = index;
        while (i < End && S[i] != '\n') i++;
        return i;
    }

    protected int SkipWhitespace(int index)
    {
        var i = index;
        while (i < End && char.IsWhiteSpace(S[i])) i++;
        return i;
    }

    // Spaces and tabs only: the next token on the same line.
    protected int SkipBlanks(int index)
    {
        var i = index;
        while (i < End && (S[i] == ' ' || S[i] == '\t')) i++;
        return i;
    }

    protected bool OnlyBlanksBefore(int index)
    {
        for (var i = index - 1; i >= 0; i--)
        {
            if (S[i] == '\n') return true;
            if (S[i] != ' ' && S[i] != '\t' && S[i] != '\r') return false;
        }
        return true;
    }

    protected static bool IsIdentStart(char c) => c == '_' || char.IsLetter(c);

    protected static bool IsIdentPart(char c) => c == '_' || char.IsLetterOrDigit(c);

    protected int IdentEnd(int index)
    {
        var i = index;
        while (i < End && IsIdentPart(S[i])) i++;
        return i;
    }

    protected static bool LooksLikeTypeName(ReadOnlySpan<char> word)
    {
        if (word.Length == 0 || !char.IsUpper(word[0])) return false;
        if (word.Length == 1) return true;
        foreach (var c in word)
        {
            if (char.IsLower(c)) return true;
        }
        return false;
    }

    protected CodeTokenKind Whitespace()
    {
        Pos = SkipWhitespace(Pos);
        return CodeTokenKind.Plain;
    }

    protected CodeTokenKind LineComment()
    {
        Pos = LineEnd(Pos);
        return CodeTokenKind.Comment;
    }

    protected CodeTokenKind BlockComment(string close = "*/", int openLength = 2)
    {
        var from = Pos + openLength;
        var index = from < End ? Slice(from, End).IndexOf(close, StringComparison.Ordinal) : -1;
        Pos = index < 0 ? End : from + index + close.Length;
        return CodeTokenKind.Comment;
    }

    // A string closed by the same quote, with backslash escapes. Without a closing quote it stops before the end of the line.
    protected CodeTokenKind Quoted(char quote, bool backslashEscapes = true)
    {
        var i = Pos + 1;
        while (i < End)
        {
            var c = S[i];
            if (c == quote)
            {
                i++;
                break;
            }
            if (c == '\n') break;
            if (backslashEscapes && c == '\\' && i + 1 < End)
            {
                i += 2;
                continue;
            }
            i++;
        }
        Pos = Math.Min(i, End);
        return CodeTokenKind.String;
    }

    // C-style numbers: 0x1F, 0b101, 1_000, 1.5e-3, .5, with a type suffix (u, l, f, d, m, n).
    protected CodeTokenKind Number()
    {
        var i = Pos;
        var hex = false;
        var dot = false;
        if (S[i] == '.')
        {
            dot = true;
            i++;
        }
        else if (S[i] == '0' && (At(i + 1) | 0x20) is 'x')
        {
            hex = true;
            i += 2;
        }
        else if (S[i] == '0' && (At(i + 1) | 0x20) is 'b' or 'o')
        {
            i += 2;
        }
        while (i < End)
        {
            var c = S[i];
            if (char.IsAsciiDigit(c) || c == '_' || (hex && char.IsAsciiHexDigit(c)))
            {
                i++;
            }
            else if (c == '.' && !hex && !dot && char.IsAsciiDigit(At(i + 1)))
            {
                dot = true;
                i++;
            }
            else if (!hex && (c | 0x20) == 'e' && (char.IsAsciiDigit(At(i + 1)) || (At(i + 1) is '+' or '-' && char.IsAsciiDigit(At(i + 2)))))
            {
                i += At(i + 1) is '+' or '-' ? 3 : 2;
            }
            else if (char.IsAsciiLetter(c))
            {
                i++;
            }
            else
            {
                break;
            }
        }
        Pos = i;
        return CodeTokenKind.Number;
    }

    protected CodeTokenKind Punctuation()
    {
        Pos++;
        return CodeTokenKind.Punctuation;
    }
}

/// <summary>The scanner of text that is not highlighted: one plain token.</summary>
internal sealed class PlainLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    protected override CodeTokenKind Next()
    {
        Pos = End;
        return CodeTokenKind.Plain;
    }
}
