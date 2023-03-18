using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

/// <summary>
/// A block that always renders as the same size, regardless of its content
/// </summary>
public class FixedSizeBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    /// <summary>The fixed size of this block.</summary>
    public UnboundedBlockSize Size { get; set; }

    /// <summary>The desired width of this block.</summary>
    public BlockLength Width
    {
        get => Size.Width;
        set => Size = Size with { Width = value };
    }

    /// <summary>The desired height of this block.</summary>
    public BlockLength Height
    {
        get => Size.Height;
        set => Size = Size with { Height = value };
    }

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize() => Size;

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize) => maxSize.Constrain(Size);

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Content is null) return;

        var bufferSize = buffer.BlockSize().Constrain(Size);
        var contentSize = Content.CalcSize(bufferSize).Constrain(bufferSize);
        RenderChild(Content, buffer.Slice(0, 0, contentSize.Height, contentSize.Width));
    }

    /// <inheritdoc />
    public override RerenderMode GetRerenderModeForChild(UiBlock child) => RerenderMode.DesiredSizeChanged;
}
