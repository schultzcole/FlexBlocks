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
    [PublicAPI]
    public UiBlock? Content { get; set; }

    /// <summary>The desired size of this block.</summary>
    [PublicAPI]
    public UnboundedBlockSize MaxSize { get; set; }

    /// <summary>The desired width of this block.</summary>
    [PublicAPI]
    public BlockLength Width
    {
        [PublicAPI]
        get => MaxSize.Width;
        [PublicAPI]
        set => MaxSize = MaxSize with { Width = value };
    }

    /// <summary>The desired height of this block.</summary>
    [PublicAPI]
    public BlockLength Height
    {
        [PublicAPI]
        get => MaxSize.Height;
        [PublicAPI]
        set => MaxSize = MaxSize with { Height = value };
    }

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize() =>
        Content is not null
            ? Content.CalcMaxSize().Constrain(MaxSize)
            : MaxSize;

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        var boundedSize = maxSize.Constrain(MaxSize);
        return Content is not null
            ? Content.CalcSize(boundedSize).Constrain(boundedSize)
            : boundedSize;
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
