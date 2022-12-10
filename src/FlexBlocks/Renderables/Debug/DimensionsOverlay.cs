using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks.Renderables.Debug;

/// <summary>
/// Renders the current dimensions of the buffer to the buffer on the top and left sides
/// </summary>
public class DimensionsOverlay : IRenderable
{
    public void Render(Span2D<char> buffer)
    {
        var widthStr = buffer.Width.ToString();
        if (widthStr.Length < buffer.Width)
        {
            var widthXOffset = (buffer.Width / 2) - (widthStr.Length / 2);
            widthStr.CopyTo(buffer.GetRowSpan(0)[widthXOffset..]);
        }

        var heightStr = buffer.Height.ToString();
        if (heightStr.Length < buffer.Height)
        {
            var heightYOffset = (buffer.Height / 2) - (heightStr.Length / 2);
            for (int row = 0; row < heightStr.Length; row++)
            {
                buffer[row + heightYOffset, 0] = heightStr[row];
            }
        }
    }
}
