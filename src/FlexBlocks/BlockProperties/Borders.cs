using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public static class Borders
{
    /// <summary>Renders all sides and corners as a filled block.</summary>
    public static Border Block { get; } = new('█');

    /// <summary>Renders a border with straight edges and sharp, square corners.</summary>
    /// <example><code>
    /// ┌──┬┐
    /// │  ││
    /// ├──┼┤
    /// └──┴┘
    /// </code></example>
    public static Border Square { get; } = new(
        TopLeft: '┌',
        Top: '─',
        TopRight: '┐',
        Right: '│',
        BottomRight: '┘',
        Bottom: '─',
        BottomLeft: '└',
        Left: '│',
        InteriorVertical: '│',
        InteriorHorizontal: '─',
        TopT: '┬',
        RightT: '┤',
        BottomT: '┴',
        LeftT: '├',
        InteriorJunction: '┼'
    );

    /// <summary>Renders a border with straight edges and rounded corners.</summary>
    /// <example><code>
    /// ╭──┬╮
    /// │  ││
    /// ├──┼┤
    /// ╰──┴╯
    /// </code></example>
    public static Border Rounded { get; } = new(
        TopLeft: '╭',
        Top: '─',
        TopRight: '╮',
        Right: '│',
        BottomRight: '╯',
        Bottom: '─',
        BottomLeft: '╰',
        Left: '│',
        InteriorVertical: '│',
        InteriorHorizontal: '─',
        TopT: '┬',
        RightT: '┤',
        BottomT: '┴',
        LeftT: '├',
        InteriorJunction: '┼'
    );

    /// <summary>Creates a new StyledBorderBuilder</summary>
    public static StyledBorderBuilder Builder() => new();
}
