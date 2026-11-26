namespace Klyroo.Display;

internal sealed class JsonLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    protected override CodeTokenKind Next()
    {
        var c = S[Pos];
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '/' && At(Pos + 1) == '/') return LineComment();
        if (c == '/' && At(Pos + 1) == '*') return BlockComment();
        if (c == '"')
        {
            Quoted('"');
            var after = SkipWhitespace(Pos);
            return At(after) == ':' ? CodeTokenKind.Property : CodeTokenKind.String;
        }
        if (char.IsAsciiDigit(c) || (c == '-' && char.IsAsciiDigit(At(Pos + 1))))
        {
            if (c == '-') Pos++;
            return Number();
        }
        if (IsIdentStart(c))
        {
            var from = Pos;
            Pos = IdentEnd(Pos);
            var word = Slice(from, Pos);
            return word.SequenceEqual("true") || word.SequenceEqual("false") || word.SequenceEqual("null") ? CodeTokenKind.Keyword : CodeTokenKind.Plain;
        }
        if (c is '{' or '}' or '[' or ']' or ',' or ':') return Punctuation();
        Pos++;
        return CodeTokenKind.Plain;
    }
}

// HTML and XML. In HTML, the contents of <script> and <style> are highlighted as JavaScript and CSS, and Razor's @expressions and @* comments *@ are recognized.
internal sealed class MarkupLexer(string source, int start, int end, TokenWriter writer, bool html) : Lexer(source, start, end, writer)
{
    private bool _inTag;
    private bool _closing;
    private bool _expectName;
    private bool _afterEquals;
    private string? _rawName;
    private string? _pendingRaw;

    protected override CodeTokenKind Next() => _inTag ? InTag() : Text();

    private CodeTokenKind Text()
    {
        if (_pendingRaw is { } raw)
        {
            _pendingRaw = null;
            var index = Slice(Pos, End).IndexOf("</" + raw, StringComparison.OrdinalIgnoreCase);
            var stop = index < 0 ? End : Pos + index;
            if (stop > Pos)
            {
                Lexer inner = raw == "script"
                    ? new JavaScriptLexer(S, Pos, stop, W, typescript: false)
                    : new CssLexer(S, Pos, stop, W, CssDialect.Css);
                inner.Run();
                Pos = stop;
                return AlreadyEmitted;
            }
        }
        var c = S[Pos];
        if (c == '<')
        {
            if (StartsWith("<!--", Pos)) return BlockComment("-->", 4);
            if (StartsWith("<![CDATA[", Pos))
            {
                BlockComment("]]>", 9);
                return CodeTokenKind.String;
            }
            var next = At(Pos + 1);
            if (next is '!' or '?')
            {
                var close = Slice(Pos, End).IndexOf('>');
                Pos = close < 0 ? End : Pos + close + 1;
                return CodeTokenKind.Keyword;
            }
            if (next == '/' && IsIdentStart(At(Pos + 2)))
            {
                Pos += 2;
                return StartTag(closing: true);
            }
            if (IsIdentStart(next))
            {
                Pos++;
                return StartTag(closing: false);
            }
            Pos++;
            return CodeTokenKind.Plain;
        }
        if (html && c == '@' && AtRazor(Pos)) return Razor();
        var i = Pos + 1;
        while (i < End && S[i] != '<' && !(html && S[i] == '@' && AtRazor(i))) i++;
        Pos = i;
        return CodeTokenKind.Plain;
    }

    private CodeTokenKind StartTag(bool closing)
    {
        _inTag = true;
        _closing = closing;
        _expectName = true;
        _afterEquals = false;
        return CodeTokenKind.Punctuation;
    }

    private bool AtRazor(int index)
    {
        if (index > 0 && char.IsLetterOrDigit(S[index - 1])) return false;
        var next = At(index + 1);
        return IsIdentStart(next) || next is '(' or '*' or '{';
    }

    private CodeTokenKind Razor()
    {
        if (At(Pos + 1) == '*')
        {
            BlockComment("*@");
            return CodeTokenKind.Comment;
        }
        if (IsIdentStart(At(Pos + 1)))
        {
            Pos = IdentEnd(Pos + 1);
            while (At(Pos) == '.' && IsIdentStart(At(Pos + 1))) Pos = IdentEnd(Pos + 1);
        }
        else
        {
            Pos++;
        }
        return CodeTokenKind.Keyword;
    }

