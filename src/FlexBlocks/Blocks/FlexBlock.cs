using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

public enum FlexWrapping { NoWrap, Wrap }

public enum FLexDirection { Horizontal, Vertical }

[PublicAPI]
public class FlexBlock : UiBlock
{
    public List<UiBlock>? Contents { get; set; }

    [MemberNotNullWhen(false, nameof(Contents))]
    public bool IsEmpty => Contents is null or { Count: <= 0 };

    /// <summary>
    /// Whether the contents of this block should wrap to the next line if they are too long to fit in the flex
    /// dimension of the parent (as determined by the Direction).
    /// </summary>
    public FlexWrapping Wrapping { get; set; } = FlexWrapping.NoWrap;

    /// <summary>
    /// The direction that children in this block should be arranged.
    /// </summary>
    public FLexDirection Direction { get; set; } = FLexDirection.Horizontal;

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize()
    {
        if (IsEmpty) return UnboundedBlockSize.Zero;

        return Contents.Aggregate(
            UnboundedBlockSize.Zero,
            (maxSize, block) =>
            {
                var maxSizeInFlexBasis = GetSizeInFlexBasis(maxSize);
                var blockMaxSize = GetSizeInFlexBasis(block.CalcMaxSize());
                return MakeUnboundedSizeInScreenBasis(
                    maxSizeInFlexBasis.FlexLength + blockMaxSize.FlexLength,
                    BlockLength.Max(maxSizeInFlexBasis.CrossLength, blockMaxSize.CrossLength)
                );
            }
        );
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize) =>
        IsEmpty ? BlockSize.Zero : LayoutChildren(maxSize, null);

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

            if (size is null || size.IsEmpty) continue;

