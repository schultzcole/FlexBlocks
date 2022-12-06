using CommunityToolkit.HighPerformance;

namespace CharConsole.Blocks;

public class BorderBlock : SizedBlock
{
    /// The type of border to render for this block
    public BorderType BorderType { get; set; }

    /// What padding, if any should exist between the border and the content
    public Padding? Padding { get; set; }

    public Padding EffectivePadding =>
        (BorderType == BorderType.None)
            ? Padding?.Copy() ?? Padding.Zero
            : Padding?.Expand(1) ?? Padding.One;

    protected override BlockSize CalcMaxContentSize(BlockSize maxSize) => CalcMaxContentSize(maxSize, EffectivePadding);

    private static BlockSize CalcMaxContentSize(BlockSize maxSize, Padding padding) =>
        new(
            maxSize.Width - padding.Left - padding.Right,
            maxSize.Height - padding.Top - padding.Bottom
        );

    public override void Render(Span2D<char> buffer)
    {
        RenderBorder(buffer);

        if (Content is null) return;

        var padding = EffectivePadding;
        var maxContentSize = CalcMaxContentSize(buffer.GetSize(), padding);

        var contentBuffer = buffer.Slice(
            row: padding.Top,
            column: padding.Left,
            height: maxContentSize.Height,
            width: maxContentSize.Width
        );

        RenderContent(contentBuffer);
    }

    private void RenderBorder(Span2D<char> buffer)
    {
        if (BorderType == BorderType.None) return;

        var lastRow = buffer.Height - 1;
        var lastCol = buffer.Width - 1;

        buffer[0, 0] = BorderType.TopLeft();
        buffer[0, lastCol] = BorderType.TopRight();
        buffer[lastRow, 0] = BorderType.BottomLeft();
        buffer[lastRow, lastCol] = BorderType.BottomRight();

        var t = BorderType.Top();
        var b = BorderType.Bottom();
        var l = BorderType.Left();
        var r = BorderType.Right();

        for (int col = 1; col < lastCol; col++)
        {
            buffer[0, col] = t;
            buffer[lastRow, col] = b;
        }

        for (int row = 1; row < lastRow; row++)
        {
            buffer[row, 0] = l;
            buffer[row, lastCol] = r;
        }
    }
}
