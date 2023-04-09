using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

/// <summary>A <see cref="ContentBlock"/> that optionally includes a border and padding around its content.</summary>
public class BorderBlock : ContentBlock
{
    /// <summary>The type of border to render for this block</summary>
    public Border? Border { get; set; }

    /// <summary>What padding, if any should exist between the border and the content</summary>
    public Padding? Padding { get; set; }

    /// <summary>The total amount of padding around this block's content, including both border and padding.</summary>
    public Padding EffectivePadding =>
        (Border, Padding) switch
        {
            (null,     null    ) => Padding.Zero,
            (null,     not null) => Padding,
            (not null, null    ) => Padding.One,
            (not null, not null) => Padding.Expand(1)
        };

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize()
    {
        var contentMaxSize = base.CalcMaxSize();

        var padding = EffectivePadding;
        return UnboundedBlockSize.From(
            contentMaxSize.Width + padding.Left + padding.Right,
            contentMaxSize.Height + padding.Top + padding.Bottom
        );
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        var padding = EffectivePadding;
        var maxContentSize = maxSize.ShrinkByPadding(padding);
        var contentDesiredSize = base.CalcSize(maxContentSize).Constrain(maxContentSize);

        return BlockSize.From(
            contentDesiredSize.Width + padding.Left + padding.Right,
            contentDesiredSize.Height + padding.Top + padding.Bottom
        );
    }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        RenderBorder(buffer);

        if (Content is null) return;

        var padding = EffectivePadding;
        var maxContentSize = buffer.BlockSize().ShrinkByPadding(padding);

        var contentBuffer = buffer.Slice(
            row: padding.Top,
            column: padding.Left,
            height: maxContentSize.Height,
            width: maxContentSize.Width
        );

        RenderContent(contentBuffer);
    }

    /// <summary>Renders the border to the buffer.</summary>
    private void RenderBorder(Span2D<char> buffer)
    {
        if (Border is null) return;

        var lastRow = buffer.Height - 1;
        var lastCol = buffer.Width - 1;

        buffer[0, 0] = Border.TopLeft;
        buffer[0, lastCol] = Border.TopRight;
        buffer[lastRow, 0] = Border.BottomLeft;
        buffer[lastRow, lastCol] = Border.BottomRight;

        var t = Border.Top;
        var b = Border.Bottom;
        var l = Border.Left;
        var r = Border.Right;

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
