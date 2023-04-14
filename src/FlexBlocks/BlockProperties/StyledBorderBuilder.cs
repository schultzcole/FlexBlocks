using System.Diagnostics;
using JetBrains.Annotations;
using static FlexBlocks.BlockProperties.Corner;
using static FlexBlocks.BlockProperties.LineStyle;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public enum Corner { TopLeft = '┌', TopRight = '┐', BottomRight = '┘', BottomLeft = '└' }

[PublicAPI]
public enum Edge { Horizontal = '─', Vertical = '│' }

[PublicAPI]
public enum Side { Top = '┬', Right = '┤', Bottom = '┴', Left = '├' }

[PublicAPI]
public enum LineStyle { None = -1, Thin, Thick, Dual }

[PublicAPI]
public class StyledBorderBuilder
{
    private const char DUAL_CORNER_START = '╒';
    private const char DUAL_THREE_WAY_START = '╞';

    private LineStyle _top = None;
    private LineStyle _right = None;
    private LineStyle _bottom = None;
    private LineStyle _left = None;
    private LineStyle _innerVert = None;
    private LineStyle _innerHoriz = None;

    public StyledBorderBuilder All(LineStyle style)
    {
        _top = style;
        _right = style;
        _bottom = style;
        _left = style;
        _innerVert = style;
        _innerHoriz = style;
        return this;
    }

    public StyledBorderBuilder Outer(LineStyle style)
    {
        _top = style;
        _right = style;
        _bottom = style;
        _left = style;
        return this;
    }

    public StyledBorderBuilder Inner(LineStyle style)
    {
        _innerVert = style;
        _innerHoriz = style;
        return this;
    }

    public StyledBorderBuilder Vertical(LineStyle style)
    {
        _right = style;
        _left = style;
        _innerVert = style;
        return this;
    }

    public StyledBorderBuilder Horizontal(LineStyle style)
    {
        _top = style;
        _bottom = style;
        _innerHoriz = style;
        return this;
    }

    public StyledBorderBuilder OuterVertical(LineStyle style)
    {
        _right = style;
        _left = style;
        return this;
    }

    public StyledBorderBuilder OuterHorizontal(LineStyle style)
    {
        _top = style;
        _bottom = style;
        return this;
    }

    public StyledBorderBuilder InnerVertical(LineStyle style)
    {
        _innerVert = style;
        return this;
    }

    public StyledBorderBuilder InnerHorizontal(LineStyle style)
    {
        _innerHoriz = style;
        return this;
    }

    public StyledBorderBuilder Top(LineStyle style)
    {
        _top = style;
        return this;
    }

    public StyledBorderBuilder Right(LineStyle style)
    {
        _right = style;
        return this;
    }

    public StyledBorderBuilder Bottom(LineStyle style)
    {
        _bottom = style;
        return this;
    }

    public StyledBorderBuilder Left(LineStyle style)
    {
        _left = style;
        return this;
    }

    public Border Build() => new(
        TopLeft:            StyleCorner(TopLeft, _left, _top),
        Top:                StyleEdge(Edge.Horizontal, _top),
        TopRight:           StyleCorner(TopRight, _right, _top),
        Right:              StyleEdge(Edge.Vertical, _right),
        BottomRight:        StyleCorner(BottomRight, _right, _bottom),
        Bottom:             StyleEdge(Edge.Horizontal, _bottom),
        BottomLeft:         StyleCorner(BottomLeft, _left, _bottom),
        Left:               StyleEdge(Edge.Vertical,   _left),
        InteriorVertical:   StyleEdge(Edge.Vertical,   _innerVert),
        InteriorHorizontal: StyleEdge(Edge.Horizontal, _innerHoriz),
        TopT:               StyleSideIntersection(Side.Top,    _innerVert, _top),
        RightT:             StyleSideIntersection(Side.Right,  _right,     _innerHoriz),
        BottomT:            StyleSideIntersection(Side.Bottom, _innerVert, _bottom),
        LeftT:              StyleSideIntersection(Side.Left,   _left,      _innerHoriz),
        InteriorJunction:   StyleCenterIntersection(_innerVert, _innerHoriz)
    );

    public static char? StyleCorner(Corner corner, LineStyle verticalStyle, LineStyle horizontalStyle)
    {
        switch ((verticalStyle, horizontalStyle))
        {
            case (None, None): return null;
            case (None, _):    return StyleEdge(Edge.Horizontal, horizontalStyle);
            case (_, None):    return StyleEdge(Edge.Vertical,   verticalStyle);
        }

        if (verticalStyle is not Dual && horizontalStyle is not Dual)
        {
            return (char)((int)corner + (2 * (int)verticalStyle) + (int)horizontalStyle);
        }

        int group = ((corner - TopLeft) / 4) * 3 + DUAL_CORNER_START;

        return (verticalStyle, horizontalStyle) switch
        {
            (Dual, Dual) => (char)(group + 2),
            (Dual, _)    => (char)(group + 1),
            (_, Dual)    => (char)group,
            _            => throw new UnreachableException("Case is covered by if statement above")
        };
    }

    public static char? StyleEdge(Edge edge, LineStyle style) => style switch
    {
        None  => null,
        Thin  => (char)(edge),
        Thick => (char)(edge + 1),
        Dual  => (edge is Edge.Horizontal ? '═' : '║'),
        _     => throw new ArgumentOutOfRangeException(nameof(style), style, null)
    };

    public static char? StyleCenterIntersection(LineStyle verticalStyle, LineStyle horizontalStyle) =>
        (verticalStyle, horizontalStyle) switch
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
                $"Either {nameof(verticalStyle)} ({verticalStyle}) or {nameof(horizontalStyle)} ({horizontalStyle}) are invalid"
            )
        };

    public static char? StyleSideIntersection(Side intersect, LineStyle verticalStyle, LineStyle horizontalStyle)
    {
        if (verticalStyle is None || horizontalStyle is None) return null;

        var group = ((intersect - Side.Left) / 8) * 3 + DUAL_THREE_WAY_START;

        return (verticalStyle, horizontalStyle) switch
        {
            (Thin, Thin)   => (char)intersect,
            (Thin, Thick)  => (char)(intersect + (intersect > Side.Right ? 3 : 1)),
            (Thick, Thin)  => (char)(intersect + 4),
            (Thick, Thick) => (char)(intersect + 7),
            (Dual, Dual)   => (char)(group + 2),
            (Dual, _)      => (char)(group + 1),
            (_, Dual)      => (char)group,
            _ => throw new InvalidOperationException(
                $"Either {nameof(verticalStyle)} ({verticalStyle}) or {nameof(horizontalStyle)} ({horizontalStyle}) are invalid"
            )
        };
    }
}
