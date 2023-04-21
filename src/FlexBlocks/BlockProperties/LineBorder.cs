using System.Diagnostics;
using JetBrains.Annotations;
using static FlexBlocks.BlockProperties.LineStyle;

namespace FlexBlocks.BlockProperties;

public enum LineCorner { TopLeft = '┌', TopRight = '┐', BottomRight = '┘', BottomLeft = '└' }
public enum LineEdge { Horizontal = '─', Vertical = '│' }
public enum LineSideJunction { Top = '┬', Right = '┤', Bottom = '┴', Left = '├' }
public enum LineStyle { None = -1, Thin, Thick, Dual }

[PublicAPI]
public class LineBorder : IBorder
{

    private const char DUAL_CORNER_START = '╒';
    private const char DUAL_THREE_WAY_START = '╞';

    private readonly LineStyle _top;
    private readonly LineStyle _right;
    private readonly LineStyle _bottom;
    private readonly LineStyle _left;
    private readonly LineStyle _innerVert;
    private readonly LineStyle _innerHoriz;
    private readonly LineStyle _accent;

    internal LineBorder(
        LineStyle top,
        LineStyle right,
        LineStyle bottom,
        LineStyle left,
        LineStyle innerVert,
        LineStyle innerHoriz,
        LineStyle accent
    )
    {
        _top = top;
        _right = right;
        _bottom = bottom;
        _left = left;
        _innerVert = innerVert;
        _innerHoriz = innerHoriz;
        _accent = accent;
    }

    /// <inheritdoc />
    public char? OuterEdge(BorderOuterEdge edge, BorderAccent accent)
    {
        var style = AccentOrDefault(accent, StyleForBorderEdge(edge));
        var lineEdge = edge switch
        {
            BorderOuterEdge.Top or BorderOuterEdge.Bottom => LineEdge.Horizontal,
            BorderOuterEdge.Right or BorderOuterEdge.Left => LineEdge.Vertical,
            _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };
        return StyleEdge(lineEdge, style);
    }

    /// <inheritdoc />
    public char? OuterCorner(BorderOuterCorner corner, BorderAccent verticalAccent, BorderAccent horizontalAccent)
    {
        var (vertical, horizontal) = corner.ToEdgePair();
        var verticalStyle = AccentOrDefault(verticalAccent, StyleForBorderEdge(vertical));
        var horizontalStyle = AccentOrDefault(horizontalAccent, StyleForBorderEdge(horizontal));

        switch ((verticalStyle, horizontalStyle))
        {
            case (None, None): return null;
            case (None, _):    return StyleEdge(LineEdge.Horizontal, horizontalStyle);
            case (_, None):    return StyleEdge(LineEdge.Vertical,   verticalStyle);
        }

        var lineCorner = corner switch {
            BorderOuterCorner.TopLeft     => LineCorner.TopLeft,
            BorderOuterCorner.TopRight    => LineCorner.TopRight,
            BorderOuterCorner.BottomRight => LineCorner.BottomRight,
            BorderOuterCorner.BottomLeft  => LineCorner.BottomLeft,
            _                             => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
        };

        if (verticalStyle is not Dual && horizontalStyle is not Dual)
        {
            return (char)((int)lineCorner + (2 * (int)verticalStyle) + (int)horizontalStyle);
        }

        int group = ((lineCorner - LineCorner.TopLeft) / 4) * 3 + DUAL_CORNER_START;

        return (verticalStyle, horizontalStyle) switch
        {
            (Dual, Dual) => (char)(group + 2),
            (Dual, _)    => (char)(group + 1),
            (_, Dual)    => (char)group,
            _            => throw new UnreachableException("Case is covered by if statement above")
        };
    }

