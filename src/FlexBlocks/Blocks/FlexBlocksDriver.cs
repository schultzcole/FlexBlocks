using CommunityToolkit.HighPerformance;
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

    /// Width of the render buffer when represented in 2d
    private int _width;
    /// Height of the render buffer when represented in 2d
    private int _height;

    /// The render buffer. Blocks render to this buffer and then the driver blits the buffer contents to the console window
    private char[] _1dBuffer;

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
        _1dBuffer = Array.Empty<char>();
        CreateBuffer();
    }

    /// Allocates the render buffer based on the available console space
    private void CreateBuffer()
    {
        _width = Math.Min(Console.BufferWidth, Console.WindowWidth);
        if (_maxWidth is { } maxWidth && _width > maxWidth)
        {
            _width = maxWidth;
        }
        _height = Math.Min(Console.BufferHeight, Console.WindowHeight);
        if (_maxHeight is { } maxHeight && _height > maxHeight)
        {
            _height = maxHeight;
        }
        var bufferSize = _width * _height;
        _1dBuffer = new char[bufferSize];
        Array.Fill(_1dBuffer, ' '); // \0 prints as zero-width in some terminals, so fill with a space
    }

    /// Renders the root block to the render buffer
    private void RenderRoot()
    {
        var buffer = Span2D<char>.DangerousCreate(ref _1dBuffer.DangerousGetReference(), _height, _width, 0);
        var rootDesiredSize = _rootBlock.CalcDesiredSize(buffer.BlockSize());
        var rootBuffer = buffer.Slice(
            row: 0,
            column: 0,
            height: rootDesiredSize.Height,
            width: rootDesiredSize.Width
        );
        _rootBlock.Render(rootBuffer);

        Blit();
    }

    /// Writes the render buffer to the console
    private void Blit()
    {
        if (_width == Math.Min(Console.WindowHeight, Console.BufferHeight))
        {
            // buffer width matches the console window width, no need to manually wrap lines
            Console.SetCursorPosition(0, 0);
            Console.Out.Write(_1dBuffer);
        }
        else
        {
            // buffer width is different than the console window width.
            // if we don't manually wrap, it will show more than a single line of our buffer per console line
            for (int row = 0; row < _height; row++)
            {
                Console.SetCursorPosition(0, row);
                Console.Out.Write(_1dBuffer.AsSpan((row * _width), _width));
            }
        }
        Console.SetCursorPosition(0, 0);
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
            RenderRoot();

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
            Console.SetCursorPosition(0, _height - 1);
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