    private CodeTokenKind InTag()
    {
        var c = S[Pos];
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '>')
        {
            Pos++;
            _inTag = false;
            if (!_closing && _rawName is not null) _pendingRaw = _rawName;
            return CodeTokenKind.Punctuation;
        }
        if (c == '/' && At(Pos + 1) == '>')
        {
            Pos += 2;
            _inTag = false;
            return CodeTokenKind.Punctuation;
        }
        if (c == '<')
        {
            // A tag that was never closed: the next tag starts here.
            _inTag = false;
            return Text();
        }
        if (c == '=')
        {
            Pos++;
            _afterEquals = true;
            return CodeTokenKind.Punctuation;
        }
        if (c is '"' or '\'')
        {
            var close = Slice(Pos + 1, End).IndexOf(c);
            Pos = close < 0 ? End : Pos + 1 + close + 1;
            _afterEquals = false;
            return CodeTokenKind.String;
        }
        if (c == '/') return Punctuation();
        var i = Pos;
        while (i < End && !char.IsWhiteSpace(S[i]) && S[i] is not ('=' or '>' or '<' or '"' or '\'') && !(S[i] == '/' && At(i + 1) == '>')) i++;
        if (i == Pos) i++;
        var name = Slice(Pos, i);
        Pos = i;
        if (_expectName)
        {
            _expectName = false;
            _rawName = html && name.Equals("script", StringComparison.OrdinalIgnoreCase) ? "script"
                : html && name.Equals("style", StringComparison.OrdinalIgnoreCase) ? "style"
                : null;
            return CodeTokenKind.Tag;
        }
        if (_afterEquals)
        {
            _afterEquals = false;
            return CodeTokenKind.String;
        }
        return CodeTokenKind.Property;
    }
}

internal enum CssDialect
{
    Css,
    Scss,
    Less,
}

internal sealed class CssLexer(string source, int start, int end, TokenWriter writer, CssDialect dialect) : Lexer(source, start, end, writer)
{
    private const int StatementStart = 0;
    private const int Selector = 1;
    private const int PropertyName = 2;
    private const int Value = 3;
    private const int AtRule = 4;

    private int _state;
    private bool _inBrackets;
    private bool _urlPending;

