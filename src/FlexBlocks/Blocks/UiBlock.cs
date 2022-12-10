using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using FlexBlocks.Renderables;

namespace FlexBlocks.Blocks;

/// <summary>Base class of UI components</summary>
public abstract class UiBlock
{
    /// <summary>
    /// A pattern that is rendered into this block's buffer prior to rendering this block.
    /// This is especially important for blocks whose layout may change, as without a background,
    /// areas that are not explicitly overwritten will retain their previous content.
    /// </summary>
    public Pattern? Background { get; set; } = Patterns.BlankPattern;

    /// <summary>
    /// A renderable component that gets rendered on top of this block with no influence over layout.
    /// </summary>
    /// <remarks>
    /// As a general rule, the overlay should be used for non-critical ui components such as debug
    /// information. There may of course be exceptions to this rule.
    /// </remarks>
    public IRenderable? Overlay { get; set; }

    /// <summary>
    /// The container to which this Block belongs. Should be used to <see cref="IBlockContainer.RenderChild"/>
    /// Blocks in <see cref="Render"/>, or to <see cref="IBlockContainer.RequestRerender" />.
    /// </summary>
    public IBlockContainer? Container { get; internal set; }

    /// <summary>Computes the size in columns and rows that this block will take up when rendered.</summary>
    /// <remarks>This is used to determine the size of the buffer that will be passed to <see cref="Render"/>.</remarks>
    /// <param name="maxSize">The maximum space available in which to render this Block.</param>
    public abstract BlockSize CalcDesiredSize(BlockSize maxSize);

    /// <summary>Renders this Block to a given render buffer.</summary>
    /// <remarks>This method should not be called directly in user code in most cases. Most often, it gets called by the
    /// <see cref="IBlockContainer"/> that contains this block.</remarks>
    /// <param name="buffer">The buffer to render to. This buffer represents the full extent of the console window
    /// available to render this Block to.</param>
    public abstract void Render(Span2D<char> buffer);

    /// <summary>
    /// Returns the rerender mode that should be used to rerender this UiBlock, given that
    /// the given child block is being rerendered with <see cref="RerenderMode.DesiredSizeChanged"/>.
    /// </summary>
    public virtual RerenderMode GetRerenderModeForChild(UiBlock child) => RerenderMode.InPlace;

    /// <summary>Sends a request for this Block's container to rerender it.</summary>
    /// <exception cref="UnattachedUiBlockException">
    ///     Thrown if this method is called prior to the initial rendering of this block by a <see cref="IBlockContainer"/>.
    /// </exception>
    protected void RequestRerender(RerenderMode rerenderMode = RerenderMode.InPlace)
    {
        EnsureContainer();

        Container.RequestRerender(this, rerenderMode);
    }

    /// <summary>Renders the given child block to the given buffer via this Block's container.</summary>
    /// <exception cref="UnattachedUiBlockException">
    ///     Thrown if this method is called prior to the initial rendering of this block by a <see cref="IBlockContainer"/>.
    /// </exception>
    protected void RenderChild(UiBlock child, Span2D<char> childBuffer)
    {
        EnsureContainer();

        Container.RenderChild(this, child, childBuffer);
    }

    /// <summary>Ensures that <see cref="Container"/> has been set prior to a call that requires it.</summary>
    [MemberNotNull(nameof(Container))]
    protected void EnsureContainer([CallerMemberName] string? caller = null)
    {
        if (Container is not null) return;

        var typeName = GetType().Name;
        throw new UnattachedUiBlockException(
            $"{nameof(UiBlock)} of type {typeName} cannot {caller} without a Container. " +
            $"This can occur if {caller} is called prior to the block initially being rendered, " +
            "or if the block was rendered outside the context of a container."
        );
    }
}
