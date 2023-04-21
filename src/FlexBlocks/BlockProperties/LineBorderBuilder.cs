using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public class LineBorderBuilder
{
    private LineStyle _top = LineStyle.None;
    private LineStyle _right = LineStyle.None;
    private LineStyle _bottom = LineStyle.None;
    private LineStyle _left = LineStyle.None;
    private LineStyle _innerVert = LineStyle.None;
    private LineStyle _innerHoriz = LineStyle.None;
    private LineStyle _accent = LineStyle.None;

    public LineBorder Build() => new(_top, _right, _bottom, _left, _innerVert, _innerHoriz, _accent);

    public LineBorderBuilder All(LineStyle style)
    {
        _top = style;
        _right = style;
        _bottom = style;
        _left = style;
        _innerVert = style;
        _innerHoriz = style;
        return this;
    }

    public LineBorderBuilder Outer(LineStyle style)
    {
        _top = style;
        _right = style;
        _bottom = style;
        _left = style;
        return this;
    }

    public LineBorderBuilder Inner(LineStyle style)
    {
        _innerVert = style;
        _innerHoriz = style;
        return this;
    }

    public LineBorderBuilder Vertical(LineStyle style)
    {
        _right = style;
        _left = style;
        _innerVert = style;
        return this;
    }

    public LineBorderBuilder Horizontal(LineStyle style)
    {
        _top = style;
        _bottom = style;
        _innerHoriz = style;
        return this;
    }

    public LineBorderBuilder OuterVertical(LineStyle style)
    {
        _right = style;
        _left = style;
        return this;
    }

    public LineBorderBuilder OuterHorizontal(LineStyle style)
    {
        _top = style;
        _bottom = style;
        return this;
    }

    public LineBorderBuilder InnerVertical(LineStyle style)
    {
        _innerVert = style;
        return this;
    }

    public LineBorderBuilder InnerHorizontal(LineStyle style)
    {
        _innerHoriz = style;
        return this;
    }

    public LineBorderBuilder Top(LineStyle style)
    {
        _top = style;
        return this;
    }

    public LineBorderBuilder Right(LineStyle style)
    {
        _right = style;
        return this;
    }

    public LineBorderBuilder Bottom(LineStyle style)
    {
        _bottom = style;
        return this;
    }

    public LineBorderBuilder Left(LineStyle style)
    {
        _left = style;
        return this;
    }

    public LineBorderBuilder Accent(LineStyle style)
    {
        _accent = style;
        return this;
    }
}
