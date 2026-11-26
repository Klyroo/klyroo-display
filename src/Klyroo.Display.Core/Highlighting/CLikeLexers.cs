namespace Klyroo.Display;

internal sealed class CSharpLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    private static readonly WordSet Keywords = new(
        "abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit " +
        "extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out " +
        "override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true " +
        "try typeof uint ulong unchecked unsafe ushort using virtual void volatile while var async await record init required partial yield get set " +
        "where with nameof dynamic when");

    private int _attributeDepth;

    protected override CodeTokenKind Next()
    {
        var c = S[Pos];
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '/' && At(Pos + 1) == '/') return LineComment();
        if (c == '/' && At(Pos + 1) == '*') return BlockComment();
        if (c == '#' && OnlyBlanksBefore(Pos))
        {
            Pos = LineEnd(Pos);
            return CodeTokenKind.Keyword;
        }
        if (c == '"') return String(Pos, interpolated: false, verbatim: false);
        if (c is '$' or '@')
        {
            var i = Pos;
            var interpolated = false;
            var verbatim = false;
            while (i < End && i - Pos < 4 && S[i] is '$' or '@')
            {
                if (S[i] == '$') interpolated = true;
                else verbatim = true;
                i++;
            }
            if (At(i) == '"') return String(i, interpolated, verbatim);
            if (c == '@' && IsIdentStart(At(Pos + 1)))
            {
                Pos = IdentEnd(Pos + 1);
                return CodeTokenKind.Plain;
            }
            return Punctuation();
        }
        if (c == '\'') return Quoted('\'');
        if (char.IsAsciiDigit(c) || (c == '.' && char.IsAsciiDigit(At(Pos + 1)))) return Number();
        if (IsIdentStart(c)) return Word();
        if (c == '[')
        {
            if (_attributeDepth > 0 || OnlyBlanksBefore(Pos)) _attributeDepth++;
        }
        else if (c == ']' && _attributeDepth > 0)
        {
            _attributeDepth--;
        }
        return Punctuation();
    }

    // quote is the index of the opening quote; Pos is the start of the prefix.
    private CodeTokenKind String(int quote, bool interpolated, bool verbatim)
    {
        var quotes = 1;
        while (At(quote + quotes) == '"') quotes++;
        if (quotes >= 3)
        {
            // Raw string: it ends at a run of as many quotes as it opened with.
            var closing = new string('"', quotes);
            var from = quote + quotes;
            var index = from < End ? Slice(from, End).IndexOf(closing, StringComparison.Ordinal) : -1;
            Pos = index < 0 ? End : from + index + quotes;
            return CodeTokenKind.String;
        }
        if (quotes == 2)
        {
            // An empty string.
            Pos = quote + 2;
            return CodeTokenKind.String;
        }
        var i = quote + 1;
        while (i < End)
        {
            var c = S[i];
            if (c == '"')
            {
                if (verbatim && At(i + 1) == '"')
                {
                    i += 2;
                    continue;
                }
                i++;
                break;
            }
            if (!verbatim && (c == '\n' || c == '\r')) break;
            if (!verbatim && c == '\\' && At(i + 1) is not ('\n' or '\r' or '\0'))
            {
                i += 2;
                continue;
            }
            if (interpolated && c == '{')
            {
                if (At(i + 1) == '{')
                {
                    i += 2;
                    continue;
                }
                var after = SkipHole(i + 1, multiline: verbatim);
                i = after > 0 ? after : i + 1;
                continue;
            }
            i++;
        }
        Pos = Math.Min(i, End);
        return CodeTokenKind.String;
    }

    // Returns the index after the closing brace of an interpolation hole, or -1 when it does not close within a bounded distance.
    private int SkipHole(int index, bool multiline)
    {
        var depth = 1;
        var limit = Math.Min(End, index + 2000);
        var i = index;
        while (i < limit)
        {
            if (!SpendLookahead()) return -1;
            var c = S[i];
            if (c == '\n' && !multiline) return -1;
            if (c == '{') depth++;
            else if (c == '}' && --depth == 0) return i + 1;
            else if (c == '"')
            {
                i++;
                while (i < limit && S[i] != '"' && S[i] != '\n')
                {
                    if (S[i] == '\\') i++;
                    i++;
                }
            }
            else if (c == '\'' && At(i + 2) == '\'')
            {
                i += 2;
            }
            i++;
        }
        return -1;
    }

    private CodeTokenKind Word()
    {
        var start = Pos;
        Pos = IdentEnd(Pos);
        var word = Slice(start, Pos);
        var isMember = HasLast && LastKind == CodeTokenKind.Punctuation && S[LastEnd - 1] == '.' && !(LastEnd - LastStart >= 2 && S[LastEnd - 2] == '.');
        if (isMember) return At(SkipBlanks(Pos)) == '(' ? CodeTokenKind.Function : CodeTokenKind.Plain;
        if (Keywords.Contains(word)) return CodeTokenKind.Keyword;
        if (LastIsKeywordIn("class", "struct", "interface", "enum", "record", "namespace", "new")) return CodeTokenKind.Type;
        if (_attributeDepth > 0 && LooksLikeTypeName(word) && HasLast && LastKind == CodeTokenKind.Punctuation && S[LastEnd - 1] is '[' or ',') return CodeTokenKind.Property;
        if (At(SkipBlanks(Pos)) == '(') return CodeTokenKind.Function;
        return LooksLikeTypeName(word) ? CodeTokenKind.Type : CodeTokenKind.Plain;
    }
}

