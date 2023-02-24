using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

public class FlexBlock : AlignableBlock
{
    public List<UiBlock>? Contents { get; set; }

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

        var childCount = Contents.Count;
        var bufferSize = buffer.BlockSize();

        Span<BlockSize?> concreteSizes = stackalloc BlockSize?[childCount];

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
            concreteSizes[i] = concreteSize;
            remainingChildrenToAllocate--;
        }

        // If there are unbounded children, divide remaining width among them.
        if (remainingChildrenToAllocate > 0 && remainingConcreteWidth > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                if (concreteSizes[i] is not null) continue;

                var child = Contents[i];

                var availableWidth = remainingConcreteWidth / remainingChildrenToAllocate;

                var concreteSize = child.CalcSize(BlockSize.From(availableWidth, bufferSize.Height));
                remainingConcreteWidth -= concreteSize.Width;
                concreteSizes[i] = concreteSize;
                remainingChildrenToAllocate--;
            }
        }

        var xPos = 0;

        // Render children into their allocated segments of the buffer.
        for (int i = 0; i < childCount; i++)
        {
            var child = Contents[i];
            var size = concreteSizes[i]!.Value; // we can be sure that all concrete sizes have been calculated by now

            var childBuffer = buffer.Slice(0, xPos, size.Height, size.Width);
            RenderChild(child, childBuffer);
            xPos += size.Width;
        }
    }
}
