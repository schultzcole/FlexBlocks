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
    public IBorder? Border { get; set; }

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
        return contentMaxSize.ExpandByPadding(padding);
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        var padding = EffectivePadding;
        var maxContentSize = maxSize.ShrinkByPadding(padding);
        var contentDesiredSize = (Content?.CalcSize(maxContentSize) ?? BlockSize.Zero).Constrain(maxContentSize);

        return contentDesiredSize.ExpandByPadding(padding);
    }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Border is not null) BorderRenderHelper.RenderOuter(Border, buffer);

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
}
