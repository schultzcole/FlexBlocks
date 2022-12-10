using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks.Renderables;

/// <summary>A Pattern is an IRenderable that fills the full available buffer with some repetition of characters.</summary>
public abstract class Pattern : IRenderable
{
    /// <inheritdoc />
    public abstract void Render(Span2D<char> buffer);
}
