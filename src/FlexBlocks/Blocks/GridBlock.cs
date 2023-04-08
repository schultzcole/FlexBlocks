using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

public class GridBlock : UiBlock
{
    public UiBlock?[,]? Contents { get; set; }

    [MemberNotNullWhen(false, nameof(Contents))]
    public bool IsEmpty =>
        Contents is null || Contents.GetLength(0) == 0 || Contents.GetLength(1) == 0;

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize()
    {
        if (IsEmpty) return UnboundedBlockSize.Zero;

        var numRows = Contents.GetLength(0);
        var numCols = Contents.GetLength(1);

        Span<BlockLength> colWidths = stackalloc BlockLength[numCols];
        colWidths.Fill(0);
        Span<BlockLength> rowHeights = stackalloc BlockLength[numRows];
        rowHeights.Fill(0);
        for (int row = 0; row < numRows; row++)
        for (int col = 0; col < numCols; col++)
        {
            var block = Contents[row, col];
            if (block is null) continue;

            var blockSize = block.CalcMaxSize();
            colWidths[col] = BlockLength.Max(colWidths[col], blockSize.Width);
            rowHeights[row] = BlockLength.Max(rowHeights[row], blockSize.Height);
        }

        var totalWidth = BlockLength.Zero;
        foreach (var width in colWidths) totalWidth += width;
        var totalHeight = BlockLength.Zero;
        foreach (var height in rowHeights) totalHeight += height;

        return UnboundedBlockSize.From(totalWidth, totalHeight);
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize) => LayoutChildren(maxSize, Span2D<BufferSlice?>.Empty);

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (IsEmpty) return;

        var numRows = Contents.GetLength(0);
        var numCols = Contents.GetLength(1);

        var totalCells = numRows * numCols;
        var childArrangementArray = ArrayPool<BufferSlice?>.Shared.Rent(totalCells);
        var childArrangement = childArrangementArray
            .AsSpan(0, totalCells)
            .AsSpan2D(numRows, numCols);
        childArrangement.Fill(null);

        LayoutChildren(buffer.BlockSize(), childArrangement);

        for (int row = 0; row < numRows; row++)
        for (int col = 0; col < numCols; col++)
        {
            var slice = childArrangement[row, col];
            if (slice is null) continue;

            var block = Contents[row, col];
            if (block is null) continue;

            var childBuffer = buffer.Slice(slice.Row, slice.Column, slice.Height, slice.Width);
            RenderChild(block, childBuffer);
        }

