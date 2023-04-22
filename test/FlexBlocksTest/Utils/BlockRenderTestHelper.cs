using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;

namespace FlexBlocksTest.Utils;

public static class BlockRenderTestHelper
{
    private const char BLANK = '×';

    /// <summary>
    /// Creates a render buffer of the given size and renders the given block into the buffer.
    /// </summary>
    /// <returns>Returns the render buffer array that the block was rendered into.</returns>
    public static char[,] RenderBlock(UiBlock block, int width, int height)
    {
        var buffer = new char[height, width];
        var bufferSpan = buffer.AsSpan2D();
        bufferSpan.Fill(BLANK);

        var rootBlock = new FixedSizeBlock { Background = null, Size = UnboundedBlockSize.From(width, height), Content = block };

        var container = new SimpleBlockContainer();
        container.RenderBlock(rootBlock, bufferSpan);

        return buffer;
    }
}
