namespace Klyroo.Display;

internal sealed class SqlLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    private static readonly WordSet Keywords = new(
        "SELECT FROM WHERE AND OR NOT NULL IS IN LIKE ILIKE BETWEEN EXISTS ANY ALL SOME AS DISTINCT TOP LIMIT OFFSET FETCH NEXT ROWS ONLY ORDER BY GROUP " +
        "HAVING UNION INTERSECT EXCEPT JOIN INNER LEFT RIGHT FULL OUTER CROSS NATURAL ON USING INSERT INTO VALUES UPDATE SET DELETE MERGE MATCHED CREATE " +
        "ALTER DROP TRUNCATE TABLE VIEW INDEX UNIQUE PRIMARY KEY FOREIGN REFERENCES CONSTRAINT CHECK DEFAULT AUTO_INCREMENT IDENTITY SEQUENCE SCHEMA " +
        "DATABASE TRIGGER PROCEDURE FUNCTION RETURNS RETURN BEGIN END DECLARE IF ELSE ELSEIF THEN WHEN CASE WHILE LOOP FOR EACH ROW COMMIT ROLLBACK " +
        "TRANSACTION SAVEPOINT START GRANT REVOKE WITH RECURSIVE OVER PARTITION WINDOW ASC DESC CASCADE RESTRICT TRUE FALSE ADD COLUMN RENAME TO COLLATE " +
        "ESCAPE LATERAL EXPLAIN ANALYZE VACUUM TEMP TEMPORARY UNLOGGED REPLACE IGNORE CAST",
        ignoreCase: true);

    private static readonly WordSet Types = new(
        "INT INTEGER BIGINT SMALLINT TINYINT DECIMAL NUMERIC FLOAT REAL DOUBLE PRECISION BOOLEAN BOOL BIT CHAR VARCHAR NVARCHAR NCHAR TEXT CLOB BLOB BYTEA " +
        "DATE TIME TIMESTAMP TIMESTAMPTZ DATETIME DATETIME2 INTERVAL UUID JSON JSONB XML MONEY SERIAL BIGSERIAL BINARY VARBINARY ARRAY",
        ignoreCase: true);

    protected override CodeTokenKind Next()
    {
        var c = S[Pos];
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '-' && At(Pos + 1) == '-') return LineComment();
        if (c == '/' && At(Pos + 1) == '*') return BlockComment();
        if (c == '\'') return SingleQuoted();
        if (c == '"')
        {
            Quoted('"', backslashEscapes: false);
            return CodeTokenKind.String;
        }
        if (c == '`')
        {
            Quoted('`', backslashEscapes: false);
            return CodeTokenKind.Property;
        }
        if (c == '$') return Dollar();
        if (c == '@')
        {
            Pos++;
            while (At(Pos) == '@') Pos++;
            Pos = IdentEnd(Pos);
            return CodeTokenKind.Property;
        }
        if (c == ':' && At(Pos + 1) != ':' && (Pos == 0 || S[Pos - 1] != ':') && IsIdentStart(At(Pos + 1)))
        {
            Pos = IdentEnd(Pos + 1);
            return CodeTokenKind.Property;
        }
        if (char.IsAsciiDigit(c) || (c == '.' && char.IsAsciiDigit(At(Pos + 1)))) return Number();
        if (IsIdentStart(c))
        {
            var from = Pos;
            Pos = IdentEnd(Pos);
            var word = Slice(from, Pos);
            if (Keywords.Contains(word)) return CodeTokenKind.Keyword;
            if (Types.Contains(word)) return CodeTokenKind.Type;
            return At(SkipBlanks(Pos)) == '(' ? CodeTokenKind.Function : CodeTokenKind.Plain;
        }
        return Punctuation();
    }

    // A string closed by an apostrophe, where a doubled apostrophe is an escaped one. It may span lines.
    private CodeTokenKind SingleQuoted()
    {
        var i = Pos + 1;
        while (i < End)
        {
            if (S[i] == '\'')
            {
                if (At(i + 1) == '\'')
                {
                    i += 2;
                    continue;
                }
                i++;
                break;
            }
            i++;
        }
        Pos = Math.Min(i, End);
        return CodeTokenKind.String;
    }

    // $1 is a parameter; $$ ... $$ and $tag$ ... $tag$ are PostgreSQL dollar-quoted strings.
    private CodeTokenKind Dollar()
    {
        if (char.IsAsciiDigit(At(Pos + 1)))
        {
            Pos++;
            while (char.IsAsciiDigit(At(Pos))) Pos++;
            return CodeTokenKind.Property;
        }
        var tagEnd = IdentEnd(Pos + 1);
        if (At(tagEnd) == '$')
        {
            var delimiter = S.Substring(Pos, tagEnd + 1 - Pos);
            var from = tagEnd + 1;
            var close = from < End ? Slice(from, End).IndexOf(delimiter, StringComparison.Ordinal) : -1;
            Pos = close < 0 ? End : from + close + delimiter.Length;
            return CodeTokenKind.String;
        }
        return Punctuation();
    }
}