            var childBuffer = buffer.Slice(size);
            RenderChild(child, childBuffer);
        }

        ArrayPool<BufferSlice?>.Shared.Return(childArrangement);
    }

    /// <summary>
    /// Populates the given <paramref name="childArrangement"/> array with a set of buffer slices representing where
    /// each of the blocks in Contents should be placed in the render buffer.
    /// </summary>
    /// <returns>The total size that the arranged children take up.</returns>
    private BlockSize LayoutChildren(BlockSize bufferSize, BufferSlice?[]? childArrangement)
    {
        // -- Terminology --
        // screen-basis:
        //     The coordinate system of the screen to which we're rendering.
        //     The dimensions of this basis are 'x' and 'y', and the lengths in these dimensions are 'width' and
        //     'height', respectively.
        //     BufferSlices and BlockSizes are in this basis.
        // flex-basis:
        //     The coordinate system in which child blocks "flow", as determined by the Direction property.
        //     The dimensions of this basis are 'flex' and 'cross'.
        //     When Direction is Horizontal, 'flex' and 'cross' are aligned with the 'x' and 'y' dimensions of the
        //     screen basis. When Direction is Vertical, 'flex' and 'cross' are transposed.

        if (Contents is null) return BlockSize.Zero;

        var childCount = Contents.Count;

        var flexBasisBufferSize = GetSizeInFlexBasis(bufferSize);

        var crossPos = 0;
        var startOfCurrentLine = 0;
        var allocatedLengthInCurrentLine = 0;
        var unallocatedBlocksInCurrentLine = 0;
        var maxLengthInFlexDirection = 0;

        Span<BlockSize?> boundedSizes = stackalloc BlockSize?[childCount];
        var i = 0;
        for (; i < childCount; i++)
        {
            var block = Contents[i];
            var maxSize = block.CalcMaxSize();
            var (maxFlexLength, _) = GetSizeInFlexBasis(maxSize);

            if (maxFlexLength.IsBounded)
            {
                var concreteSize = block.CalcSize(bufferSize);
                boundedSizes[i] = concreteSize;
                var (childFlexLength, _) = GetSizeInFlexBasis(concreteSize);

                var isOverflowing = allocatedLengthInCurrentLine + childFlexLength > flexBasisBufferSize.FlexLength;
                if (isOverflowing)
                {
                    if (Wrapping == FlexWrapping.NoWrap) break;

                    // wrap to next line
                    arrangeLine(boundedSizes, i - 1);

                    if (crossPos > flexBasisBufferSize.CrossLength) break;

                    startOfCurrentLine = i;
                    allocatedLengthInCurrentLine = childFlexLength;
                    unallocatedBlocksInCurrentLine = 0;
                }
                else
                {
                    allocatedLengthInCurrentLine += childFlexLength;
                }
            }
            else
            {
                unallocatedBlocksInCurrentLine++;
            }
        }

        if (startOfCurrentLine <= i)
        {
            // arrange the last non-overflowing line up until the end of blocks that were measured
            arrangeLine(boundedSizes, i - 1);
        }

        return MakeSizeInScreenBasis(maxLengthInFlexDirection, crossPos);

        ////////////////////////////////////////////////////////////////
        // adds a line's worth of blocks to the final arrangement array.
        ////////////////////////////////////////////////////////////////
        void arrangeLine(Span<BlockSize?> sizes, int endIndex)
        {
            var flexPos = 0;
            var maxLineCrossLength = 0;
            for (int i = startOfCurrentLine; i <= endIndex; i++)
            {
                var maybeSize = sizes[i];
                if (maybeSize is null)
                {
                    var remainingFlexLength = flexBasisBufferSize.FlexLength - allocatedLengthInCurrentLine;
                    var availableCrossLength = flexBasisBufferSize.CrossLength - crossPos;
                    var availableFlexLength =
                        (int)Math.Ceiling((float)remainingFlexLength / unallocatedBlocksInCurrentLine);
                    var block = Contents[i];
                    var availableSize = MakeSizeInScreenBasis(availableFlexLength, availableCrossLength);
                    maybeSize = block.CalcSize(availableSize).Constrain(availableSize);
                    unallocatedBlocksInCurrentLine--;
                    allocatedLengthInCurrentLine += GetSizeInFlexBasis(maybeSize.Value).FlexLength;
                }

                var size = GetSizeInFlexBasis(maybeSize.Value);

                maxLineCrossLength = Math.Max(maxLineCrossLength, size.CrossLength);

                var fitsInFlexDimension = flexPos + size.FlexLength <= flexBasisBufferSize.FlexLength;
                var fitsInCrossDimension = crossPos + size.CrossLength <= flexBasisBufferSize.CrossLength;
                if (fitsInFlexDimension && fitsInCrossDimension)
                {
                    if (childArrangement is not null)
                    {
                        childArrangement[i] =
                            MakeSliceInScreenBasis(flexPos, crossPos, size.FlexLength, size.CrossLength);
                    }
                }

                flexPos += size.FlexLength;
            }

            crossPos += maxLineCrossLength;
            maxLengthInFlexDirection = Math.Max(maxLengthInFlexDirection, flexPos);
        }
    }

    /// <summary>Returns the dimensions of the given screen-basis BlockSize transformed to flex-basis.</summary>
    private (int FlexLength, int CrossLength) GetSizeInFlexBasis(BlockSize size) => Direction switch
    {
        FLexDirection.Horizontal => (size.Width, size.Height),
        FLexDirection.Vertical   => (size.Height, size.Width),
        _                        => throw new Exception($"Unknown FlexDirection: {Direction}")
    };

    /// <summary>Returns the dimensions of the given screen-basis UnboundedBlockSize transformed to flex-basis.</summary>
    private (BlockLength FlexLength, BlockLength CrossLength) GetSizeInFlexBasis(UnboundedBlockSize size) =>
        Direction switch
        {
            FLexDirection.Horizontal => (size.Width, size.Height),
            FLexDirection.Vertical   => (size.Height, size.Width),
            _                        => throw new Exception($"Unknown FlexDirection: {Direction}")
        };

    /// <summary>Creates a screen basis BufferSlice with the given position and size given in the flex basis.</summary>
    private BufferSlice MakeSliceInScreenBasis(int flexPos, int crossPos, int flexLength, int crossLength) =>
        Direction switch
        {
            FLexDirection.Horizontal => new BufferSlice(flexPos, crossPos, flexLength, crossLength),
            FLexDirection.Vertical   => new BufferSlice(crossPos, flexPos, crossLength, flexLength),
            _                        => throw new Exception($"Unknown FlexDirection: {Direction}")
        };

    /// <summary>Creates a screen basis BlockSize with the given size given in the flex basis.</summary>
    private BlockSize MakeSizeInScreenBasis(int flexLength, int crossLength) =>
        Direction switch {
            FLexDirection.Horizontal => new BlockSize(flexLength, crossLength),
            FLexDirection.Vertical   => new BlockSize(crossLength, flexLength),
            _                        => throw new Exception($"Unknown FlexDirection: {Direction}")
        };

    /// <summary>Creates a screen basis UnboundedBlockSize with the given size given in the flex basis.</summary>
    private UnboundedBlockSize MakeUnboundedSizeInScreenBasis(BlockLength flexLength, BlockLength crossLength) =>
        Direction switch {
            FLexDirection.Horizontal => new UnboundedBlockSize(flexLength, crossLength),
            FLexDirection.Vertical   => new UnboundedBlockSize(crossLength, flexLength),
            _                        => throw new Exception($"Unknown FlexDirection: {Direction}")
        };
}
