using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

/// <summary>
/// A block which simply applies an optional maximum width and/or height to its content.
/// Useful for applying a size constraint to a block that would normally be unbounded.
/// </summary>
[PublicAPI]
public sealed class BoundedBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    /// <summary>The desired maximum size of this block.</summary>
    public UnboundedBlockSize MaxSize { get; set; }

    /// <summary>
    /// The desired width of this block.
    /// If <see cref="BlockLength.Unbounded"/>, the contents will not be constrained in width.
    /// </summary>
    public BlockLength Width
    {
        get => MaxSize.Width;
        set => MaxSize = MaxSize with { Width = value };
    }

    /// <summary>
    /// The desired height of this block.
    /// If <see cref="BlockLength.Unbounded"/>, the contents will not be constrained in height.
    /// </summary>
    public BlockLength Height
    {
        get => MaxSize.Height;
        set => MaxSize = MaxSize with { Height = value };
    }

    /// <inheritdoc />
    public override BlockBounds GetBounds()
    {
        if (Content is null) return BlockBounds.Bounded;

        var contentBounds = Content.GetBounds();
        var maxBounds = MaxSize.ToBounds();

        return BlockBounds.Min(contentBounds, maxBounds);
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        if (Content is null) return BlockSize.Zero;

        var boundedSize = maxSize.Constrain(MaxSize);
        return Content.CalcSize(boundedSize).Constrain(boundedSize);
    }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Content is null) return;
        RenderChild(Content, buffer);
    }

    /// <inheritdoc />
    public override RerenderMode GetRerenderModeForChild(UiBlock child) => RerenderMode.DesiredSizeChanged;
}
