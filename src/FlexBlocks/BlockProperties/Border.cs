using CommunityToolkit.HighPerformance;
using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

/// <summary>Defines a set of characters to use for each corner and side of a border.</summary>
[PublicAPI]
public record Border(
    char? TopLeft,
    char? Top,
    char? TopRight,
    char? Right,
    char? BottomRight,
    char? Bottom,
    char? BottomLeft,
    char? Left,
    char? InteriorVertical,
    char? InteriorHorizontal,
    char? TopT,
    char? RightT,
    char? BottomT,
    char? LeftT,
    char? InteriorJunction
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

    public Padding ToPadding() => new(
        Top is null ? 0 : 1,
        Right is null ? 0 : 1,
        Bottom is null ? 0 : 1,
        Left is null ? 0 : 1
    );

    public void RenderOuter(Span2D<char> buffer)
    {
        var lastRow = buffer.Height - 1;
        var lastCol = buffer.Width - 1;

        if (TopLeft is { } tl) buffer[0, 0] = tl;
        if (TopRight is { } tr) buffer[0, lastCol] = tr;
        if (BottomLeft is { } bl) buffer[lastRow, 0] = bl;
        if (BottomRight is { } br) buffer[lastRow, lastCol] = br;

        if (Top is { } t) buffer.Slice(0,          1,       1,           lastCol - 1).Fill(t);
        if (Bottom is { } b) buffer.Slice(lastRow, 1,       1,           lastCol - 1).Fill(b);
        if (Left is { } l) buffer.Slice(1,         0,       lastRow - 1, 1).Fill(l);
        if (Right is { } r) buffer.Slice(1,        lastCol, lastRow - 1, 1).Fill(r);
    }
}
