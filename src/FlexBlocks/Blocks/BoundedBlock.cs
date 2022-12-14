using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

/// <summary>
/// A block which simply applies an optional maximum width and/or height to its content.
/// Useful for applying a size constraint to a block that would normally be unbounded.
/// </summary>
public class BoundedBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    /// <summary>The desired size of this block.</summary>
    public DesiredBlockSize DesiredSize { get; set; }

    /// <summary>The desired width of this block.</summary>
    public BlockLength Width
    {
        get => DesiredSize.Width;
        set => DesiredSize = DesiredSize with { Width = value };
    }

    /// <summary>The desired height of this block.</summary>
    public BlockLength Height
    {
        get => DesiredSize.Height;
        set => DesiredSize = DesiredSize with { Height = value };
    }

    /// <inheritdoc />
    public override DesiredBlockSize CalcDesiredSize(BlockSize maxSize) =>
        Content is not null
            ? DesiredBlockSize.Min(DesiredSize, Content.CalcDesiredSize(maxSize))
            : DesiredSize;

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Content is null) return;
        RenderChild(Content, buffer);
    }

    /// <inheritdoc />
    public override RerenderMode GetRerenderModeForChild(UiBlock child) => RerenderMode.DesiredSizeChanged;
}
