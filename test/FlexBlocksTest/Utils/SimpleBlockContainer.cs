using CommunityToolkit.HighPerformance;
using FlexBlocks;
using FlexBlocks.Blocks;

namespace FlexBlocksTest.Utils;

public class SimpleBlockContainer : IBlockContainer
{
    public void RenderBlock(UiBlock block, Span2D<char> buffer)
    {
        block.Container = this;
        block.Background?.Render(buffer);
        block.Render(buffer);
        block.Overlay?.Render(buffer);
    }

    /// <inheritdoc />
    public void RenderChild(UiBlock parent, UiBlock child, Span2D<char> childBuffer) => RenderBlock(child, childBuffer);

    /// <inheritdoc />
    public void RequestRerender(UiBlock block, RerenderMode rerenderMode = RerenderMode.InPlace) =>
        throw new System.NotImplementedException();
}
