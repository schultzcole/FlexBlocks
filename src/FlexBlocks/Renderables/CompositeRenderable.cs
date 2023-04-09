using System.Collections;
using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;
using JetBrains.Annotations;

namespace FlexBlocks.Renderables;

/// <summary>
/// Combines multiple renderables by layering them one on top of the other.
/// </summary>
[PublicAPI]
public class CompositeRenderable : IRenderable, ICollection<IRenderable>
{
    /// <summary>
    /// The renderables to layer on top of each other.
    /// Layers are rendered in the order they appear in the list, so higher indices have higher "priority".
    /// </summary>
    [PublicAPI]
    public List<IRenderable> Layers { get; }

    [PublicAPI]
    public CompositeRenderable()
    {
        Layers = new List<IRenderable>();
    }

    [PublicAPI]
    public CompositeRenderable(List<IRenderable> layers)
    {
        Layers = layers;
    }

    public void Render(Span2D<char> buffer)
    {
        foreach (var layer in Layers)
        {
            layer.Render(buffer);
        }
    }

    /// <inheritdoc />
    public IEnumerator<IRenderable> GetEnumerator() => Layers.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Layers).GetEnumerator();

    /// <inheritdoc />
    public void Add(IRenderable item) => Layers.Add(item);

    /// <inheritdoc />
    public void Clear() => Layers.Clear();

    /// <inheritdoc />
    public bool Contains(IRenderable item) => Layers.Contains(item);

    /// <inheritdoc />
    public void CopyTo(IRenderable[] array, int arrayIndex) => Layers.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(IRenderable item) => Layers.Remove(item);

    /// <inheritdoc />
    public int Count => Layers.Count;

    /// <inheritdoc />
    public bool IsReadOnly => ((ICollection<IRenderable>)Layers).IsReadOnly;
}
