using System.Collections.Immutable;
using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks;

/// <summary>
/// Responsible for holding the render buffer for a FlexBlocks application and managing rendering Blocks to that buffer.
/// </summary>
internal class BlockRenderer : IBlockContainer
{
    private const bool RETHROW_RENDER_ERRORS = true;

    private readonly FlexBlocksDriver _driver;
    private readonly RenderQueue _renderQueue = new();

    public UiBlock RootBlock { get; }

    public BlockRenderer(FlexBlocksDriver driver, UiBlock rootBlock)
    {
        _driver = driver;
        RootBlock = rootBlock;
    }

    /// <summary>Renders a child block (with optional parent block) to the given buffer.</summary>
    /// <remarks>
    /// BlockRenderer itself needs to be able to render a root block without a parent,
    /// which is why this method is private. The publicly accessible implementation of
    /// <see cref="IBlockContainer.RenderChild"/> in this class requires a non-null parent.
    /// </remarks>
    private void RenderBlock(UiBlock? parent, UiBlock child, Span2D<char> childBuffer)
    {
        _driver.SetBlockRenderInfo(parent, child, childBuffer);

        child.Container = this;

        try { child.Background?.Render(childBuffer); }
        catch { if (RETHROW_RENDER_ERRORS) throw; }

        try { child.Render(childBuffer); }
        catch { if (RETHROW_RENDER_ERRORS) throw; }

        try { child.Overlay?.Render(childBuffer); }
        catch { if (RETHROW_RENDER_ERRORS) throw; }
    }

    /// <summary>Renders the root block to the given buffer, thereby rendering the whole block hierarchy.</summary>
    private void RenderRoot(Span2D<char> buffer) => RenderBlock(null, RootBlock, buffer);

    /// <summary>Renders a frame</summary>
    public void RenderFrame(Span2D<char> buffer, bool fullRerender, CancellationToken token)
    {
        if (fullRerender)
        {
            _renderQueue.Clear();
            RenderRoot(buffer);
            return;
        }

        var queue = _renderQueue.Consume();
        foreach (var block in queue)
        {
            var renderInfo = _driver.GetBlockRenderInfo(block);
            var parent = renderInfo.Parent;

            if (parent is null)
            {
                token.ThrowIfCancellationRequested();
                RenderRoot(buffer);
            }
            else
            {
                if (SetContainsAncestor(parent, queue)) continue;

                var slice = renderInfo.BufferSlice;
                var slicedBuffer = buffer.Slice(slice.Row, slice.Column, slice.Height, slice.Width);
                token.ThrowIfCancellationRequested();
                RenderBlock(renderInfo.Parent, block, slicedBuffer);
            }
        }
    }

    /// <summary>Returns whether the given set contains any ancestor of the given block (including the given block).</summary>
    private bool SetContainsAncestor(UiBlock block, ImmutableHashSet<UiBlock> set)
    {
        UiBlock? parent = block;

        // Search up through this block's ancestors. If one of its ancestors is also in the queue, skip this block.
        while (parent is not null)
        {
            if (set.Contains(parent))
            {
                return true;
            }

            parent = _driver.GetBlockRenderInfo(parent).Parent;
        }

        return false;
    }


    // IBlockContainer Implementation

    /// <inheritdoc />
    public void RenderChild(UiBlock parent, UiBlock child, Span2D<char> childBuffer) =>
        RenderBlock(parent, child, childBuffer);

    /// <inheritdoc />
    public void RequestRerender(UiBlock block, RerenderMode rerenderMode = RerenderMode.InPlace)
    {
        while (rerenderMode != RerenderMode.InPlace)
        {
            var renderInfo = _driver.GetBlockRenderInfo(block);
            var parent = renderInfo.Parent;

            if (parent is null) break;

            rerenderMode = parent.GetRerenderModeForChild(block);
            block = parent;
        }


        _renderQueue.EnqueueBlock(block);
    }
}
