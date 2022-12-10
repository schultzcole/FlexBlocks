using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks.Renderables.Debug;

/// <summary>
/// Overlays the number of times this renderable has been rendered in the top-left corner of the buffer.
/// </summary>
public sealed class RenderCountOverlay : IRenderable
{
    private int _renderCount;

    public void Render(Span2D<char> buffer)
    {
        _renderCount++;

        var countStr = _renderCount.ToString();
        if (countStr.Length >= buffer.Width) return;

        countStr.CopyTo(buffer.GetRowSpan(0));
    }
}