internal sealed class PythonLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    private static readonly WordSet Keywords = new(
        "False None True and as assert async await break class continue def del elif else except finally for from global if import in is lambda " +
        "nonlocal not or pass raise return try while with yield self cls");

    private static readonly WordSet BuiltinTypes = new("int str float bool list dict set tuple bytes object type frozenset bytearray complex");

    protected override CodeTokenKind Next()
    {
        var c = S[Pos];
        if (char.IsWhiteSpace(c)) return Whitespace();
        if (c == '#') return LineComment();
        if ((c is '"' or '\'' || IsIdentStart(c)) && TryString()) return CodeTokenKind.String;
        if (char.IsAsciiDigit(c) || (c == '.' && char.IsAsciiDigit(At(Pos + 1)))) return Number();
        if (c == '@' && OnlyBlanksBefore(Pos) && IsIdentStart(At(Pos + 1)))
        {
            Pos = IdentEnd(Pos + 1);
            while (At(Pos) == '.' && IsIdentStart(At(Pos + 1))) Pos = IdentEnd(Pos + 1);
            return CodeTokenKind.Property;
        }
        if (IsIdentStart(c)) return Word();
        return Punctuation();
    }

    // Prefixes (r, b, u, f and their combinations) and triple quotes.
    private bool TryString()
    {
        var i = Pos;
        while (i < End && i - Pos < 3 && S[i] is 'r' or 'R' or 'b' or 'B' or 'u' or 'U' or 'f' or 'F') i++;
        var quote = At(i);
        if (quote is not ('"' or '\'')) return false;
        if (At(i + 1) == quote && At(i + 2) == quote)
        {
            var j = i + 3;
            while (j < End)
            {
                if (S[j] == '\\')
                {
                    j += 2;
                    continue;
                }
                if (S[j] == quote && At(j + 1) == quote && At(j + 2) == quote)
                {
                    j += 3;
                    break;
                }
                j++;
            }
            Pos = Math.Min(j, End);
            return true;
        }
        var k = i + 1;
        while (k < End)
        {
            var c = S[k];
            if (c == quote)
            {
                k++;
                break;
            }
            if (c == '\n') break;
            if (c == '\\')
            {
                k += At(k + 1) == '\r' && At(k + 2) == '\n' ? 3 : 2;
                continue;
            }
            k++;
        }
        Pos = Math.Min(k, End);
        return true;
    }

    private CodeTokenKind Word()
    {
        var from = Pos;
        Pos = IdentEnd(Pos);
        var word = Slice(from, Pos);
        var isCall = At(SkipBlanks(Pos)) == '(';
        if (HasLast && LastKind == CodeTokenKind.Punctuation && S[LastEnd - 1] == '.' && !(LastEnd - LastStart >= 2 && S[LastEnd - 2] == '.'))
        {
            return isCall ? CodeTokenKind.Function : CodeTokenKind.Property;
        }
        if (Keywords.Contains(word)) return CodeTokenKind.Keyword;
        if (LastIsKeywordIn("def")) return CodeTokenKind.Function;
        if (LastIsKeywordIn("class")) return CodeTokenKind.Type;
        if (isCall) return LooksLikeTypeName(word) ? CodeTokenKind.Type : CodeTokenKind.Function;
        if (BuiltinTypes.Contains(word) || LooksLikeTypeName(word)) return CodeTokenKind.Type;
        return CodeTokenKind.Plain;
    }
}

