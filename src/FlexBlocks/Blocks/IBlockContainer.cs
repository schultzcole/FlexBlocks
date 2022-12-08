using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

/// <summary>
/// The top level container that handles rendering the Block hierarchy.
/// </summary>
public interface IBlockContainer
{
    /// <summary>
    /// Renders the given child block to the given buffer,
    /// registering the link between parent and child in this container.
    /// </summary>
    public void RenderChild(UiBlock parent, UiBlock child, Span2D<char> childBuffer);

    /// <summary>
    /// Requests for the given block to be rerendered.
    /// </summary>
    public void RequestRerender(UiBlock block);
}
