using CommunityToolkit.HighPerformance;
using static FlexBlocks.BlockProperties.BorderAccent;
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


    public static void RenderInner(
        IBorder border,
        Span2D<char> buffer,
        Span<(int index, BorderAccent accent)> rowBorders,
        Span<(int index, BorderAccent accent)> colBorders
    )
    {
        RenderInnerLines(border, buffer, Vertical,   colBorders);
        RenderInnerLines(border, buffer, Horizontal, rowBorders);

        RenderOuterJunctions(border, buffer, Top,    colBorders);
        RenderOuterJunctions(border, buffer, Bottom, colBorders);
        RenderOuterJunctions(border, buffer, Left,   rowBorders);
        RenderOuterJunctions(border, buffer, Right,  rowBorders);

        RenderInnerJunctions(border, buffer, rowBorders, colBorders);
    }

    private static void RenderInnerLines(
        IBorder border,
        Span2D<char> buffer,
        BorderInnerEdge edge,
        Span<(int index, BorderAccent accent)> indices
    )
    {
        var maybeLine = border.InnerEdge(edge);
        var maybeAccentLine = border.InnerEdge(edge, Accent);

        if (maybeLine is null && maybeAccentLine is null) return;

        foreach (var (index, accent) in indices)
        {
            var columnSlice = edge switch
            {
                Vertical   => buffer.Slice(1, index, buffer.Height - 2, 1),
                Horizontal => buffer.Slice(index, 1, 1, buffer.Width - 2),
                _          => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
            };

            if (
                accent switch
                {
                    Normal => maybeLine,
                    Accent => maybeAccentLine,
                    _                   => null
                } is { } c
            )
            {
                columnSlice.Fill(c);
            }
        }
    }

    private static void RenderOuterJunctions(
        IBorder border,
        Span2D<char> buffer,
        BorderOuterEdge edge,
        Span<(int index, BorderAccent accent)> indices
    )
    {
        var maybeJunction = border.OuterJunction(edge);
        var maybeAccentJunction = border.OuterJunction(edge, Normal, Accent);

        if (maybeJunction is null && maybeAccentJunction is null) return;

        var lastCol = buffer.Width - 1;
        var lastRow = buffer.Height - 1;
        foreach (var (index, accent) in indices)
        {
            var junction = accent switch
            {
                Normal => maybeJunction,
                Accent => maybeAccentJunction,
                _                   => null
            };
            if (junction is not { } j) continue;

            var loc = edge switch
            {
                Top    => buffer.Slice(0, index, 1, 1),
                Right  => buffer.Slice(index, lastCol, 1, 1),
                Bottom => buffer.Slice(lastRow, index, 1, 1),
                Left   => buffer.Slice(index, 0, 1, 1),
                _      => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
            };

            loc.Fill(j);
        }
    }

    private static void RenderInnerJunctions(
        IBorder border,
        Span2D<char> buffer,
        Span<(int index, BorderAccent accent)> rowBorders,
        Span<(int index, BorderAccent accent)> colBorders
    )
    {
        var maybeJunction            = border.InnerJunction(Normal, Normal);
        var maybeVertAccentJunction  = border.InnerJunction(Accent, Normal);
        var maybeHorizAccentJunction = border.InnerJunction(Normal, Accent);
        var maybeAccentJunction      = border.InnerJunction(Accent, Accent);

        if (
            maybeJunction is null &&
            maybeVertAccentJunction is null &&
            maybeHorizAccentJunction is null &&
            maybeAccentJunction is null
        )
        {
            return;
        }

        foreach (var (row, horizAccent) in rowBorders)
        foreach (var (col, vertAccent) in colBorders)
        {
            if (
                (vertAccent, horizAccent) switch
                {
                    (Normal, Normal) => maybeJunction,
                    (Accent, Normal) => maybeVertAccentJunction,
                    (Normal, Accent) => maybeHorizAccentJunction,
                    (Accent, Accent) => maybeAccentJunction,
                    _                => null
                } is { } j
            )
            {
                buffer[row, col] = j;
            }
        }
    }
}
