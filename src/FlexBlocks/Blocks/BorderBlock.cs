using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

/// <summary>A <see cref="UiBlock"/> that optionally includes a border and padding around its content.</summary>
[PublicAPI]
public sealed class BorderBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    /// <summary>The type of border to render for this block</summary>
    public Border? Border { get; set; }

    /// <summary>What padding, if any should exist between the border and the content</summary>
    public Padding? Padding { get; set; }

    /// <summary>The total amount of padding around this block's content, including both border and padding.</summary>
    public Padding EffectivePadding =>
        (Border, Padding) switch
        {
            (null,     null)     => Padding.Zero,
            (null,     not null) => Padding,
            (not null, null)     => Border.ToPadding(),
            (not null, not null) => Padding + Border.ToPadding()
        };

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize()
    {
        var contentMaxSize = Content?.CalcMaxSize() ?? UnboundedBlockSize.Zero;

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
        var contentDesiredSize = (Content?.CalcSize(maxContentSize) ?? BlockSize.Zero).Constrain(maxContentSize);

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

        RenderChild(Content, contentBuffer);
    }

    /// <summary>Renders the border to the buffer.</summary>
    private void RenderBorder(Span2D<char> buffer)
    {
        if (Border is null) return;

        var lastRow = buffer.Height - 1;
        var lastCol = buffer.Width - 1;

        if (Border.TopLeft     is { } tl) buffer[0, 0]             = tl;
        if (Border.TopRight    is { } tr) buffer[0, lastCol]       = tr;
        if (Border.BottomLeft  is { } bl) buffer[lastRow, 0]       = bl;
        if (Border.BottomRight is { } br) buffer[lastRow, lastCol] = br;

        if (Border.Top is { } t) buffer.Slice(0,    1, 1, lastCol - 1).Fill(t);
        if (Border.Bottom is { } b) buffer.Slice(lastRow, 1, 1, lastCol - 1).Fill(b);
        if (Border.Left is { } l) buffer.Slice(1,   0, lastRow - 1, 1).Fill(l);
        if (Border.Right is { } r) buffer.Slice(1,   lastCol, lastRow - 1, 1).Fill(r);
    }
}
