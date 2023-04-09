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
    char Left)
{
    /// <summary>Creates a new Border where every side and corner uses the same character.</summary>
    [PublicAPI]
    public Border(char all)
        : this(all, all, all, all, all, all, all, all) { }

    /// <summary>Creates a new Border where the corners are blank and the horizontal and vertical sides
    /// use the given characters.</summary>
    [PublicAPI]
    public Border(char horizontal, char vertical)
        : this(' ', horizontal, ' ', vertical, ' ', horizontal, ' ', vertical) { }

    /// <summary>Creates a new Border where each corner has a unique character and the horizontal and vertical sides
    /// use the given characters.</summary>
    [PublicAPI]
    public Border(char topLeft, char topRight, char bottomRight, char bottomLeft, char horizontal, char vertical)
        : this(topLeft, horizontal, topRight, vertical, bottomRight, horizontal, bottomLeft, vertical) { }
}
