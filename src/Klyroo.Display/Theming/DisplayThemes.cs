using Klyroo.UI.Theming;

namespace Klyroo.Display.Theming;

/// <summary>Default themes of Klyroo.Display. Every class is a literal, so the class extractor and Tailwind see it.</summary>
internal static class DisplayThemes
{
    private const string Panel = "flex w-full min-w-0 flex-col gap-2 rounded-(--klyroo-radius) border border-(--klyroo-border) bg-(--klyroo-bg) p-4 text-sm text-(--klyroo-text)";
    private const string Header = "flex items-center justify-between gap-2";
    private const string Label = "text-xs font-semibold uppercase text-(--klyroo-text-muted)";
    private const string Actions = "flex items-center gap-1";
    private const string ActionButton = "inline-flex items-center gap-1 rounded-(--klyroo-radius) px-2 py-1 text-xs font-medium text-(--klyroo-text-muted) hover:bg-(--klyroo-bg-muted) hover:text-(--klyroo-text)";
    private const string Pre = "m-0 overflow-x-auto rounded-(--klyroo-radius) bg-(--klyroo-bg-muted) p-3 font-mono text-xs text-(--klyroo-text-highlighted)";

    public static ComponentTheme CodeBlock() =>
        new ComponentTheme("codeBlock", "root", "header", "label", "actions", "copy", "copyIcon", "pre",
                "line", "lineIdle", "lineHighlighted", "lineNumbered",
                "tokenKeyword", "tokenString", "tokenNumber", "tokenComment", "tokenType", "tokenFunction", "tokenProperty", "tokenTag", "tokenPunctuation")
            .Slot("root", Panel)
            .Slot("header", Header)
            .Slot("label", Label)
            .Slot("actions", Actions)
            .Slot("copy", ActionButton)
            .Slot("copyIcon", "size-3.5")
            .Slot("pre", Pre)
            // A line bleeds over the padding of the block so its fill and its gutter marker reach the edges; the 2px border plus the start padding equal that padding.
            .Slot("line", "-mx-3 block w-max min-w-[calc(100%+1.5rem)] border-s-2 ps-2.5 pe-3")
            .Slot("lineIdle", "border-transparent")
            .Slot("lineHighlighted", "border-(--klyroo-primary) bg-(--klyroo-bg-accented)")
            .Slot("lineNumbered", "before:me-4 before:inline-block before:w-[3ch] before:select-none before:text-end before:text-(--klyroo-text-dimmed) before:content-[attr(data-line)]")
            .Slot("tokenKeyword", CodeRendering.TokenClass(CodeTokenKind.Keyword)!)
            .Slot("tokenString", CodeRendering.TokenClass(CodeTokenKind.String)!)
            .Slot("tokenNumber", CodeRendering.TokenClass(CodeTokenKind.Number)!)
            .Slot("tokenComment", CodeRendering.TokenClass(CodeTokenKind.Comment)!)
            .Slot("tokenType", CodeRendering.TokenClass(CodeTokenKind.Type)!)
            .Slot("tokenFunction", CodeRendering.TokenClass(CodeTokenKind.Function)!)
            .Slot("tokenProperty", CodeRendering.TokenClass(CodeTokenKind.Property)!)
            .Slot("tokenTag", CodeRendering.TokenClass(CodeTokenKind.Tag)!)
            .Slot("tokenPunctuation", CodeRendering.TokenClass(CodeTokenKind.Punctuation)!);

    public static ComponentTheme SecretBlock() =>
        new ComponentTheme("secretBlock", "root", "header", "label", "actions", "toggle", "toggleIcon", "copy", "copyIcon", "pre")
            .Slot("root", Panel)
            .Slot("header", Header)
            .Slot("label", Label)
            .Slot("actions", Actions)
            .Slot("toggle", ActionButton)
            .Slot("toggleIcon", "size-3.5")
            .Slot("copy", ActionButton)
            .Slot("copyIcon", "size-3.5")
            .Slot("pre", Pre);

    public static ComponentTheme FileList() =>
        new ComponentTheme("fileList", "root", "title", "list", "item", "main", "badge", "badgeIdle", "badgeTone", "badgeIcon", "body", "name", "description", "meta", "more", "moreIcon")
            .Slot("root", "flex w-full min-w-0 flex-col gap-2 text-sm text-(--klyroo-text)")
            .Slot("title", "m-0 font-semibold text-(--klyroo-text-highlighted)")
            .Slot("list", "m-0 flex list-none flex-col gap-2 p-0")
            .Slot("item", "flex items-center justify-between gap-3")
            .Slot("main", "flex min-w-0 items-center gap-2.5")
            .Slot("badge", "inline-flex h-6 min-w-8 shrink-0 items-center justify-center rounded-(--klyroo-radius) px-1.5 text-[10px] font-bold uppercase")
            .Slot("badgeIdle", "bg-(--klyroo-bg-muted) text-(--klyroo-text-highlighted)")
            .Slot("badgeTone", "bg-(--kly-color)/10 text-(--kly-color-text)")
            .Slot("badgeIcon", "size-3.5")
            .Slot("body", "flex min-w-0 flex-col")
            .Slot("name", "truncate font-medium text-(--klyroo-text-highlighted) hover:underline")
            .Slot("description", "text-xs text-(--klyroo-text-muted)")
            .Slot("meta", "shrink-0 text-xs text-(--klyroo-text-muted)")
            .Slot("more", "inline-flex items-center gap-1.5 self-start rounded-(--klyroo-radius) px-2 py-1 text-xs font-medium text-(--klyroo-text-muted) hover:bg-(--klyroo-bg-muted) hover:text-(--klyroo-text) focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--klyroo-primary)")
            .Slot("moreIcon", "size-3.5");
}