        ArrayPool<BufferSlice?>.Shared.Return(childArrangementArray);
    }

    /// <summary>Calculates the size of each cell in the grid and positions each child within its cell.</summary>
    /// <returns>The total size of the final layout.</returns>
    private BlockSize LayoutChildren(BlockSize bufferSize, scoped Span2D<BufferSlice?> childArrangement)
    {
        if (IsEmpty) return BlockSize.Zero;

        var numRows = Contents.GetLength(0);
        var numCols = Contents.GetLength(1);

        var totalCells = numRows * numCols;
        var boundedSizesArray = ArrayPool<BlockSize?>.Shared.Rent(totalCells);
        Span2D<BlockSize?> boundedSizes = boundedSizesArray
            .AsSpan(0, totalCells)
            .AsSpan2D(numRows, numCols);
        boundedSizes.Fill(null);

        Span<BlockLength> colWidths = stackalloc BlockLength[numCols];
        colWidths.Fill(0);
        Span<BlockLength> rowHeights = stackalloc BlockLength[numRows];
        rowHeights.Fill(0);

        MeasureBoundedChildren(Contents, bufferSize, boundedSizes, colWidths, rowHeights);
        MeasureUnboundedChildren(Contents, bufferSize, boundedSizes, colWidths, rowHeights);

        var size = ArrangeChildren(Contents, bufferSize, childArrangement, boundedSizes, rowHeights, colWidths);

        ArrayPool<BlockSize?>.Shared.Return(boundedSizesArray);

        return size;
    }

    /// <summary>Computes the column widths and row heights based from bounded children.</summary>
    private static void MeasureBoundedChildren(
        UiBlock?[,] contents,
        BlockSize bufferSize,
        scoped Span2D<BlockSize?> boundedSizes,
        scoped Span<BlockLength> colWidths,
        scoped Span<BlockLength> rowHeights)
    {
        var numRows = contents.GetLength(0);
        var numCols = contents.GetLength(1);

        for (int row = 0; row < numRows; row++)
        for (int col = 0; col < numCols; col++)
        {
            var block = contents[row, col];
            if (block is null)
            {
                boundedSizes[row, col] = BlockSize.Zero;
                continue;
            }

            var maxSize = block.CalcMaxSize();

            if (maxSize.Width.IsUnbounded && maxSize.Height.IsUnbounded)
            {
                colWidths[col] = BlockLength.Unbounded;
                rowHeights[row] = BlockLength.Unbounded;
                continue;
            }

            var concreteSize = block.CalcSize(bufferSize);

            colWidths[col] = maxSize.Width.IsBounded
                ? BlockLength.Max(colWidths[col], concreteSize.Width)
                : BlockLength.Unbounded;

            rowHeights[row] = maxSize.Height.IsBounded
                ? BlockLength.Max(rowHeights[row], concreteSize.Height)
                : BlockLength.Unbounded;

            if (maxSize.IsBounded) boundedSizes[row, col] = concreteSize;
        }
    }

    /// <summary>Computes remaining undecided column widths and row heights based on the unbounded children.</summary>
    private static void MeasureUnboundedChildren(
        UiBlock?[,] contents,
        BlockSize bufferSize,
        scoped Span2D<BlockSize?> boundedSizes,
        scoped Span<BlockLength> colWidths,
        scoped Span<BlockLength> rowHeights
    )
    {
        AllocateRemainingLength(bufferSize.Width, colWidths);
        AllocateRemainingLength(bufferSize.Height, rowHeights);

        var numRows = contents.GetLength(0);
        var numCols = contents.GetLength(1);

        for (int row = 0; row < numRows; row++)
        for (int col = 0; col < numCols; col++)
        {
            if (boundedSizes[row, col] is not null) continue;

            var block = contents[row, col]!;
            var cellWidth = colWidths[col].Value.GetValueOrDefault();
            var cellHeight = rowHeights[row].Value.GetValueOrDefault();
            var concreteSize = block.CalcSize(BlockSize.From(cellWidth, cellHeight));

            boundedSizes[row, col] = concreteSize;
        }
    }

    /// <summary>Does the final arrangement of this grid's children based on the size of each child and the
    /// computed column widths and row heights.</summary>
    private static BlockSize ArrangeChildren(
        UiBlock?[,] contents,
        BlockSize bufferSize,
        scoped Span2D<BufferSlice?> childArrangement,
        scoped Span2D<BlockSize?> boundedSizes,
        scoped Span<BlockLength> rowHeights,
        scoped Span<BlockLength> colWidths
    )
    {
        var numRows = contents.GetLength(0);
        var numCols = contents.GetLength(1);
        var xPos = 0;
        var yPos = 0;
        for (int row = 0; row < numRows; row++)
        {
            var rowHeight = rowHeights[row].Value ??
                throw new UnreachableException("All rowHeights have been allocated by now");
            if (yPos + rowHeight > bufferSize.Height) break;

            xPos = 0;
            for (int col = 0; col < numCols; col++)
            {
                var colWidth = colWidths[col].Value ??
                    throw new UnreachableException("All colWidths have been allocated by now");
                if (xPos + colWidth > bufferSize.Width) break;

                if (boundedSizes[row, col] is { } size)
                {
                    if (!childArrangement.IsEmpty)
                    {
                        childArrangement[row, col] = new BufferSlice(xPos, yPos, size.Width, size.Height);
                    }
                }

                xPos += colWidth;
            }

            yPos += rowHeight;
        }

        return BlockSize.From(xPos, yPos);
    }

    /// <summary>Proportionally distributes remaining length among unbounded cells.</summary>
    private static void AllocateRemainingLength(int totalLength, scoped Span<BlockLength> cellLengths)
    {
        var remainingUnallocatedLength = totalLength;
        var remainingUnallocatedCellCount = 0;
        foreach (var len in cellLengths)
        {
            if (len.IsBounded)
            {
                remainingUnallocatedLength -= len.Value.Value;
            }
            else
            {
                remainingUnallocatedCellCount++;
            }
        }

        for (int i = 0; i < cellLengths.Length; i++)
        {
            if (remainingUnallocatedLength <= 0) break;
            if (remainingUnallocatedCellCount <= 0) break;
            if (cellLengths[i].IsBounded) continue;

            var availableLength = (int)Math.Ceiling((float)remainingUnallocatedLength / remainingUnallocatedCellCount);
            cellLengths[i] = availableLength;
            remainingUnallocatedLength -= availableLength;
            remainingUnallocatedCellCount--;
        }
    }
}
