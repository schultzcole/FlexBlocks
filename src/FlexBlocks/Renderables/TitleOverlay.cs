using CommunityToolkit.HighPerformance;
using JetBrains.Annotations;

namespace FlexBlocks.Renderables;

/// <summary>
/// Renders text overlaid on the top row of a block.
/// Will only render one line of text. If the text is too long for the width of the block, it will be truncated.
/// </summary>
[PublicAPI]
public class TitleOverlay : IRenderable
{
    /// <summary>
    /// The text to render for this block's title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The offset to use to determine the horizontal positioning of the title text.
    /// </summary>
    public Index Offset { get; set; } = 1;

    /// <inheritdoc />
    public void Render(Span2D<char> buffer)
    {
        if (Title is null) return;

        var topRow = buffer.GetRowSpan(0);
        var titleLength = Math.Min(Title.Length, topRow.Length - Offset.Value);
        var start = Offset.IsFromEnd ? (topRow.Length - Offset.Value - titleLength) : Offset.Value;
        var offsetRow = topRow.Slice(start, titleLength);
        Title.AsSpan()[..Math.Min(Title.Length, offsetRow.Length)].CopyTo(offsetRow);
    }
}
