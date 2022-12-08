using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

/// <summary>Base class of UI components</summary>
public abstract class UiBlock
{
    /// <summary>Returns whether this block's desired size should be recomputed.</summary>
    public virtual bool ShouldRerenderWithChildren { get; } = false;

    /// <summary>
    /// The container to which this Block belongs. Should be used to <see cref="IBlockContainer.RenderChild"/>
    /// Blocks in <see cref="Render"/>, or to <see cref="IBlockContainer.RequestRerender" />
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

    /// <summary>Sends a request for this Block's container to rerender it.</summary>
    /// <exception cref="UnattachedUiBlockException">
    ///     Thrown if this method is called prior to the initial rendering of this block by a <see cref="IBlockContainer"/>.
    /// </exception>
    protected void RequestRerender()
    {
        if (Container is null)
        {
            var typeName = GetType().Name;
            throw new UnattachedUiBlockException(
                $"{nameof(UiBlock)} of type {typeName} cannot request a rerender without a Container. " +
                $"This can occur if {nameof(RequestRerender)} is called prior to the block initially being rendered, " +
                "or if the block was rendered outside the context of a container."
            );
        }

        Container.RequestRerender(this);
    }

    /// <summary>Renders the given child block to the given buffer via this Block's container.</summary>
    /// <exception cref="UnattachedUiBlockException">
    ///     Thrown if this method is called prior to the initial rendering of this block by a <see cref="IBlockContainer"/>.
    /// </exception>
    protected void RenderChild(UiBlock child, Span2D<char> childBuffer)
    {
        if (Container is null)
        {
            var typeName = GetType().Name;
            throw new UnattachedUiBlockException(
                $"{nameof(UiBlock)} of type {typeName} cannot render a child block without a Container. " +
                $"This can occur if {nameof(RenderChild)} is called prior to the block initially being rendered, " +
                "or if the block was rendered outside the context of a container."
            );
        }

        Container.RenderChild(this, child, childBuffer);
    }
}

/// <summary>
/// Thrown when attempting to use a UiBlock's <see cref="UiBlock.Container"/> before a render has been initiated by the
/// container.
/// </summary>
public class UnattachedUiBlockException : Exception
{
    public UnattachedUiBlockException(string message) : base(message) { }
    public UnattachedUiBlockException(string message, Exception inner) : base(message, inner) { }
}