    protected override CodeTokenKind Next()
    {
        var c = S[Pos];
        if (_urlPending)
        {
            _urlPending = false;
            if (c == '(')
            {
                var close = Slice(Pos, Math.Min(End, Pos + 2000)).IndexOf(')');
                Pos = close < 0 ? Math.Min(End, Pos + 2000) : Pos + close + 1;
                return CodeTokenKind.String;
            }
        }
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '/' && At(Pos + 1) == '*') return BlockComment();
        if (c == '/' && At(Pos + 1) == '/' && dialect != CssDialect.Css) return LineComment();
        if (c is '"' or '\'') return Quoted(c);
        switch (_state)
        {
            case StatementStart:
                return AtStatementStart(c);
            case Selector:
                return InSelector(c);
            case PropertyName:
                return InPropertyName(c);
            default:
                return InValue(c);
        }
    }

    private CodeTokenKind AtStatementStart(char c)
    {
        if (c is '}' or ';' or '{') return Punctuation();
        if (c == '@' && !(dialect == CssDialect.Less && IsLessVariable()))
        {
            _state = AtRule;
            Pos++;
            while (Pos < End && (IsIdentPart(S[Pos]) || S[Pos] == '-')) Pos++;
            return CodeTokenKind.Keyword;
        }
        _state = FindTerminator(Pos) == '{' ? Selector : PropertyName;
        return Next();
    }

    private bool IsLessVariable()
    {
        var i = Pos + 1;
        while (i < End && (IsIdentPart(S[i]) || S[i] == '-')) i++;
        return i > Pos + 1 && At(SkipBlanks(i)) == ':';
    }

    // The first of { ; } outside parentheses, strings and comments, within a bounded distance.
    private char FindTerminator(int from)
    {
        var depth = 0;
        var limit = Math.Min(End, from + 1000);
        for (var i = from; i < limit; i++)
        {
            var c = S[i];
            if (c is '"' or '\'')
            {
                i++;
                while (i < limit && S[i] != c && S[i] != '\n')
                {
                    if (S[i] == '\\') i++;
                    i++;
                }
            }
            else if (c == '/' && At(i + 1) == '*')
            {
                var close = Slice(i + 2, End).IndexOf("*/", StringComparison.Ordinal);
                if (close < 0) return '\0';
                i += close + 3;
            }
            else if (c == '/' && At(i + 1) == '/' && dialect != CssDialect.Css)
            {
                i = LineEnd(i);
            }
            else if (c == '#' && At(i + 1) == '{')
            {
                var close = InterpolationEnd(i);
                i = close > 0 ? close - 1 : i + 1;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (c == ')' && depth > 0)
            {
                depth--;
            }
            else if (depth == 0 && c is '{' or ';' or '}')
            {
                return c;
            }
        }
        return '\0';
    }

    // The index after the closing brace of #{ ... }, or -1.
    private int InterpolationEnd(int index)
    {
        var depth = 0;
        var limit = Math.Min(End, index + 500);
        for (var i = index + 1; i < limit; i++)
        {
            if (S[i] == '{') depth++;
            else if (S[i] == '}' && --depth == 0) return i + 1;
        }
        return -1;
    }

    private int NameEnd(int index)
    {
        var i = index;
        while (i < End && (IsIdentPart(S[i]) || S[i] == '-')) i++;
        return i;
    }

    private bool TryInterpolation()
    {
        if (At(Pos) != '#' || At(Pos + 1) != '{') return false;
        var close = InterpolationEnd(Pos);
        Pos = close > 0 ? close : Pos + 2;
        return true;
    }

    private CodeTokenKind InSelector(char c)
    {
        if (c == '{' || c == ';' || c == '}')
        {
            _state = StatementStart;
            _inBrackets = false;
            return Punctuation();
        }
        if (TryInterpolation()) return CodeTokenKind.Plain;
        if (c is '.' or '#' && (IsIdentStart(At(Pos + 1)) || At(Pos + 1) == '-'))
        {
            Pos = NameEnd(Pos + 1);
            return CodeTokenKind.Type;
        }
        if (c == ':')
        {
            Pos++;
            if (At(Pos) == ':') Pos++;
            if (IsIdentStart(At(Pos)) || At(Pos) == '-')
            {
                Pos = NameEnd(Pos);
                return CodeTokenKind.Keyword;
            }
            return CodeTokenKind.Punctuation;
        }
        if (c == '[')
        {
            _inBrackets = true;
            return Punctuation();
        }
        if (c == ']')
        {
            _inBrackets = false;
            return Punctuation();
        }
        if (char.IsAsciiDigit(c)) return CssNumber();
        if (IsIdentStart(c) || (c == '-' && (IsIdentStart(At(Pos + 1)) || At(Pos + 1) == '-')))
        {
            Pos = NameEnd(Pos);
            return _inBrackets ? CodeTokenKind.Property : CodeTokenKind.Tag;
        }
        return Punctuation();
    }

    private CodeTokenKind InPropertyName(char c)
    {
        if (c == ';' || c == '}' || c == '{')
        {
            _state = StatementStart;
            return Punctuation();
        }
        if (c == ':')
        {
            _state = Value;
            return Punctuation();
        }
        if (TryInterpolation()) return CodeTokenKind.Property;
        if (IsIdentStart(c) || c is '-' or '$' or '@' or '*')
        {
            Pos++;
            while (Pos < End && (IsIdentPart(S[Pos]) || S[Pos] == '-')) Pos++;
            return CodeTokenKind.Property;
        }
        return Punctuation();
    }

    private CodeTokenKind InValue(char c)
    {
        if (_state == Value && c is ';' or '}' or '{')
        {
            _state = StatementStart;
            return Punctuation();
        }
        if (_state == AtRule && c is ';' or '{' or '}')
        {
            _state = StatementStart;
            return Punctuation();
        }
        if (TryInterpolation()) return CodeTokenKind.Plain;
        if (c == '!' && IsIdentStart(At(Pos + 1)))
        {
            Pos = IdentEnd(Pos + 1);
            return CodeTokenKind.Keyword;
        }
        if (c == '#' && char.IsAsciiHexDigit(At(Pos + 1)))
        {
            var i = Pos + 1;
            while (i < End && char.IsAsciiHexDigit(S[i])) i++;
            if (!(i < End && (IsIdentPart(S[i]) || S[i] == '-')))
            {
                Pos = i;
                return CodeTokenKind.Number;
            }
        }
        if (char.IsAsciiDigit(c) || (c is '.' or '-' or '+' && (char.IsAsciiDigit(At(Pos + 1)) || (At(Pos + 1) == '.' && char.IsAsciiDigit(At(Pos + 2))))))
        {
            return CssNumber();
        }
        if (c is '$' or '@' && (IsIdentStart(At(Pos + 1)) || At(Pos + 1) == '-'))
        {
            Pos = NameEnd(Pos + 1);
            return CodeTokenKind.Property;
        }
        if (IsIdentStart(c) || (c == '-' && (IsIdentStart(At(Pos + 1)) || At(Pos + 1) == '-')))
        {
            var from = Pos;
            Pos = NameEnd(Pos);
            var word = Slice(from, Pos);
            if (At(Pos) == '(')
            {
                if (word.Equals("url", StringComparison.OrdinalIgnoreCase) && At(SkipBlanks(Pos + 1)) is not ('"' or '\'')) _urlPending = true;
                return CodeTokenKind.Function;
            }
            if (word.StartsWith("--", StringComparison.Ordinal)) return CodeTokenKind.Property;
            if (_state == AtRule && At(SkipBlanks(Pos)) == ':') return CodeTokenKind.Property;
            return CodeTokenKind.Plain;
        }
        return Punctuation();
    }

    // 12, -1.5, .5em, 50%, 1e3px
    private CodeTokenKind CssNumber()
    {
        var i = Pos;
        if (S[i] is '+' or '-') i++;
        while (i < End && char.IsAsciiDigit(S[i])) i++;
        if (At(i) == '.' && char.IsAsciiDigit(At(i + 1)))
        {
            i++;
            while (i < End && char.IsAsciiDigit(S[i])) i++;
        }
        if ((At(i) | 0x20) == 'e' && (char.IsAsciiDigit(At(i + 1)) || (At(i + 1) is '+' or '-' && char.IsAsciiDigit(At(i + 2)))))
        {
            i += 2;
            while (i < End && char.IsAsciiDigit(S[i])) i++;
        }
        if (At(i) == '%') i++;
        else
        {
            while (i < End && char.IsAsciiLetter(S[i])) i++;
        }
        Pos = Math.Max(i, Pos + 1);
        return CodeTokenKind.Number;
    }
}
