using System.Buffers;
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
        Contents is null ? null : MeasureChildren(maxSize, null);

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Contents is null) return;

        var concreteSizes = ArrayPool<BufferSlice?>.Shared.Rent(Contents.Count);
        Array.Fill(concreteSizes, null);

        MeasureChildren(buffer.BlockSize(), concreteSizes);

        for (int i = 0; i < Contents.Count; i++)
        {
            var child = Contents[i];
            var size = concreteSizes[i];

            if (size is null) continue;

            var childBuffer = buffer.Slice(size.Row, size.Column, size.Height, size.Width);
            RenderChild(child, childBuffer);
        }

        ArrayPool<BufferSlice?>.Shared.Return(concreteSizes);
    }

    private BlockSize MeasureChildren(BlockSize bufferSize, BufferSlice?[]? concreteSizes)
    {
        if (Contents is null) return BlockSize.Zero;

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

        var xPos = 0;
        var yPos = 0;
        var maxHeightInRow = 0;
        var maxWidth = 0;
        var maxHeight = 0;
        for (int i = 0; i < childCount; i++)
        {
            if (accumulatedSizes[i] is null) continue;

            var size = accumulatedSizes[i]!.Value;

            if (size.Width + xPos > bufferSize.Width)
            {
                // Overflowing the end of the row
                if (Wrapping)
                {
                    xPos = 0;
                    yPos += maxHeightInRow;
                    maxHeight = yPos;
                    maxHeightInRow = 0;
                    if (yPos > bufferSize.Height) break;
                }
                else
                {
                    break;
                }
            }

            if (size.Height + yPos <= bufferSize.Height)
            {
                maxHeightInRow = Math.Max(size.Height, maxHeightInRow);
                if (concreteSizes is not null)
                {
                    concreteSizes[i] = new BufferSlice(xPos, yPos, size.Width, size.Height);
                }
            }

            xPos += size.Width;
            maxWidth = Math.Max(maxWidth, xPos);
        }

        return BlockSize.From(maxWidth, maxHeight);
    }
}
