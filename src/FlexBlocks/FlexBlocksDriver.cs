using System.Text;
using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;
using Nito.AsyncEx;
using Timer = System.Timers.Timer;

namespace FlexBlocks;

/// <summary>The entrypoint of the FlexBlocks application.</summary>
public sealed class FlexBlocksDriver
{
    /// <summary>Optional custom max width</summary>
    private readonly int? _maxWidth;
    /// <summary>Optional custom max height</summary>
    private readonly int? _maxHeight;

    /// <summary>The width of the current render buffer.</summary>
    public int Width { get; private set; }

    /// <summary>The height of the current render buffer.</summary>
    public int Height { get; private set; }

    private readonly BlockRenderInfoCache _renderInfoCache = new();

    /// <summary>Buffer to which blocks are rendered before being blitted to the console window.</summary>
    private char[] _renderBuffer;

    /// <summary>A 2D representation of the flat render buffer.</summary>
    private Span2D<char> RenderBuffer2D => new(_renderBuffer, Height, Width);

    /// <summary>Responsible for rendering blocks to the console and maintaining
    /// their hierarchy for the purposes of re-rendering later.</summary>
    private readonly BlockRenderer _blockRenderer;

    /// <summary>
    /// Creates a new flex blocks driver.
    /// If no maximum dimensions are defined, the root block will fill the console window.
    /// </summary>
    /// <param name="rootBlock">The top level ui block that will be rendered when this driver runs.</param>
    /// <param name="maxWidth">Optional maximum render buffer width.</param>
    /// <param name="maxHeight">Optional maximum render buffer height.</param>
    public FlexBlocksDriver(UiBlock rootBlock, int? maxWidth = default, int? maxHeight = default)
    {
        _maxWidth = maxWidth;
        _maxHeight = maxHeight;
        (Width, Height) = ComputeBufferSize();
        _renderBuffer = Array.Empty<char>();
        AllocateBuffer();
        _blockRenderer = new BlockRenderer(this, rootBlock);
    }

    /// <summary>Allocates the render buffer based on the available console space</summary>
    private (int width, int height) ComputeBufferSize()
    {
        var width = Math.Min(Console.BufferWidth, Console.WindowWidth);
        if (width > _maxWidth)
        {
            width = _maxWidth.Value;
        }

        var height = Math.Min(Console.BufferHeight, Console.WindowHeight);
        if (height > _maxHeight)
        {
            height = _maxHeight.Value;
        }

        return (width, height);
    }

    /// <summary>Allocates a new render buffer using the Width and Height.</summary>
    private void AllocateBuffer() { _renderBuffer = new char[Width * Height]; }

    /// <summary>Keeps track of whether the BlockRenderer is currently consuming the render queue.</summary>
    private bool _isConsumingRenderQueue;

    /// <summary>
    /// A count of the number of times a frame was skipped because the previous frame was not finished rendering.
    /// </summary>
    internal int DroppedFrames { get; private set; }

    /// <summary>Renders the contents of the block renderer to the screen.</summary>
    /// <param name="forceRerender">If true, the full block hierarchy will be rendered, even if a rerender wasn't requested.</param>
    /// <param name="token">Cancellation token for the render operation.</param>
    private void Render(bool forceRerender, CancellationToken token)
    {
        // Prevent reentrant calls of ConsumeRenderQueue. We do not want to have multiple simultaneous
        // instances of this method running and modifying the buffer simultaneously, so if a new frame request comes in
        // while we're in the process of rendering a previous frame, just drop the incoming frame request.
        if (_isConsumingRenderQueue)
        {
            DroppedFrames++;
            return;
        }

        _isConsumingRenderQueue = true;

        try
        {
            var fullRerender = forceRerender;
            var (width, height) = ComputeBufferSize();
            if (width != Width || height != Height)
            {
                Width = width;
                Height = height;
                AllocateBuffer();
                fullRerender = true;
            }

            _blockRenderer.RenderFrame(RenderBuffer2D, fullRerender, token);

            Blit();
        }
        finally
        {
            _isConsumingRenderQueue = false;
        }
    }

    /// <summary>Writes the render buffer to the console</summary>
    private void Blit()
    {
        if (Width == Console.BufferWidth)
        {
            // buffer width matches the console window width, no need to manually wrap lines
            Console.SetCursorPosition(0, 0);
            Console.Out.Write(_renderBuffer);
        }
        else
        {
            // buffer width is different than the console window width.
            // if we don't manually wrap, it will show more than a single line of our buffer per console line
            for (int row = 0; row < Height; row++)
            {
                Console.SetCursorPosition(0, row);
                Console.Out.Write(_renderBuffer.AsSpan((row * Width), Width));
            }
        }

        Console.SetCursorPosition(0, 0);
    }

    internal void SetBlockRenderInfo(UiBlock? parent, UiBlock child, Span2D<char> childBuffer) =>
        _renderInfoCache.SetBlockRenderInfo(parent, child, RenderBuffer2D, childBuffer);

    internal BlockRenderInfo GetBlockRenderInfo(UiBlock block) => _renderInfoCache.GetBlockRenderInfo(block);

    /// <summary>Starts the flex blocks driver.</summary>
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
            Console.OutputEncoding = Encoding.Unicode;

            // on the off chance that the quitToken is canceled before now,
            // quit before bothering to do the work of rendering the first frame.
            quitTokenSource.Token.ThrowIfCancellationRequested();

            // initial render. subsequent renders will be initiated from within the block hierarchy using RequestRerender
            Render(true, quitTokenSource.Token);

            // rerender the whole screen when a hot reload was detected.
            HotReloader.OnHotReloaded += () => Render(true, quitTokenSource.Token);

            // render loop
            var timer = new Timer(TimeSpan.FromMilliseconds(10));
            timer.Elapsed += (_, _) => Render(false, quitTokenSource.Token);
            timer.Start();

            // yield thread until program is quit (either externally via quitToken or via user-initiated kill signal)
            using var cancelTaskSource = new CancellationTokenTaskSource<object>(quitTokenSource.Token);
            await cancelTaskSource.Task.ConfigureAwait(true);
            timer.Stop();
        }
        catch (TaskCanceledException)
        {
            // quit silently
        }
        finally
        {
            // return the console state to something resembling normalcy
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, Height - 1);
            Console.WriteLine();
        }
    }

    /// <summary>Starts the flex blocks driver.</summary>
    /// <returns>A task the completes when the driver has quit.</returns>
    public Task Run() => Run(CancellationToken.None);

    /// <summary>Starts up a fresh flex blocks driver with the given root block.</summary>
    /// <param name="rootBlock">The root block of this driver.</param>
    /// <param name="quitToken">Quits the program when canceled.</param>
    /// <returns>A task the completes when the driver has quit.</returns>
    public static async Task Run(UiBlock rootBlock, CancellationToken quitToken)
    {
        var driver = new FlexBlocksDriver(rootBlock);
        await driver.Run(quitToken);
    }

    /// <summary>Starts up a fresh flex blocks driver with the given root block.</summary>
    /// <param name="rootBlock">The root block of this driver.</param>
    /// <returns>A task the completes when the driver has quit.</returns>
    public static Task Run(UiBlock rootBlock) => Run(rootBlock, CancellationToken.None);
}