    /// <inheritdoc />
    public char? OuterJunction(BorderOuterEdge edge, BorderAccent outerAccent, BorderAccent innerAccent)
    {
        var outerStyle = AccentOrDefault(outerAccent, StyleForBorderEdge(edge));
        var innerStyle = AccentOrDefault(innerAccent, StyleForBorderInnerEdge(edge.IntersectingInnerEdge()));

        if (outerStyle is None || innerStyle is None) return null;

        var (vStyle, hStyle) = edge switch
        {
            BorderOuterEdge.Top or BorderOuterEdge.Bottom => (innerStyle, outerStyle),
            BorderOuterEdge.Right or BorderOuterEdge.Left => (outerStyle, innerStyle),
            _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };

        LineSideJunction intersect = edge switch
        {
            BorderOuterEdge.Top    => LineSideJunction.Top,
            BorderOuterEdge.Right  => LineSideJunction.Right,
            BorderOuterEdge.Bottom => LineSideJunction.Bottom,
            BorderOuterEdge.Left   => LineSideJunction.Left,
            _                      => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };

        var group = ((intersect - LineSideJunction.Left) / 8) * 3 + DUAL_THREE_WAY_START;

        return (vStyle, hStyle) switch
        {
            (Thin, Thin)   => (char)intersect,
            (Thin, Thick)  => (char)(intersect + (intersect > LineSideJunction.Right ? 3 : 1)),
            (Thick, Thin)  => (char)(intersect + 4),
            (Thick, Thick) => (char)(intersect + 7),
            (Dual, Dual)   => (char)(group + 2),
            (Dual, _)      => (char)(group + 1),
            (_, Dual)      => (char)group,
            _ => throw new InvalidOperationException(
                $"Either {nameof(vStyle)} ({vStyle}) or {nameof(hStyle)} ({hStyle}) are invalid"
            )
        };
    }

    /// <inheritdoc />
    public char? InnerEdge(BorderInnerEdge edge, BorderAccent accent)
    {
        var style = AccentOrDefault(accent, StyleForBorderInnerEdge(edge));
        var lineEdge = edge switch
        {
            BorderInnerEdge.Vertical   => LineEdge.Vertical,
            BorderInnerEdge.Horizontal => LineEdge.Horizontal,
            _                          => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };
        return StyleEdge(lineEdge, style);
    }

    /// <inheritdoc />
    public char? InnerJunction(BorderAccent verticalAccent, BorderAccent horizontalAccent)
    {
        var vStyle = AccentOrDefault(verticalAccent, _innerVert);
        var hStyle = AccentOrDefault(horizontalAccent, _innerHoriz);
        return (vStyle, hStyle) switch
        {
            (None, _)      => null,
            (_, None)      => null,
            (Thin, Thin)   => '┼',
            (Thin, Thick)  => '┿',
            (Thick, Thin)  => '╂',
            (Thick, Thick) => '╋',
            (Dual, Dual)   => '╬',
            // there are no double/thick combos in the spec so just fall back to thin for those
            (Dual, _) => '╫',
            (_, Dual) => '╪',
            _ => throw new InvalidOperationException(
                $"Either {nameof(vStyle)} ({vStyle}) or {nameof(hStyle)} ({hStyle}) are invalid"
            )
        };
    }

    private static char? StyleEdge(LineEdge edge, LineStyle style)
    {
        return style switch
        {
            None  => null,
            Thin  => (char)(edge),
            Thick => (char)(edge + 1),
            Dual  => (edge is LineEdge.Horizontal ? '═' : '║'),
            _     => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    private LineStyle StyleForBorderEdge(BorderOuterEdge edge)
    {
        return edge switch
        {
            BorderOuterEdge.Top    => _top,
            BorderOuterEdge.Right  => _right,
            BorderOuterEdge.Bottom => _bottom,
            BorderOuterEdge.Left   => _left,
            _                      => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };
    }

    private LineStyle StyleForBorderInnerEdge(BorderInnerEdge edge)
    {
        return edge switch
        {
            BorderInnerEdge.Vertical   => _innerVert,
            BorderInnerEdge.Horizontal => _innerHoriz,
            _                          => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };
    }

    private LineStyle AccentOrDefault(BorderAccent accent, LineStyle def)
    {
        return accent switch
        {
            BorderAccent.Normal => def,
            BorderAccent.Accent => _accent,
            _                   => throw new ArgumentOutOfRangeException(nameof(accent), accent, null)
        };
    }
}
