using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

public enum Sizing { Fill, Content }

public enum Alignment { Start, Center, End }

/// <summary>
/// A block type that contains another single block, can align that block within itself,
/// and can either size itself to its content or grow to fill available space in either or both dimensions.
/// </summary>
[PublicAPI]
public class ContentBlock : AlignableBlock
{
    [PublicAPI]
    public UiBlock? Content { get; set; }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer) => RenderContent(buffer);

    /// <inheritdoc />
    public override bool HasContent => Content is not null;

    /// <inheritdoc />
    protected override UnboundedBlockSize? CalcContentMaxSize() => Content?.CalcMaxSize();

    /// <inheritdoc />
    protected override BlockSize? CalcContentSize(BlockSize maxSize) => Content?.CalcSize(maxSize);

    /// <summary>Renders this block's content to the given buffer, possibly aligned (<see cref="Alignment"/>).</summary>
    protected virtual void RenderContent(Span2D<char> contentBuffer)
    {
        if (Content is null) return;

        var alignedBuffer = ComputeAlignedContentBuffer(contentBuffer);

        RenderChild(Content, alignedBuffer);
    }
}
