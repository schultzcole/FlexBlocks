using Nito.AsyncEx;

namespace FlexBlocks.Blocks;

public sealed class FlexBlocksDriver
{
    /// The top level ui block
    private readonly UiBlock _rootBlock;

    /// Optional custom max width
    private readonly int? _maxWidth;
    /// Optional custom max height
    private readonly int? _maxHeight;

    private readonly BlockRenderer _blockRenderer;

    /// Creates a new flex blocks driver.
    /// If no maximum dimensions are defined, the root block will fill the console window.
    /// <param name="rootBlock">The top level ui block that will be rendered when this driver runs.</param>
    /// <param name="maxWidth">Optional maximum render buffer width.</param>
    /// <param name="maxHeight">Optional maximum render buffer height.</param>
    public FlexBlocksDriver(UiBlock rootBlock, int? maxWidth = default, int? maxHeight = default)
    {
        _rootBlock = rootBlock;
        _maxWidth = maxWidth;
        _maxHeight = maxHeight;
        var (width, height) = ComputeBufferSize(_maxWidth, _maxHeight);
        _blockRenderer = new BlockRenderer(width, height);
    }

    /// Allocates the render buffer based on the available console space
    private (int width, int height) ComputeBufferSize(int? maxWidth, int? maxHeight)
    {
        var width = Math.Min(Console.BufferWidth, Console.WindowWidth);
        if (_maxWidth is not null && width > maxWidth)
        {
            width = maxWidth.Value;
        }
        var height = Math.Min(Console.BufferHeight, Console.WindowHeight);
        if (_maxHeight is not null && height > maxHeight)
        {
            height = maxHeight.Value;
        }

        return (width, height);
    }


    /// Starts the flex blocks driver.
    /// <param name="quitToken">Quits the program when canceled.</param>
    /// <returns>A task the completes when the driver has quit.</returns>
    public async Task Run(CancellationToken quitToken)
    {
        try
        {
            // token source used to cancel if user sends a kill signal
            var quitTokenSource = CancellationTokenSource.CreateLinkedTokenSource(quitToken);

            // if user sends kill signal (Ctrl+C), close gracefully
            Console.CancelKeyPress += (_, evt) =>
            {
                evt.Cancel = true;
                quitTokenSource.Cancel();
            };

            Console.Clear();
            Console.CursorVisible = false;

            // on the off chance that the quitToken is canceled before now, quit before bothering to do the work of rendering the block
            quitTokenSource.Token.ThrowIfCancellationRequested();

            // initial render
            _blockRenderer.RenderRoot(_rootBlock);

            // yield thread until program is quit (either externally via quitToken or via user-initiated kill signal)
            using var cancelTaskSource = new CancellationTokenTaskSource<object>(quitTokenSource.Token);
            await cancelTaskSource.Task.ConfigureAwait(true);
        }
        catch (TaskCanceledException)
        {
            // quit silently
        }
        finally
        {
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, _blockRenderer.Height - 1);
            Console.WriteLine();
        }
    }

    /// Starts the flex blocks driver.
    /// <returns>A task the completes when the driver has quit.</returns>
    public Task Run() => Run(CancellationToken.None);

    /// Starts up a fresh flex blocks driver with the given root block.
    /// <param name="rootBlock">The root block of this driver.</param>
    /// <param name="quitToken">Quits the program when canceled.</param>
    /// <returns>A task the completes when the driver has quit.</returns>
    public static async Task Run(UiBlock rootBlock, CancellationToken quitToken)
    {
        var driver = new FlexBlocksDriver(rootBlock);
        await driver.Run(quitToken);
    }

    /// Starts up a fresh flex blocks driver with the given root block.
    /// <param name="rootBlock">The root block of this driver.</param>
    /// <returns>A task the completes when the driver has quit.</returns>
    public static Task Run(UiBlock rootBlock) => Run(rootBlock, CancellationToken.None);
}
