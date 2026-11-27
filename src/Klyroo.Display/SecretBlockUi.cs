using Klyroo.UI.Theming;

namespace Klyroo.Display;

/// <summary>Per-instance class overrides of <see cref="KlySecretBlock"/> slots.</summary>
public sealed record SecretBlockUi : ISlotOverrides
{
    /// <summary>Classes merged into the <c>root</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Root { get; init; }
    /// <summary>Classes merged into the <c>header</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Header { get; init; }
    /// <summary>Classes merged into the <c>label</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Label { get; init; }
    /// <summary>Classes merged into the <c>actions</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Actions { get; init; }
    /// <summary>Classes merged into the <c>toggle</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Toggle { get; init; }
    /// <summary>Classes merged into the <c>toggleIcon</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? ToggleIcon { get; init; }
    /// <summary>Classes merged into the <c>copy</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Copy { get; init; }
    /// <summary>Classes merged into the <c>copyIcon</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? CopyIcon { get; init; }
    /// <summary>Classes merged into the <c>pre</c> slot of <see cref="KlySecretBlock"/>, after the theme and the app configuration.</summary>
    public string? Pre { get; init; }

    string? ISlotOverrides.For(string slot) => slot switch
    {
        "root" => Root,
        "header" => Header,
        "label" => Label,
        "actions" => Actions,
        "toggle" => Toggle,
        "toggleIcon" => ToggleIcon,
        "copy" => Copy,
        "copyIcon" => CopyIcon,
        "pre" => Pre,
        _ => null,
    };
}