internal sealed class BashLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    private static readonly WordSet Keywords = new("if then else elif fi for while until do done case esac in function select time coproc");
    private static readonly WordSet CommandStarters = new("then do else elif if while until time");

    private readonly List<(string Word, bool StripTabs)> _heredocs = [];
    private bool _commandPosition = true;
    private bool _assignmentName;
    private bool _valueNext;
    private bool _bodyPending;

    protected override CodeTokenKind Next()
    {
        if (_bodyPending && _heredocs.Count > 0)
        {
            _bodyPending = false;
            return HeredocBody();
        }
        _bodyPending = false;
        var c = S[Pos];
        if (c == '\n')
        {
            Pos++;
            _commandPosition = true;
            _valueNext = false;
            _assignmentName = false;
            _bodyPending = _heredocs.Count > 0;
            return CodeTokenKind.Plain;
        }
        if (c is ' ' or '\t' or '\r')
        {
            Pos = SkipBlanks(Pos);
            while (At(Pos) == '\r' && At(Pos + 1) != '\n') Pos++;
            return CodeTokenKind.Plain;
        }
        if (c == '\\')
        {
            Pos += At(Pos + 1) == '\0' ? 1 : 2;
            if (At(Pos - 1) == '\r' && At(Pos) == '\n') Pos++;
            return CodeTokenKind.Plain;
        }
        if (c == '#' && (Pos == 0 || S[Pos - 1] is ' ' or '\t' or '\n' or '\r' or ';' or '|' or '&' or '(' or ')' or '{' or '}')) return LineComment();
        if (c == '"') return AfterWord(DoubleQuoted());
        if (c == '\'')
        {
            var close = Slice(Pos + 1, End).IndexOf('\'');
            Pos = close < 0 ? End : Pos + 1 + close + 1;
            return AfterWord(CodeTokenKind.String);
        }
        if (c == '$') return Dollar();
        if (c == '<' && At(Pos + 1) == '<' && At(Pos + 2) != '<' && TryHeredocStart()) return CodeTokenKind.Punctuation;
        if (c is '|' or '&' or ';' or '(' or '{')
        {
            Pos++;
            if (At(Pos) == c && c is '|' or '&') Pos++;
            _commandPosition = true;
            _valueNext = false;
            _assignmentName = false;
            return CodeTokenKind.Punctuation;
        }
        if (c == '=' && _assignmentName)
        {
            _assignmentName = false;
            _valueNext = true;
            Pos++;
            return CodeTokenKind.Punctuation;
        }
        if (c is ')' or '}' or '<' or '>' or '=' or '!' or '`')
        {
            Pos++;
            if (c == '`') _commandPosition = true;
            return CodeTokenKind.Punctuation;
        }
        return Word();
    }

    private CodeTokenKind AfterWord(CodeTokenKind kind)
    {
        _commandPosition = _valueNext;
        _valueNext = false;
        return kind;
    }

    private CodeTokenKind DoubleQuoted()
    {
        var i = Pos + 1;
        while (i < End)
        {
            var c = S[i];
            if (c == '\\')
            {
                i += 2;
                continue;
            }
            i++;
            if (c == '"') break;
        }
        Pos = Math.Min(i, End);
        return CodeTokenKind.String;
    }

    private CodeTokenKind Dollar()
    {
        var next = At(Pos + 1);
        if (next == '{')
        {
            var close = Slice(Pos + 2, End).IndexOf('}');
            Pos = close < 0 ? End : Pos + 2 + close + 1;
            return AfterWord(CodeTokenKind.Property);
        }
        if (next == '(')
        {
            Pos += 2;
            _commandPosition = true;
            return CodeTokenKind.Punctuation;
        }
        if (IsIdentStart(next))
        {
            Pos = IdentEnd(Pos + 1);
            return AfterWord(CodeTokenKind.Property);
        }
        if (char.IsAsciiDigit(next) || next is '@' or '*' or '#' or '?' or '$' or '!' or '-')
        {
            Pos += 2;
            return AfterWord(CodeTokenKind.Property);
        }
        return Punctuation();
    }

    private bool TryHeredocStart()
    {
        var i = Pos + 2;
        var strip = At(i) == '-';
        if (strip) i++;
        i = SkipBlanks(i);
        var quote = At(i) is '"' or '\'' ? At(i) : '\0';
        if (quote != '\0') i++;
        var wordStart = i;
        while (i < End && (IsIdentPart(S[i]) || S[i] == '-' || S[i] == '.')) i++;
        if (i == wordStart || _heredocs.Count >= 8) return false;
        var word = S.Substring(wordStart, i - wordStart);
        if (quote != '\0' && At(i) == quote) i++;
        _heredocs.Add((word, strip));
        Pos = i;
        return true;
    }

    // Pos is at the start of the line after the one that opened the heredoc: the body runs to the line that is the delimiter.
    private CodeTokenKind HeredocBody()
    {
        var (word, strip) = _heredocs[0];
        _heredocs.RemoveAt(0);
        var lineStart = Pos;
        while (lineStart < End)
        {
            var lineEnd = LineEnd(lineStart);
            var line = Slice(lineStart, lineEnd).TrimEnd('\r');
            if (strip) line = line.TrimStart('\t');
            if (line.SequenceEqual(word))
            {
                Pos = lineEnd;
                return CodeTokenKind.String;
            }
            lineStart = lineEnd + 1;
        }
        Pos = End;
        return CodeTokenKind.String;
    }

    private CodeTokenKind Word()
    {
        var from = Pos;
        var i = Pos;
        while (i < End && !char.IsWhiteSpace(S[i]) && S[i] is not ('|' or '&' or ';' or '(' or ')' or '<' or '>' or '"' or '\'' or '`' or '$' or '\\')) i++;
        if (i == from) i++;
        var word = Slice(from, i);
        var wasCommand = _commandPosition && !_valueNext;

        if (word.SequenceEqual("[") || word.SequenceEqual("[[") || word.SequenceEqual("]") || word.SequenceEqual("]]"))
        {
            Pos = i;
            _commandPosition = false;
            return CodeTokenKind.Punctuation;
        }

        var declaring = HasLast && LastKind == CodeTokenKind.Function && (LastIs("export") || LastIs("local") || LastIs("declare") || LastIs("readonly") || LastIs("typeset"));
        if ((_commandPosition || declaring) && !_valueNext && IsIdentStart(S[from]))
        {
            var nameEnd = IdentEnd(from);
            if (At(nameEnd) == '=' || (At(nameEnd) == '+' && At(nameEnd + 1) == '='))
            {
                Pos = nameEnd;
                _assignmentName = true;
                _commandPosition = false;
                return CodeTokenKind.Property;
            }
        }

        Pos = i;
        if (Keywords.Contains(word))
        {
            _commandPosition = CommandStarters.Contains(word);
            _valueNext = false;
            return CodeTokenKind.Keyword;
        }
        CodeTokenKind kind;
        if (wasCommand || LastIsKeywordIn("function")) kind = CodeTokenKind.Function;
        else if (word.Length > 1 && word[0] == '-' && (char.IsLetter(word[1]) || word[1] == '-')) kind = CodeTokenKind.Property;
        else if (word.IndexOfAnyExceptInRange('0', '9') < 0) kind = CodeTokenKind.Number;
        else kind = CodeTokenKind.Plain;
        return AfterWord(kind);
    }
}

