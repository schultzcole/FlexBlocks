using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

/// <summary>
/// Determines what will be rerendered
/// </summary>
public enum RerenderMode
{
    /// <summary>The block will be rerendered into the same slice of the console buffer as the last time it was rendered.</summary>
    InPlace,
    /// <summary>The slice of the console buffer to which this block is rendered will be recalculated. May cascade re-renders up the block hierarchy.</summary>
    DesiredSizeChanged
}

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
    /// <param name="block">The UiBlock to be rerendered. This block should belong to this BlockContainer.</param>
    /// <param name="rerenderMode">
    ///   Determines the extent of what should be rerendered.
    ///   <list type="bullet">
    ///     <item>
    ///       <term><see cref="RerenderMode.InPlace"/>: </term>
    ///       <description>the block will be immediately re-rendered into the same buffer slice it was previously rendered to.</description>
    ///     </item>
    ///     <item>
    ///       <term><see cref="RerenderMode.DesiredSizeChanged"/>: </term>
    ///       <description>the block's parent will be rerendered as well, possibly cascading up the block hierarchy.</description>
    ///     </item>
    ///   </list>
    /// </param>
    public void RequestRerender(UiBlock block, RerenderMode rerenderMode = RerenderMode.InPlace);
}
