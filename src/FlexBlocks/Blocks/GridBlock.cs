﻿using System.Buffers;
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
    /// <summary>The blocks that make up the cells of this grid.</summary>
    public UiBlock?[,]? Contents { get; set; }

    /// <summary>The border to show around the cells of this block.</summary>
    public IBorder? Border { get; set; }

    /// <summary>
    /// The rows that should render an accented border next to them. If the index is from end, the accented border is
    /// rendered before the corresponding row, otherwise it is rendered after the corresponding row.
    /// </summary>
    public List<Index>? AccentRows { get; set; }

    /// <summary>
    /// The columns that should render an accented border next to them. If the index is from end, the accented border is
    /// rendered before the corresponding column, otherwise it is rendered after the corresponding column.
    /// </summary>
    public List<Index>? AccentColumns { get; set; }

    [MemberNotNullWhen(false, nameof(Contents))]
    public bool IsEmpty =>
        Contents is null || Contents.GetLength(0) == 0 || Contents.GetLength(1) == 0;

    /// <summary>Adds a new column to this grid.</summary>
    /// <param name="newColumn">The blocks to insert into the new column.</param>
    [MemberNotNull(nameof(Contents))]
    public void AddColumn(IReadOnlyList<UiBlock?> newColumn)
    {
        EnsureCapacity(IsEmpty ? 1 : Contents.GetLength(1) + 1, newColumn.Count);
        CopyColumnToContents(newColumn);
    }

    /// <summary>Adds a new column to this grid.</summary>
    /// <param name="newColumn">The blocks to insert into the new column.</param>
    [MemberNotNull(nameof(Contents))]
    public void AddColumn(scoped Span<UiBlock?> newColumn)
    {
        EnsureCapacity(IsEmpty ? 1 : Contents.GetLength(1) + 1, newColumn.Length);
        CopyColumnToContents(newColumn);
    }

    /// <summary>Adds a new column to this grid.</summary>
    /// <param name="newColumn">The blocks to insert into the new column.</param>
    [MemberNotNull(nameof(Contents))]
    public void AddColumn(params UiBlock?[] newColumn) => AddColumn(newColumn.AsSpan());

    /// <summary>Adds a new row to this grid.</summary>
    /// <param name="newRow">The blocks to insert into the new row.</param>
    [MemberNotNull(nameof(Contents))]
    public void AddRow(IReadOnlyList<UiBlock?> newRow)
    {
        EnsureCapacity(newRow.Count, IsEmpty ? 1 : Contents.GetLength(0) + 1);
        CopyRowToContents(newRow);
    }

    /// <summary>Adds a new row to this grid.</summary>
    /// <param name="newRow">The blocks to insert into the new row.</param>
    [MemberNotNull(nameof(Contents))]
    public void AddRow(scoped Span<UiBlock?> newRow)
    {
        EnsureCapacity(newRow.Length, IsEmpty ? 1 : Contents.GetLength(0) + 1);
        CopyRowToContents(newRow);
    }

    /// <summary>Adds a new row to this grid.</summary>
    /// <param name="newRow">The blocks to insert into the new row.</param>
    [MemberNotNull(nameof(Contents))]
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
    public override BlockBounds GetBounds()
    {
        if (IsEmpty) return BlockBounds.Bounded;

        var numRows = Contents.GetLength(0);
        var numCols = Contents.GetLength(1);

        var horizBounding = Bounding.Bounded;
        var vertBounding = Bounding.Bounded;

        for (int row = 0; row < numRows; row++)
        for (int col = 0; col < numCols; col++)
        {
            var block = Contents[row, col];
            if (block is null) continue;

            var blockSize = block.GetBounds();
            horizBounding = BoundingExtensions.Max(blockSize.Horizontal, horizBounding);
            vertBounding = BoundingExtensions.Max(blockSize.Vertical, vertBounding);

            if (horizBounding == Bounding.Unbounded && vertBounding == Bounding.Unbounded) break;
        }

        return BlockBounds.From(horizBounding, vertBounding);
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize) => LayoutChildren(maxSize);

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

        LayoutChildren(buffer.BlockSize(), ref childArrangement);

        if (Border is not null) RenderBorder(Border, childArrangement, buffer);

        for (int row = 0; row < childArrangement.Height; row++)
        for (int col = 0; col < childArrangement.Width; col++)
        {
            var slice = childArrangement[row, col];
            if (slice is null) continue;

            var block = Contents[row, col];
            if (block is null) continue;

            var childBuffer = buffer.Slice(slice);
            RenderChild(block, childBuffer);
        }

        ArrayPool<BufferSlice?>.Shared.Return(childArrangementArray);
    }

    private void RenderBorder(IBorder border, Span2D<BufferSlice?> childArrangement, Span2D<char> buffer)
    {
        BorderRenderHelper.RenderOuter(border, buffer);

        Span<(int index, BorderAccent accent)> rowGaps =
            stackalloc (int index, BorderAccent accent)[childArrangement.Height - 1];
        Span<(int index, BorderAccent accent)> colGaps =
            stackalloc (int index, BorderAccent accent)[childArrangement.Width - 1];
        for (int row = 1; row < childArrangement.Height; row++)
        {
            var slice = childArrangement[row, 0]!;
            rowGaps[row - 1] = (slice.Row - 1, default);
        }

        for (int col = 1; col < childArrangement.Width; col++)
        {
            var slice = childArrangement[0, col]!;
            colGaps[col - 1] = (slice.Column - 1, default);
        }

        foreach (var row in AccentRows ?? Enumerable.Empty<Index>()) rowGaps[row].accent = BorderAccent.Accent;
        foreach (var col in AccentColumns ?? Enumerable.Empty<Index>()) colGaps[col].accent = BorderAccent.Accent;

        BorderRenderHelper.RenderInner(border, buffer, rowGaps, colGaps);
    }

    /// <summary>Calculates the size of each cell in the grid and positions each child within its cell.</summary>
    /// <returns>The total size of the final layout.</returns>
    private BlockSize LayoutChildren(BlockSize bufferSize, ref Span2D<BufferSlice?> childArrangement)
    {
        if (IsEmpty) return BlockSize.Zero;

        var contentSpan = Contents.AsSpan2D();

        var numRows = contentSpan.Height;
        var numCols = contentSpan.Width;

        var totalCells = (int)contentSpan.Length;
        var boundedSizesArray = ArrayPool<BlockSize?>.Shared.Rent(totalCells);
        Span2D<BlockSize?> boundedSizes = boundedSizesArray
                                          .AsSpan(0, totalCells)
                                          .AsSpan2D(numRows, numCols);
        boundedSizes.Fill(null);

        Span<BlockLength> colWidths = stackalloc BlockLength[numCols];
        Span<BlockLength> rowHeights = stackalloc BlockLength[numRows];
        colWidths.Fill(0);
        rowHeights.Fill(0);

        var borderPadding = Border?.ToPadding() ?? Padding.Zero;

        var innerBufferSize = bufferSize.ShrinkByPadding(borderPadding);
        MeasureBoundedChildren(contentSpan, innerBufferSize, boundedSizes, colWidths, rowHeights);

        var colAccents = AccentColumns as IReadOnlyList<Index> ?? Array.Empty<Index>();
        var rowAccents = AccentRows as IReadOnlyList<Index> ?? Array.Empty<Index>();

        Span<int> colGaps = stackalloc int[numCols - 1];
        Span<int> rowGaps = stackalloc int[numRows - 1];

        if (Border is not null) CalculateGaps(Border, colAccents, rowAccents, colGaps, rowGaps);

        AllocateRemainingLength(innerBufferSize.Width,  colWidths,  colGaps);
        AllocateRemainingLength(innerBufferSize.Height, rowHeights, rowGaps);

        var size = ArrangeChildren(
            contentSpan,
            bufferSize,
            borderPadding,
            ref childArrangement,
            boundedSizes,
            rowHeights,
            colWidths,
            colAccents,
            rowAccents,
            colGaps,
            rowGaps
        );

        ArrayPool<BlockSize?>.Shared.Return(boundedSizesArray);

        return size;
    }

    private BlockSize LayoutChildren(BlockSize bufferSize)
    {
        var childArrangement = Span2D<BufferSlice?>.Empty;
        return LayoutChildren(bufferSize, ref childArrangement);
    }

    /// <summary>Computes the column widths and row heights based from bounded children.</summary>
    private static void MeasureBoundedChildren(
        Span2D<UiBlock?> contents,
        BlockSize bufferSize,
        scoped Span2D<BlockSize?> boundedSizes,
        scoped Span<BlockLength> colWidths,
        scoped Span<BlockLength> rowHeights)
    {
        for (int row = 0; row < contents.Height; row++)
        for (int col = 0; col < contents.Width; col++)
        {
            var block = contents[row, col];
            if (block is null)
            {
                boundedSizes[row, col] = BlockSize.Zero;
                continue;
            }

            var bounds = block.GetBounds();

            if (bounds.IsUnbounded)
            {
                colWidths[col] = BlockLength.Unbounded;
                rowHeights[row] = BlockLength.Unbounded;
                continue;
            }

            var concreteSize = block.CalcSize(bufferSize);

            colWidths[col] = bounds.Horizontal == Bounding.Bounded
                ? BlockLength.Max(colWidths[col], concreteSize.Width)
                : BlockLength.Unbounded;

            rowHeights[row] = bounds.Vertical == Bounding.Bounded
                ? BlockLength.Max(rowHeights[row], concreteSize.Height)
                : BlockLength.Unbounded;

            if (bounds.IsBounded) boundedSizes[row, col] = concreteSize;
        }
    }

    /// <summary>Does the final arrangement of this grid's children based on the size of each child and the
    /// computed column widths and row heights.</summary>
    private static BlockSize ArrangeChildren(
        Span2D<UiBlock?> contents,
        BlockSize bufferSize,
        Padding padding,
        ref Span2D<BufferSlice?> childArrangement,
        scoped Span2D<BlockSize?> boundedSizes,
        scoped Span<BlockLength> rowHeights,
        scoped Span<BlockLength> colWidths,
        IReadOnlyList<Index> colAccents,
        IReadOnlyList<Index> rowAccents,
        scoped Span<int> colGaps,
        scoped Span<int> rowGaps
    )
    {
        var writeArrangement = !childArrangement.IsEmpty;
        var numRows = contents.Height;
        var numCols = contents.Width;
        var xPos = padding.Left;
        var yPos = padding.Top;
        var rightLimit = bufferSize.Width - padding.Right;
        var bottomLimit = bufferSize.Height - padding.Bottom;
        var maxRow = 0;
        var maxCol = 0;
        for (int row = 0; row < numRows; row++)
        {
            var rowHeight = rowHeights[row].Value ??
                throw new UnreachableException("All rowHeights have been allocated by now");
            var rowGap = (row < numRows - 1) ? rowGaps[row] : 0;
            if (yPos + rowHeight + rowGap > bottomLimit) break;

            xPos = padding.Left;
            for (int col = 0; col < numCols; col++)
            {
                var colWidth = colWidths[col].Value ??
                    throw new UnreachableException("All colWidths have been allocated by now");
                var colGap = (col < numCols - 1) ? colGaps[col] : 0;
                if (xPos + colWidth + colGap > rightLimit)
                {
                    numCols = col + 1;
                    break;
                }

                if (writeArrangement)
                {
                    var size = boundedSizes[row, col];
                    var width = size?.Width ?? colWidth;
                    var height = size?.Height ?? rowHeight;
                    childArrangement[row, col] = new BufferSlice(xPos, yPos, width, height);
                }

                maxCol = Math.Max(maxCol, col);
                xPos += colWidth + colGap;
            }

            maxRow = Math.Max(maxRow, row);
            yPos += rowHeight + rowGap;
        }

        if (writeArrangement) childArrangement = childArrangement.Slice(0, 0, maxRow + 1, maxCol + 1);

        return BlockSize.From(xPos + padding.Right, yPos + padding.Bottom);
    }

    /// <summary>Proportionally distributes remaining length among unbounded cells.</summary>
    private static void AllocateRemainingLength(
        int totalLength,
        scoped Span<BlockLength> cellLengths,
        scoped Span<int> cellGaps
    )
    {
        var remainingUnallocatedLength = totalLength;
        var remainingUnallocatedCellCount = 0;
        var cellCount = cellLengths.Length;
        var lastIndex = cellCount - 1;
        for (var i = 0; i < cellCount; i++)
        {
            var len = cellLengths[i];

            if (len.IsBounded) remainingUnallocatedLength -= len.Value.Value;
            else               remainingUnallocatedCellCount++;

            if (i < lastIndex) remainingUnallocatedLength -= cellGaps[i];
        }

        for (int i = 0; remainingUnallocatedCellCount > 0 && i < cellCount; i++)
        {
            if (cellLengths[i].IsBounded) continue;

            if (remainingUnallocatedLength <= 0)
            {
                cellLengths[i] = 0;
                continue;
            }

            var availableLength = (int)Math.Ceiling((float)remainingUnallocatedLength / remainingUnallocatedCellCount);
            cellLengths[i] = availableLength;
            remainingUnallocatedLength -= availableLength;
            remainingUnallocatedCellCount--;
        }
    }

    private static void CalculateGaps(
        IBorder border,
        IEnumerable<Index> colAccents,
        IEnumerable<Index> rowAccents,
        scoped Span<int> colGaps,
        scoped Span<int> rowGaps
    )
    {
        var vGap       = (border.InnerEdge(BorderInnerEdge.Vertical) is not null) ? 1 : 0;
        var hGap       = (border.InnerEdge(BorderInnerEdge.Horizontal) is not null) ? 1 : 0;
        var accentVGap = (border.InnerEdge(BorderInnerEdge.Vertical,   BorderAccent.Accent) is not null) ? 1 : 0;
        var accentHGap = (border.InnerEdge(BorderInnerEdge.Horizontal, BorderAccent.Accent) is not null) ? 1 : 0;

        colGaps.Fill(vGap);
        rowGaps.Fill(hGap);

        foreach (var col in colAccents) colGaps[col] = accentVGap;
        foreach (var row in rowAccents) rowGaps[row] = accentHGap;
    }
}
