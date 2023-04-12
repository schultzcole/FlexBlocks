using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

/// <summary>Defines a set of characters to use for each corner and side of a border.</summary>
[PublicAPI]
public record Border(
    char TopLeft,
    char Top,
    char TopRight,
    char Right,
    char BottomRight,
    char Bottom,
    char BottomLeft,
    char Left,
    char InteriorVertical,
    char InteriorHorizontal,
    char TopT,
    char RightT,
    char BottomT,
    char LeftT,
    char InteriorJunction
)
{
    /// <summary>Creates a new Border where every side and corner uses the same character.</summary>
    public Border(char all) : this(
        TopLeft: all,
        Top: all,
        TopRight: all,
        Right: all,
        BottomRight: all,
        Bottom: all,
        BottomLeft: all,
        Left: all,
        InteriorVertical: all,
        InteriorHorizontal: all,
        TopT: all,
        RightT: all,
        BottomT: all,
        LeftT: all,
        InteriorJunction: all
    ) { }
}
