using CommunityToolkit.HighPerformance;
using static FlexBlocks.BlockProperties.BorderInnerEdge;
using static FlexBlocks.BlockProperties.BorderOuterCorner;
using static FlexBlocks.BlockProperties.BorderOuterEdge;

namespace FlexBlocks.BlockProperties;

public static class BorderRenderHelper
{
    public static void RenderOuter(IBorder border, Span2D<char> buffer)
    {
        var lastRow = buffer.Height - 1;
        var lastCol = buffer.Width - 1;

        if (border.OuterCorner(TopLeft) is { } tl) buffer[0, 0] = tl;
        if (border.OuterCorner(TopRight) is { } tr) buffer[0, lastCol] = tr;
        if (border.OuterCorner(BottomLeft) is { } bl) buffer[lastRow, 0] = bl;
        if (border.OuterCorner(BottomRight) is { } br) buffer[lastRow, lastCol] = br;

        if (border.OuterEdge(Top) is { } t) buffer.Slice(0,          1,       1, lastCol - 1).Fill(t);
        if (border.OuterEdge(Bottom) is { } b) buffer.Slice(lastRow, 1,       1, lastCol - 1).Fill(b);
        if (border.OuterEdge(Left) is { } l) buffer.Slice(1,         0,       lastRow - 1, 1).Fill(l);
        if (border.OuterEdge(Right) is { } r) buffer.Slice(1,        lastCol, lastRow - 1, 1).Fill(r);
    }


    public static void RenderInner(IBorder border, Span2D<char> buffer, Span<int> rowGaps, Span<int> colGaps)
    {
        var maybeVert = border.InnerEdge(Vertical);
        var maybeHoriz = border.InnerEdge(Horizontal);

        if (maybeVert is null && maybeHoriz is null) return;

        if (maybeVert is { } vert)
        {
            foreach (var col in colGaps)
            {
                buffer.Slice(1, col, buffer.Height - 2, 1).Fill(vert);
            }
        }

        if (maybeHoriz is { } horiz)
        {
            foreach (var row in rowGaps)
            {
                buffer.Slice(row, 1, 1, buffer.Width - 2).Fill(horiz);
            }
        }

        RenderOuterJunction(border, Top, buffer, colGaps);
        RenderOuterJunction(border, Bottom, buffer, colGaps);
        RenderOuterJunction(border, Left, buffer, rowGaps);
        RenderOuterJunction(border, Right, buffer, rowGaps);

        if (border.InnerJunction() is { } junction)
        {
            foreach (var row in rowGaps)
            foreach (var col in colGaps)
            {
                buffer[row, col] = junction;
            }
        }
    }

    private static void RenderOuterJunction(IBorder border, BorderOuterEdge edge, Span2D<char> buffer, Span<int> indices)
    {
        if (border.OuterJunction(edge) is not {} junction) return;

        switch (edge)
        {
            case Top:
                foreach (var index in indices) buffer[0, index] = junction;
                break;
            case Right:
                var lastCol = buffer.Width - 1;
                foreach (var index in indices) buffer[index, lastCol] = junction;
                break;
            case Bottom:
                var lastRow = buffer.Height - 1;
                foreach (var index in indices) buffer[lastRow, index] = junction;
                break;
            case Left:
                foreach (var index in indices) buffer[index, 0] = junction;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(edge), edge, null);
        }
    }
}