internal sealed class YamlLexer(string source, int start, int end, TokenWriter writer) : Lexer(source, start, end, writer)
{
    private static readonly WordSet Constants = new("true false null ~", ignoreCase: true);

    private int _blockIndent = -1;
    private int _flow;

    protected override CodeTokenKind Next()
    {
        var atLineStart = Pos == 0 || S[Pos - 1] == '\n';
        if (_blockIndent >= 0 && atLineStart)
        {
            var i = Pos;
            while (i < End && S[i] == ' ') i++;
            var blank = i >= End || S[i] is '\n' or '\r';
            if (blank || i - Pos > _blockIndent)
            {
                Pos = LineEnd(Pos);
                if (Pos < End) Pos++;
                return CodeTokenKind.String;
            }
            _blockIndent = -1;
        }
        var c = S[Pos];
        if (atLineStart && (StartsWith("---", Pos) || StartsWith("...", Pos)) && At(Pos + 3) is ' ' or '\n' or '\r' or '\0' or '\t')
        {
            Pos += 3;
            return CodeTokenKind.Keyword;
        }
        if (c == '\n')
        {
            Pos++;
            return CodeTokenKind.Plain;
        }
        if (c is ' ' or '\t' or '\r')
        {
            Pos = SkipBlanks(Pos);
            while (At(Pos) == '\r' && At(Pos + 1) != '\n') Pos++;
            return CodeTokenKind.Plain;
        }
        if (c == '#' && (Pos == 0 || S[Pos - 1] is ' ' or '\t' or '\n')) return LineComment();
        if (c == '-' && _flow == 0 && IsBreak(At(Pos + 1))) return Punctuation();
        if (c == ':' && (IsBreak(At(Pos + 1)) || (_flow > 0 && At(Pos + 1) is ',' or ']' or '}'))) return Punctuation();
        if (c is '[' or '{')
        {
            _flow++;
            return Punctuation();
        }
        if (c is ']' or '}')
        {
            if (_flow > 0) _flow--;
            return Punctuation();
        }
        if (c == ',') return Punctuation();
        if (c is '"' or '\'')
        {
            if (c == '"') Quoted('"');
            else SingleQuoted();
            var after = SkipBlanks(Pos);
            return At(after) == ':' && IsBreak(At(after + 1)) ? CodeTokenKind.Property : CodeTokenKind.String;
        }
        if ((c is '&' or '*' or '!') && At(Pos + 1) is not (' ' or '\n' or '\r' or '\0'))
        {
            var i = Pos + 1;
            while (i < End && !char.IsWhiteSpace(S[i]) && !(_flow > 0 && S[i] is ',' or ']' or '}')) i++;
            Pos = i;
            return CodeTokenKind.Type;
        }
        if (c is '|' or '>' && _flow == 0 && TryBlockScalar()) return CodeTokenKind.Punctuation;
        return PlainScalar();
    }

