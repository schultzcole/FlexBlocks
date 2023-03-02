using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

public class FlexBlock : AlignableBlock
{
    public List<UiBlock>? Contents { get; set; }

    /// <summary>
    /// Whether the contents of this block should wrap to the next row if they are too wide for the width of the parent.
    /// </summary>
    public bool Wrapping { get; set; }

    /// <inheritdoc />
    public override bool HasContent => Contents?.Count > 0;

    /// <inheritdoc />
    protected override UnboundedBlockSize? CalcContentMaxSize() =>
        Contents?.Aggregate(
            UnboundedBlockSize.Zero,
            static (maxSize, block) =>
            {
                var blockMaxSize = block.CalcMaxSize();
                return UnboundedBlockSize.From(
                    maxSize.Width + blockMaxSize.Width,
                    BlockLength.Max(maxSize.Height, blockMaxSize.Height)
                );
            }
        );

    /// <inheritdoc />
    protected override BlockSize? CalcContentSize(BlockSize maxSize) =>
        Contents?.Aggregate(
            BlockSize.Zero,
            (runningSize, block) =>
            {
                var remainingSize = BlockSize.From(maxSize.Width - runningSize.Width, maxSize.Height);
                var blockSize = block.CalcSize(remainingSize);
                return BlockSize.From(
                    runningSize.Width + blockSize.Width,
                    Math.Max(runningSize.Height, blockSize.Height)
                );
            }
        );

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Contents is null) return;

        Span<BlockSize> concreteSizes = stackalloc BlockSize[Contents.Count];

        MeasureChildren(buffer.BlockSize(), concreteSizes);

        var xPos = 0;
        var yPos = 0;
        var maxHeightInRow = 0;
        for (int i = 0; i < Contents.Count; i++)
        {
            var child = Contents[i];
            var size = concreteSizes[i];

            if (size.Width + xPos > buffer.Width)
            {
                if (Wrapping)
                {
                    xPos = 0;
                    yPos += maxHeightInRow;
                    if (yPos > buffer.Height) return;
                }
                else
                {
                    return;
                }
            }

            if (size.Height + yPos <= buffer.Height)
            {
                maxHeightInRow = Math.Max(size.Height, maxHeightInRow);

                var childBuffer = buffer.Slice(yPos, xPos, size.Height, size.Width);
                RenderChild(child, childBuffer);
            }

            xPos += size.Width;
        }
    }

    private void MeasureChildren(BlockSize bufferSize, Span<BlockSize> concreteSizes)
    {
        if (Contents is null) return;

        var childCount = Contents.Count;

        Span<BlockSize?> accumulatedSizes = stackalloc BlockSize?[childCount];

        var remainingConcreteWidth = bufferSize.Width;
        var remainingChildrenToAllocate = Contents.Count;

        // Find children with bound width and calculate their concrete size.
        for (var i = 0; i < childCount; i++)
        {
            var child = Contents[i];
            var maxSize = child.CalcMaxSize();
            if (maxSize.Width.IsUnbounded) continue;

            var concreteSize = child.CalcSize(maxSize.Constrain(bufferSize));
            remainingConcreteWidth -= concreteSize.Width;
            accumulatedSizes[i] = concreteSize;
            remainingChildrenToAllocate--;
        }

        // If there are unbounded children, divide remaining width among them.
        if (remainingChildrenToAllocate > 0 && remainingConcreteWidth > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                if (accumulatedSizes[i] is not null) continue;

                var child = Contents[i];

                var availableWidth = remainingConcreteWidth / remainingChildrenToAllocate;

                var concreteSize = child.CalcSize(BlockSize.From(availableWidth, bufferSize.Height));
                remainingConcreteWidth -= concreteSize.Width;
                accumulatedSizes[i] = concreteSize;
                remainingChildrenToAllocate--;
            }
        }

        for (int i = 0; i < childCount; i++)
        {
            concreteSizes[i] = accumulatedSizes[i] ?? BlockSize.Zero;
        }
    }
}
