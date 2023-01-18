using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks.Renderables;

/// <summary>
/// Represents a thing that can be rendered to a 2D character buffer with no ability to influence the layout or size of
/// the buffer, unlike a <see cref="UiBlock"/>.
/// </summary>
public interface IRenderable
{
    /// <summary>Renders this object to a given render buffer.</summary>
    /// <param name="buffer">The buffer to render to.</param>
    public void Render(Span2D<char> buffer);
}
