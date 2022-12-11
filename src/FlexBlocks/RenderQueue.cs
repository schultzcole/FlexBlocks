using System.Runtime.CompilerServices;
using FlexBlocks.Blocks;
using Nito.AsyncEx;

namespace FlexBlocks;

internal sealed class RenderQueue : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    /// <summary>A collection of blocks that should be re-rendered on the next "frame".</summary>
    /// <remarks>This table is functionally used as a set; it does not store actual values for each key.</remarks>
    private readonly ConditionalWeakTable<UiBlock, object?> _queue = new();

    /// <summary>Forces block enqueue operations to wait while a block consumption operation is occurring.</summary>
    private readonly AsyncManualResetEvent _consumeGate = new(true);

    /// <summary>Whether the queue is available to consume.</summary>
    /// <remarks>If this returns false, you should not call <see cref="Consume"/></remarks>
    public bool IsReadyToConsume => _consumeGate.IsSet; // gate is closed; we are currently iterating through the queue.

    /// <summary>Adds a block to the queue.</summary>
    /// <remarks>
    /// The block may not be added to the queue immediately if the queue is currently in the process of being consumed.
    /// Use <see cref="EnqueueBlockAsync"/> if you want to await the block being added to the queue.
    /// </remarks>
    public void EnqueueBlock(UiBlock block)
    {
        _consumeGate.WaitAsync(_cts.Token)
            .ContinueWith(_ => _queue.TryAdd(block, null));
    }

    /// <summary>Adds a block to the queue.</summary>
    /// <returns>A task that resolves when the block has been enqueued.</returns>
    public async Task EnqueueBlockAsync(UiBlock block)
    {
        await _consumeGate.WaitAsync(_cts.Token);
        _queue.TryAdd(block, null);
    }

    /// <summary>
    /// Returns an enumerable that contains the blocks currently in the queue
    /// and clears the queue once enumeration has completed.
    /// </summary>
    /// <remarks>
    /// This method is not reentrant! If you call this method while an existing consume operation is in progress,
    /// it will throw an exception.
    /// </remarks>
    /// <exception cref="ReentrantConsumeException">Thrown if this method is called reentrantly.</exception>
    public IEnumerable<UiBlock> Consume(CancellationToken token)
    {
        if (!IsReadyToConsume) throw new ReentrantConsumeException();

        _consumeGate.Reset(); // close the gate; enqueue ops can't add to the queue while we are iterating it.
        var any = false;
        foreach (var (block, _) in _queue)
        {
            token.ThrowIfCancellationRequested();
            any = true;
            yield return block;
        }

        if (any)
        {
            _queue.Clear();
        }
        _consumeGate.Set(); // open the gate; enqueue ops can now proceed.
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Dispose();
    }
}

/// <summary>
/// Thrown when <see cref="RenderQueue.Consume"/> is called during a pre-existing
/// </summary>
internal class ReentrantConsumeException : Exception
{
    public ReentrantConsumeException() : base(
        $"{nameof(RenderQueue.Consume)} called when a consume operation was already in progress." +
        $"Ensure {nameof(RenderQueue.IsReadyToConsume)} is true prior to attempting to consume the queue."
    ) { }
}
