using CommunityToolkit.HighPerformance;
using JetBrains.Annotations;
using static FlexBlocks.BlockProperties.BorderAccent;
using static FlexBlocks.BlockProperties.BorderOuterCorner;
using static FlexBlocks.BlockProperties.BorderOuterEdge;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public enum BorderOuterEdge { Top, Right, Bottom, Left }

[PublicAPI]
public enum BorderOuterCorner { TopLeft, TopRight, BottomRight, BottomLeft }

[PublicAPI]
public enum BorderInnerEdge { Vertical, Horizontal }

[PublicAPI]
public enum BorderAccent { Normal, Accent }

[PublicAPI]
public interface IBorder
{
    char? OuterEdge(BorderOuterEdge edge, BorderAccent accent = Normal);
    char? OuterCorner(BorderOuterCorner corner, BorderAccent verticalAccent = Normal, BorderAccent horizontalAccent = Normal);
    char? OuterJunction(BorderOuterEdge edge, BorderAccent outerAccent = Normal, BorderAccent innerAccent = Normal);
    char? InnerEdge(BorderInnerEdge edge, BorderAccent accent = Normal);
    char? InnerJunction(BorderAccent verticalAccent = Normal, BorderAccent horizontalAccent = Normal);
}

[PublicAPI]
public static class BorderExtensions
{
    public static (BorderOuterEdge verticalEdge, BorderOuterEdge horizontalEdge) ToEdgePair(
        this BorderOuterCorner corner
    )
    {
        return corner switch
        {
            TopLeft     => (Left, Top),
            TopRight    => (Right, Top),
            BottomRight => (Right, Bottom),
            BottomLeft  => (Left, Bottom),
            _                             => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
        };
    }

    public static BorderInnerEdge IntersectingInnerEdge(
        this BorderOuterEdge edge
    )
    {
        return edge switch
        {
            Top or Bottom => BorderInnerEdge.Vertical,
            Right or Left => BorderInnerEdge.Horizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };
    }

    public static Padding ToPadding(this IBorder border)
    {
        return new Padding(
            Top: border.OuterEdge(Top) is not null ? 1 : 0,
            Right: border.OuterEdge(Right) is not null ? 1 : 0,
            Bottom: border.OuterEdge(Bottom) is not null ? 1 : 0,
            Left: border.OuterEdge(Left) is not null ? 1 : 0
        );
    }

    // public static void RenderOuter(this IBorder border, Span2D<char> buffer)
    // {
    //     var lastRow = buffer.Height - 1;
    //     var lastCol = buffer.Width - 1;
    //
    //     if (border.OuterCorner(TopLeft) is { } tl) buffer[0, 0] = tl;
    //     if (border.OuterCorner(TopRight) is { } tr) buffer[0, lastCol] = tr;
    //     if (border.OuterCorner(BottomLeft) is { } bl) buffer[lastRow, 0] = bl;
    //     if (border.OuterCorner(BottomRight) is { } br) buffer[lastRow, lastCol] = br;
    //
    //     if (border.OuterEdge(Top) is { } t)    buffer.Slice(0,       1,       1,           lastCol - 1).Fill(t);
    //     if (border.OuterEdge(Bottom) is { } b) buffer.Slice(lastRow, 1,       1,           lastCol - 1).Fill(b);
    //     if (border.OuterEdge(Left) is { } l)   buffer.Slice(1,       0,       lastRow - 1, 1          ).Fill(l);
    //     if (border.OuterEdge(Right) is { } r)  buffer.Slice(1,       lastCol, lastRow - 1, 1          ).Fill(r);
    // }
}
