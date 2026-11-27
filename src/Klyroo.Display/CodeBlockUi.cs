using Klyroo.UI.Theming;

namespace Klyroo.Display;

/// <summary>Per-instance class overrides of <see cref="KlyCodeBlock"/> slots.</summary>
public sealed record CodeBlockUi : ISlotOverrides
{
    /// <summary>Classes merged into the <c>root</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Root { get; init; }
    /// <summary>Classes merged into the <c>header</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Header { get; init; }
    /// <summary>Classes merged into the <c>label</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Label { get; init; }
    /// <summary>Classes merged into the <c>actions</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Actions { get; init; }
    /// <summary>Classes merged into the <c>copy</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Copy { get; init; }
    /// <summary>Classes merged into the <c>copyIcon</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? CopyIcon { get; init; }
    /// <summary>Classes merged into the <c>pre</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Pre { get; init; }
    /// <summary>Classes merged into the <c>line</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? Line { get; init; }
    /// <summary>Classes merged into the <c>lineIdle</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? LineIdle { get; init; }
    /// <summary>Classes merged into the <c>lineHighlighted</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? LineHighlighted { get; init; }
    /// <summary>Classes merged into the <c>lineNumbered</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? LineNumbered { get; init; }
    /// <summary>Classes merged into the <c>tokenKeyword</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenKeyword { get; init; }
    /// <summary>Classes merged into the <c>tokenString</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenString { get; init; }
    /// <summary>Classes merged into the <c>tokenNumber</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenNumber { get; init; }
    /// <summary>Classes merged into the <c>tokenComment</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenComment { get; init; }
    /// <summary>Classes merged into the <c>tokenType</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenType { get; init; }
    /// <summary>Classes merged into the <c>tokenFunction</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenFunction { get; init; }
    /// <summary>Classes merged into the <c>tokenProperty</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenProperty { get; init; }
    /// <summary>Classes merged into the <c>tokenTag</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenTag { get; init; }
    /// <summary>Classes merged into the <c>tokenPunctuation</c> slot of <see cref="KlyCodeBlock"/>, after the theme and the app configuration.</summary>
    public string? TokenPunctuation { get; init; }

    string? ISlotOverrides.For(string slot) => slot switch
    {
        "root" => Root,
        "header" => Header,
        "label" => Label,
        "actions" => Actions,
        "copy" => Copy,
        "copyIcon" => CopyIcon,
        "pre" => Pre,
        "line" => Line,
        "lineIdle" => LineIdle,
        "lineHighlighted" => LineHighlighted,
        "lineNumbered" => LineNumbered,
        "tokenKeyword" => TokenKeyword,
        "tokenString" => TokenString,
        "tokenNumber" => TokenNumber,
        "tokenComment" => TokenComment,
        "tokenType" => TokenType,
        "tokenFunction" => TokenFunction,
        "tokenProperty" => TokenProperty,
        "tokenTag" => TokenTag,
        "tokenPunctuation" => TokenPunctuation,
        _ => null,
    };
}