    private static bool IsBreak(char c) => c is ' ' or '\t' or '\n' or '\r' or '\0';

    private void SingleQuoted()
    {
        var i = Pos + 1;
        while (i < End && S[i] != '\n')
        {
            if (S[i] == '\'')
            {
                if (At(i + 1) == '\'')
                {
                    i += 2;
                    continue;
                }
                i++;
                break;
            }
            i++;
        }
        Pos = Math.Min(i, End);
    }

    // | or > followed by optional indentation and chomping indicators and then the end of the line (or a comment).
    private bool TryBlockScalar()
    {
        var j = Pos + 1;
        while (At(j) is '+' or '-' || char.IsAsciiDigit(At(j))) j++;
        var k = SkipBlanks(j);
        if (!(At(k) is '\n' or '\r' or '\0' or '#')) return false;
        var lineStart = Pos;
        while (lineStart > 0 && S[lineStart - 1] != '\n') lineStart--;
        var indent = 0;
        while (lineStart + indent < Pos && S[lineStart + indent] == ' ') indent++;
        _blockIndent = indent;
        Pos = j;
        return true;
    }

    private CodeTokenKind PlainScalar()
    {
        var i = Pos;
        var isKey = false;
        while (i < End)
        {
            var c = S[i];
            if (c is '\n' or '\r') break;
            if (c == ':' && (IsBreak(At(i + 1)) || (_flow > 0 && At(i + 1) is ',' or ']' or '}')))
            {
                isKey = true;
                break;
            }
            if (c == '#' && i > Pos && S[i - 1] is ' ' or '\t') break;
            if (_flow > 0 && c is ',' or ']' or '}' or '[' or '{') break;
            i++;
        }
        var e = i;
        while (e > Pos && S[e - 1] is ' ' or '\t') e--;
        if (e == Pos) e = Pos + 1;
        var text = Slice(Pos, e);
        Pos = e;
        if (isKey) return CodeTokenKind.Property;
        if (Constants.Contains(text)) return CodeTokenKind.Keyword;
        return IsNumber(text) ? CodeTokenKind.Number : CodeTokenKind.Plain;
    }

    private static bool IsNumber(ReadOnlySpan<char> text)
    {
        if (text.Length == 0) return false;
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return text.Length > 2 && text[2..].IndexOfAnyExcept("0123456789abcdefABCDEF") < 0;
        var i = text[0] is '-' or '+' ? 1 : 0;
        var digits = 0;
        var dot = false;
        for (; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsAsciiDigit(c)) digits++;
            else if (c == '.' && !dot) dot = true;
            else if ((c | 0x20) == 'e' && digits > 0 && i + 1 < text.Length) return text[(i + 1)..].TrimStart("+-").IndexOfAnyExceptInRange('0', '9') < 0 && text[(i + 1)..].TrimStart("+-").Length > 0;
            else return false;
        }
        return digits > 0;
    }
}
