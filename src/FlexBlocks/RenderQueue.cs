using System.Collections.Immutable;
using FlexBlocks.Blocks;

namespace FlexBlocks;

internal sealed class RenderQueue
{
    /// <summary>A collection of blocks that should be re-rendered on the next "frame".</summary>
    /// <remarks>This table is functionally used as a set; it does not store actual values for each key.</remarks>
    private readonly HashSet<WeakReference<UiBlock>> _queue = new(new WeakRefComparer<UiBlock>());

    /// <summary>Adds a block to the queue.</summary>
    public void EnqueueBlock(UiBlock block) { _queue.Add(new WeakReference<UiBlock>(block)); }

    /// <summary>
    /// Returns an enumerable that contains the blocks currently in the queue
    /// and clears the queue once enumeration has completed.
    /// </summary>
    public ImmutableHashSet<UiBlock> Consume()
    {
        var builder = ImmutableHashSet.CreateBuilder<UiBlock>(ReferenceEqualityComparer.Instance);
        foreach (var blockRef in _queue)
        {
            if (!blockRef.TryGetTarget(out var block)) continue;
            builder.Add(block);
        }
        _queue.Clear();
        return builder.ToImmutable();
    }
}

internal class WeakRefComparer<T> : IEqualityComparer<WeakReference<T>>
    where T : class
{
    public bool Equals(WeakReference<T>? x, WeakReference<T>? y)
    {
        T? xTarget = null;
        T? yTarget = null;
        x?.TryGetTarget(out xTarget);
        y?.TryGetTarget(out yTarget);
        return ReferenceEquals(xTarget, yTarget);
    }

    public int GetHashCode(WeakReference<T> obj) => obj.TryGetTarget(out var target) ? target.GetHashCode() : 0;
}
