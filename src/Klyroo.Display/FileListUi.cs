using Klyroo.UI.Theming;

namespace Klyroo.Display;

/// <summary>Per-instance class overrides of <see cref="KlyFileList"/> slots.</summary>
public sealed record FileListUi : ISlotOverrides
{
    /// <summary>Classes merged into the <c>root</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Root { get; init; }
    /// <summary>Classes merged into the <c>title</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Title { get; init; }
    /// <summary>Classes merged into the <c>list</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? List { get; init; }
    /// <summary>Classes merged into the <c>item</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Item { get; init; }
    /// <summary>Classes merged into the <c>main</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Main { get; init; }
    /// <summary>Classes merged into the <c>badge</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Badge { get; init; }
    /// <summary>Classes merged into the <c>body</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Body { get; init; }
    /// <summary>Classes merged into the <c>name</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Name { get; init; }
    /// <summary>Classes merged into the <c>description</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Description { get; init; }
    /// <summary>Classes merged into the <c>meta</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? Meta { get; init; }
    /// <summary>Classes merged into the <c>more</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? More { get; init; }
    /// <summary>Classes merged into the <c>moreIcon</c> slot of <see cref="KlyFileList"/>, after the theme and the app configuration.</summary>
    public string? MoreIcon { get; init; }

    string? ISlotOverrides.For(string slot) => slot switch
    {
        "root" => Root,
        "title" => Title,
        "list" => List,
        "item" => Item,
        "main" => Main,
        "badge" => Badge,
        "body" => Body,
        "name" => Name,
        "description" => Description,
        "meta" => Meta,
        "more" => More,
        "moreIcon" => MoreIcon,
        _ => null,
    };
}
