using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

/// <summary>
/// Responsible for holding the render buffer for a FlexBlocks application and managing rendering Blocks to that buffer.
/// </summary>
internal class BlockRenderer : IBlockContainer
{
    public readonly int Width;
    public readonly int Height;

    /// <summary>The root render buffer. Blocks belonging to this BlockRenderer render to slices of this buffer</summary>
    private readonly char[] _rootBuffer;

    /// <summary>Stores data necessary to rerender each block after its initial rendering. The entry for each block is
    /// updated each time it is rendered.</summary>
    private readonly ConditionalWeakTable<UiBlock, BlockRenderInfo> _blocks = new();

    private class BlockRenderInfo
    {
        public UiBlock? Parent { get; set; }
        public BufferSlice BufferSlice { get; set; }
    }

    /// <summary>Stores information about the slice of the main render buffer that a particular block occupies.</summary>
    private record struct BufferSlice(int XOffset, int YOffset, int Width, int Height);

    public BlockRenderer(int width, int height)
    {
        Width = width;
        Height = height;
        var bufferSize = Width * Height;
        _rootBuffer = new char[bufferSize];
        Array.Fill(_rootBuffer, ' '); // \0 prints as zero-width in some terminals, so fill with a space
    }

    /// <summary>Writes the render buffer to the console</summary>
    private void Blit()
    {
        if (Width == Math.Min(Console.WindowHeight, Console.BufferHeight))
        {
            // buffer width matches the console window width, no need to manually wrap lines
            Console.SetCursorPosition(0, 0);
            Console.Out.Write(_rootBuffer);
        }
        else
        {
            // buffer width is different than the console window width.
            // if we don't manually wrap, it will show more than a single line of our buffer per console line
            for (int row = 0; row < Height; row++)
            {
                Console.SetCursorPosition(0, row);
                Console.Out.Write(_rootBuffer.AsSpan((row * Width), Width));
            }
        }

        Console.SetCursorPosition(0, 0);
    }

    /// <summary>Returns a span that represents the render buffer as a rectangle of Height and Width.</summary>
    private Span2D<char> GetRectBuffer() => new(_rootBuffer, Height, Width);

    /// <summary>Uses a BufferSlice to slice a 2d buffer.</summary>
    private Span2D<char> SliceBuffer(Span2D<char> buffer, BufferSlice slice) =>
        buffer.Slice(slice.YOffset, slice.XOffset, slice.Height, slice.Width);

    /// <summary>Renders the entire Block hierarchy to the render buffer,
    /// then blits the render buffer to the console.</summary>
    public void RenderRoot(UiBlock root)
    {
        var buffer = GetRectBuffer();
        var rootDesiredSize = root.CalcDesiredSize(buffer.BlockSize());
        var rootBuffer = buffer.Slice(
            row: 0,
            column: 0,
            height: rootDesiredSize.Height,
            width: rootDesiredSize.Width
        );
        InnerRenderChild(null, root, rootBuffer);

        Blit();
    }

    /// <summary>Renders a child block (with optional parent block) to the given buffer.</summary>
    /// <remarks>
    /// BlockRenderer itself needs to be able to render a root block without a parent,
    /// which is why this method is private. The publicly accessible implementation of
    /// <see cref="IBlockContainer.RenderChild"/> in this class requires a non-null parent.
    /// </remarks>
    private void InnerRenderChild(UiBlock? parent, UiBlock child, Span2D<char> childBuffer)
    {
        WriteBlockRenderInfo(parent, child, childBuffer);

        child.Container = this;
        child.Render(childBuffer);
    }

    /// <summary>Stores necessary render data for each block in this container,
    /// including information about the slice of the full render buffer that this child renders to.</summary>
    /// <seealso cref="_blocks"/>
    private unsafe void WriteBlockRenderInfo(UiBlock? parent, UiBlock child, Span2D<char> childBuffer)
    {
        BufferSlice bufferSlice;
        fixed (char* rootPointer = _rootBuffer)
        fixed (char* childOriginPointer = childBuffer)
        {
            var bufferOffset = (int)(childOriginPointer - rootPointer);
            var xOffset = bufferOffset % Width;
            var yOffset = bufferOffset / Width;

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
    /// Thrown if the given block has not ever been rendered by this BLockRenderer.
    /// </exception>
    private BlockRenderInfo GetBlockRenderInfo(UiBlock block)
    {
        if (block.Container == this && _blocks.TryGetValue(block, out var renderInfo)) return renderInfo;

        var typeName = block.GetType().Name;
        throw new UnattachedUiBlockException(
            $"{nameof(UiBlock)} of type {typeName} cannot be rerendered because it has not been rendered by this container."
        );
    }

    // IBlockContainer Implementation

    /// <inheritdoc />
    public void RenderChild(UiBlock parent, UiBlock child, Span2D<char> childBuffer) =>
        InnerRenderChild(parent, child, childBuffer);

    /// <inheritdoc />
    public void RequestRerender(UiBlock block, RerenderMode rerenderMode = RerenderMode.InPlace)
    {
        switch (rerenderMode)
        {
            case RerenderMode.InPlace:
            {
                RerenderBlock(block);
                break;
            }
            case RerenderMode.DesiredSizeChanged:
            {
                var renderInfo = GetBlockRenderInfo(block);
                var parent = renderInfo.Parent;
                if (parent is null)
                {
                    RenderRoot(block);
                }
                else
                {
                    var parentRerenderMode = parent.GetRerenderModeForChild(block);
                    RequestRerender(parent, parentRerenderMode);
                }

                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(rerenderMode), rerenderMode, null);
        }
    }

    /// <summary>Rerenders a block to the same buffer slice it was last rendered to.</summary>
    private void RerenderBlock(UiBlock block)
    {
        var renderInfo = GetBlockRenderInfo(block);
        var fullBuffer = GetRectBuffer();
        var slicedBuffer = SliceBuffer(fullBuffer, renderInfo.BufferSlice);
        slicedBuffer.Fill(' ');

        InnerRenderChild(renderInfo.Parent, block, slicedBuffer);

        Blit();
    }
}
