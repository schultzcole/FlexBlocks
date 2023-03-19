using System.Buffers;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

public enum FlexWrapping { NoWrap, Wrap }

public class FlexBlock : AlignableBlock
{
    public List<UiBlock>? Contents { get; set; }

    /// <summary>
    /// Whether the contents of this block should wrap to the next row if they are too wide for the width of the parent.
    /// </summary>
    public FlexWrapping Wrapping { get; set; } = FlexWrapping.NoWrap;

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
        Contents is not null ? LayoutChildren(maxSize, null) : null;

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Contents is null) return;

        var childArrangement = ArrayPool<BufferSlice?>.Shared.Rent(Contents.Count);
        Array.Fill(childArrangement, null);

        LayoutChildren(buffer.BlockSize(), childArrangement);

        for (int i = 0; i < Contents.Count; i++)
        {
            var child = Contents[i];
            var size = childArrangement[i];

            if (size is null) continue;

            var childBuffer = buffer.Slice(size.Row, size.Column, size.Height, size.Width);
            RenderChild(child, childBuffer);
        }

        ArrayPool<BufferSlice?>.Shared.Return(childArrangement);
    }

    private BlockSize LayoutChildren(BlockSize bufferSize, BufferSlice?[]? childArrangement)
    {
        if (Contents is null) return BlockSize.Zero;

        var childCount = Contents.Count;

        var yPos = 0;
        var startOfCurrentRow = 0;
        var allocatedWidthInCurrentRow = 0;
        var unallocatedBlocksInCurrentRow = 0;
        var maxWidth = 0;

        Span<BlockSize?> boundedSizes = stackalloc BlockSize?[childCount];
        var i = 0;
        for (; i < childCount; i++)
        {
            var block = Contents[i];
            var maxSize = block.CalcMaxSize();

            if (maxSize.Width.IsBounded)
            {
                var concreteSize = block.CalcSize(maxSize.Constrain(bufferSize));
                boundedSizes[i] = concreteSize;
                var width = concreteSize.Width;

                var isOverflowing = allocatedWidthInCurrentRow + width > bufferSize.Width;
                if (isOverflowing)
                {
                    if (Wrapping == FlexWrapping.NoWrap) break;

                    // wrap to next row
                    arrangeRow(boundedSizes, i - 1);

                    if (yPos > bufferSize.Height) break;

                    startOfCurrentRow = i;
                    allocatedWidthInCurrentRow = width;
                    unallocatedBlocksInCurrentRow = maxSize.Width.IsBounded ? 0 : 1;
                }
                else
                {
                    allocatedWidthInCurrentRow += width;
                }
            }
            else
            {
                unallocatedBlocksInCurrentRow++;
            }
        }

        if (startOfCurrentRow <= i)
        {
            // arrange the last non-overflowing row up until the end of blocks that were measured
            arrangeRow(boundedSizes, i - 1);
        }

        return BlockSize.From(maxWidth, yPos);

        ///////////////////////////////////////////////////////////////
        // adds a row's worth of blocks to the final arrangement array.
        ///////////////////////////////////////////////////////////////
        void arrangeRow(Span<BlockSize?> sizes, int endIndex)
        {
            var xPos = 0;
            var maxRowHeight = 0;
            for (int i = startOfCurrentRow; i <= endIndex; i++)
            {
                var maybeSize = sizes[i];
                if (maybeSize is null)
                {
                    var remainingWidth = bufferSize.Width - allocatedWidthInCurrentRow;
                    var remainingHeight = bufferSize.Height - yPos;
                    var width = (int)Math.Ceiling((float)remainingWidth / unallocatedBlocksInCurrentRow);
                    var block = Contents[i];
                    var availableSize = BlockSize.From(width, remainingHeight);
                    maybeSize = block.CalcSize(availableSize).Constrain(availableSize);
                    unallocatedBlocksInCurrentRow--;
                    allocatedWidthInCurrentRow += maybeSize.Value.Width;
                }

                var size = maybeSize.Value;

                maxRowHeight = Math.Max(maxRowHeight, size.Height);

                if (xPos + size.Width <= bufferSize.Width && yPos + size.Height <= bufferSize.Height)
                {
                    if (childArrangement is not null)
                    {
                        childArrangement[i] = new BufferSlice(xPos, yPos, size.Width, size.Height);
                    }
                }

                xPos += size.Width;
            }

            yPos += maxRowHeight;
            maxWidth = Math.Max(maxWidth, xPos);
        }
    }
}
