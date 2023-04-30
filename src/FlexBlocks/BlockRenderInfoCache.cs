using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks;

/// <summary>
/// Stores render information about UiBlocks. This is used when rerendering a block to determine what slice of the
/// screen buffer to render to.
/// </summary>
internal class BlockRenderInfoCache
{
    /// <summary>
    /// Stores data necessary to rerender each block after its initial rendering. The entry for each block is
    /// updated each time it is rendered.
    /// </summary>
    private readonly ConditionalWeakTable<UiBlock, BlockRenderInfo> _blocks = new();


    /// <summary>
    /// Stores necessary render data for each block in this container,
    /// including information about the slice of the full render buffer that this child renders to.
    /// </summary>
    public unsafe void SetBlockRenderInfo(UiBlock? parent, UiBlock child, Span2D<char> rootBuffer, Span2D<char> childBuffer)
    {
        BufferSlice bufferSlice;
        fixed (char* rootPointer = rootBuffer)
        fixed (char* childOriginPointer = childBuffer)
        {
            var bufferOffset = (int)(childOriginPointer - rootPointer);
            var xOffset = bufferOffset % rootBuffer.Width;
            var yOffset = bufferOffset / rootBuffer.Width;

            bufferSlice = new BufferSlice(xOffset, yOffset, childBuffer.Width, childBuffer.Height);
        }

        if (_blocks.TryGetValue(child, out var existingRenderInfo))
        {
            existingRenderInfo.Parent = parent;
            existingRenderInfo.BufferSlice = bufferSlice;
        }
        else
        {
            _blocks.Add(child, new BlockRenderInfo { Parent = parent, BufferSlice = bufferSlice });
        }
    }


    /// <summary>Given a UiBlock, gets the previously stored render info data for that block.</summary>
    /// <exception cref="UnattachedUiBlockException">
    /// Thrown if the given block does not exist in the cache.
    /// </exception>
    public BlockRenderInfo GetBlockRenderInfo(UiBlock block)
    {
        if (_blocks.TryGetValue(block, out var renderInfo)) return renderInfo;

        var typeName = block.GetType().Name;
        throw new UnattachedUiBlockException(
            $"{nameof(UiBlock)} of type {typeName} cannot be rerendered because it has not been rendered by this container."
        );
    }
}

internal class BlockRenderInfo
{
    public UiBlock? Parent { get; set; }
    public required BufferSlice BufferSlice { get; set; }
}