internal sealed class JavaScriptLexer : Lexer
{
    private static readonly WordSet JsKeywords = new(
        "async await break case catch class const continue debugger default delete do else export extends false finally for from function get if import " +
        "in instanceof let new null of return set static super switch this throw true try typeof undefined var void while with yield");

    private static readonly WordSet TsKeywords = JsKeywords.Union(
        "abstract any as asserts bigint boolean declare enum implements infer interface is keyof module namespace never number object override " +
        "private protected public readonly satisfies string symbol type unique unknown");

    private static readonly WordSet RegexPrecedingKeywords = new("return typeof instanceof in of new delete void throw case do else yield await");

    private readonly WordSet _keywords;

    // For each open template literal expression (${ ... }), the number of braces opened inside it.
    private readonly Stack<int> _templates = new();
    private bool _resumeTemplate;

    public JavaScriptLexer(string source, int start, int end, TokenWriter writer, bool typescript) : base(source, start, end, writer)
    {
        _keywords = typescript ? TsKeywords : JsKeywords;
    }

    protected override CodeTokenKind Next()
    {
        if (_resumeTemplate)
        {
            _resumeTemplate = false;
            return TemplateChunk();
        }
        var c = S[Pos];
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '/' && At(Pos + 1) == '/') return LineComment();
        if (c == '/' && At(Pos + 1) == '*') return BlockComment();
        if (c == '#' && Pos == 0 && At(1) == '!') return LineComment();
        if (c is '"' or '\'') return Quoted(c);
        if (c == '`')
        {
            Pos++;
            return TemplateChunk();
        }
        if (c == '/' && RegexAllowed() && TryRegex()) return CodeTokenKind.String;
        if (char.IsAsciiDigit(c) || (c == '.' && char.IsAsciiDigit(At(Pos + 1)))) return Number();
        if (IsIdentStart(c) || c == '$') return Word();
        if ((c == '@' || c == '#') && IsIdentStart(At(Pos + 1)))
        {
            Pos = IdentEnd(Pos + 1);
            return CodeTokenKind.Property;
        }
        if (_templates.Count > 0)
        {
            if (c == '{') _templates.Push(_templates.Pop() + 1);
            else if (c == '}')
            {
                var open = _templates.Pop();
                if (open == 0)
                {
                    _resumeTemplate = true;
                    Pos++;
                    return CodeTokenKind.String;
                }
                _templates.Push(open - 1);
            }
        }
        return Punctuation();
    }

    // Pos is just after the opening backtick, or just after the closing brace of an expression.
    private CodeTokenKind TemplateChunk()
    {
        var i = Pos;
        while (i < End)
        {
            var c = S[i];
            if (c == '\\')
            {
                i += 2;
                continue;
            }
            if (c == '`')
            {
                i++;
                break;
            }
            if (c == '$' && At(i + 1) == '{' && _templates.Count < 100)
            {
                _templates.Push(0);
                i += 2;
                break;
            }
            i++;
        }
        Pos = Math.Min(i, End);
        return CodeTokenKind.String;
    }

    private bool RegexAllowed()
    {
        if (!HasLast) return true;
        return LastKind switch
        {
            CodeTokenKind.Keyword => RegexPrecedingKeywords.Contains(Slice(LastStart, LastEnd)),
            CodeTokenKind.Punctuation => S[LastEnd - 1] is not (')' or ']' or '}'),
            _ => false,
        };
    }

    private bool TryRegex()
    {
        var i = Pos + 1;
        var limit = Math.Min(End, Pos + 1000);
        var inClass = false;
        while (i < limit)
        {
            if (!SpendLookahead()) return false;
            var c = S[i];
            if (c == '\n') return false;
            if (c == '\\')
            {
                i += 2;
                continue;
            }
            if (c == '[') inClass = true;
            else if (c == ']') inClass = false;
            else if (c == '/' && !inClass)
            {
                i++;
                while (i < End && char.IsAsciiLetter(S[i])) i++;
                Pos = i;
                return true;
            }
            i++;
        }
        return false;
    }

    private CodeTokenKind Word()
    {
        var start = Pos;
        Pos++;
        while (Pos < End && (IsIdentPart(S[Pos]) || S[Pos] == '$')) Pos++;
        var word = Slice(start, Pos);
        var isCall = At(SkipBlanks(Pos)) == '(';
        // A member access (a.b, a?.b), but not a spread (...b).
        if (HasLast && LastKind == CodeTokenKind.Punctuation && S[LastEnd - 1] == '.' && !(LastEnd - LastStart >= 2 && S[LastEnd - 2] == '.'))
        {
            return isCall ? CodeTokenKind.Function : CodeTokenKind.Property;
        }
        if (_keywords.Contains(word) && !(isCall && word is "get" or "set" or "of" or "from" or "type" or "is" or "module" or "async")) return CodeTokenKind.Keyword;
        if (LastIsKeywordIn("function")) return CodeTokenKind.Function;
        if (LastIsKeywordIn("class", "extends", "implements", "interface", "enum", "new")) return LooksLikeTypeName(word) || LastIsKeywordIn("class", "interface", "enum") ? CodeTokenKind.Type : isCall ? CodeTokenKind.Function : CodeTokenKind.Plain;
        if (isCall) return CodeTokenKind.Function;
        return LooksLikeTypeName(word) ? CodeTokenKind.Type : CodeTokenKind.Plain;
    }
}
