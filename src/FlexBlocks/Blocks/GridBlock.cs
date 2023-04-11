using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

[PublicAPI]
public sealed class GridBlock : UiBlock
{
    [PublicAPI]
    public UiBlock?[,]? Contents { get; set; }

    [MemberNotNullWhen(false, nameof(Contents))]
    [PublicAPI]
    public bool IsEmpty =>
        Contents is null || Contents.GetLength(0) == 0 || Contents.GetLength(1) == 0;

    /// <summary>Adds a new column to this grid.</summary>
    /// <param name="newColumn">The blocks to insert into the new column.</param>
    [MemberNotNull(nameof(Contents))]
    [PublicAPI]
    public void AddColumn(IReadOnlyList<UiBlock?> newColumn)
    {
        EnsureCapacity(IsEmpty ? 1 : Contents.GetLength(1) + 1, newColumn.Count);
        CopyColumnToContents(newColumn);
    }

    /// <summary>Adds a new column to this grid.</summary>
    /// <param name="newColumn">The blocks to insert into the new column.</param>
    [MemberNotNull(nameof(Contents))]
    [PublicAPI]
    public void AddColumn(scoped Span<UiBlock?> newColumn)
    {
        EnsureCapacity(IsEmpty ? 1 : Contents.GetLength(1) + 1, newColumn.Length);
        CopyColumnToContents(newColumn);
    }

    /// <summary>Adds a new column to this grid.</summary>
    /// <param name="newColumn">The blocks to insert into the new column.</param>
    [MemberNotNull(nameof(Contents))]
    [PublicAPI]
    public void AddColumn(params UiBlock?[] newColumn) => AddColumn(newColumn.AsSpan());

    /// <summary>Adds a new row to this grid.</summary>
    /// <param name="newRow">The blocks to insert into the new row.</param>
    [MemberNotNull(nameof(Contents))]
    [PublicAPI]
    public void AddRow(IReadOnlyList<UiBlock?> newRow)
    {
        EnsureCapacity(newRow.Count, IsEmpty ? 1 : Contents.GetLength(0) + 1);
        CopyRowToContents(newRow);
    }

    /// <summary>Adds a new row to this grid.</summary>
    /// <param name="newRow">The blocks to insert into the new row.</param>
    [MemberNotNull(nameof(Contents))]
    [PublicAPI]
    public void AddRow(scoped Span<UiBlock?> newRow)
    {
        EnsureCapacity(newRow.Length, IsEmpty ? 1 : Contents.GetLength(0) + 1);
        CopyRowToContents(newRow);
    }

    /// <summary>Adds a new row to this grid.</summary>
    /// <param name="newRow">The blocks to insert into the new row.</param>
    [MemberNotNull(nameof(Contents))]
    [PublicAPI]
    public void AddRow(params UiBlock?[] newRow) => AddRow(newRow.AsSpan());

    /// <summary>Ensures that the Content array is at least as large as the given width and height</summary>
    [MemberNotNull(nameof(Contents))]
    private void EnsureCapacity(int width, int height)
    {
        if (Contents is null)
        {
            Contents = new UiBlock?[height, width];
            return;
        }

        Span2D<UiBlock?> contentSpan = Contents.AsSpan2D();

        if (contentSpan.Width >= width && contentSpan.Height >= height) return;

        var newContents = new UiBlock?[Math.Max(contentSpan.Height, height), Math.Max(contentSpan.Width, width)];

        if (!contentSpan.IsEmpty)
        {
            var newContentSpan = newContents.AsSpan2D(0, 0, contentSpan.Height, contentSpan.Width);
            contentSpan.CopyTo(newContentSpan);
        }

        Contents = newContents;
    }

    /// <summary>Copies a given column of blocks to the last column of the grid</summary>
    /// <param name="column">the blocks to copy into the grid</param>
    /// <exception cref="InvalidOperationException">If Contents is null or 0 in either dimension</exception>
    private void CopyColumnToContents(scoped Span<UiBlock?> column)
    {
        if (IsEmpty) throw new InvalidOperationException("Cannot add column to empty Contents");
        var columnSpan2D = column.AsSpan2D(column.Length, 1);
        var contentSpan = Contents.AsSpan2D();
        columnSpan2D.CopyTo(contentSpan.Slice(0, contentSpan.Width - 1, column.Length, 1));
    }

    /// <summary>Copies a given column of blocks to the last column of the grid</summary>
    /// <param name="column">the blocks to copy into the grid</param>
    /// <exception cref="InvalidOperationException">If Contents is null or 0 in either dimension</exception>
    private void CopyColumnToContents(IReadOnlyList<UiBlock?> column)
    {
        if (IsEmpty) throw new InvalidOperationException("Cannot add column to empty Contents");
        if (column is List<UiBlock?> or UiBlock?[])
        {
            var columnSpan = (column switch
            {
                List<UiBlock?> list => CollectionsMarshal.AsSpan(list),
                UiBlock?[] arr      => arr.AsSpan(),
                _                   => throw new UnreachableException("column must be List<UiBlock?> or UiBlock?[]")
            });
            CopyColumnToContents(columnSpan);
        }
        else
        {
            for (int i = 0; i < column.Count; i++)
            {
                Contents[i, Contents.GetLength(1)] = column[i];
            }
        }
    }

    /// <summary>Copies a given row of blocks to the last row of the grid</summary>
    /// <param name="row">the blocks to copy into the grid</param>
    /// <exception cref="InvalidOperationException">If Contents is null or 0 in either dimension</exception>
    private void CopyRowToContents(scoped Span<UiBlock?> row)
    {
        if (IsEmpty) throw new InvalidOperationException("Cannot add row to empty Contents");
        var rowSpan2D = row.AsSpan2D(1, row.Length);
        var contentSpan = Contents.AsSpan2D();
        rowSpan2D.CopyTo(contentSpan.Slice(contentSpan.Height - 1, 0, 1, row.Length));
    }

    /// <summary>Copies a given row of blocks to the last row of the grid</summary>
    /// <param name="row">the blocks to copy into the grid</param>
    /// <exception cref="InvalidOperationException">If Contents is null or 0 in either dimension</exception>
    private void CopyRowToContents(IReadOnlyList<UiBlock?> row)
    {
        if (IsEmpty) throw new InvalidOperationException("Cannot add row to empty Contents");
        if (row is List<UiBlock?> or UiBlock?[])
        {
            var rowSpan = (row switch
            {
                List<UiBlock?> list => CollectionsMarshal.AsSpan(list),
                UiBlock?[] arr      => arr.AsSpan(),
                _                   => throw new UnreachableException("row must be List<UiBlock?> or UiBlock?[]")
            });
            CopyRowToContents(rowSpan);
        }
        else
        {
            for (int i = 0; i < row.Count; i++)
            {
                Contents[Contents.GetLength(1), i] = row[i];
            }
        }
    }

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
