using CommunityToolkit.HighPerformance;
using JetBrains.Annotations;

namespace FlexBlocks.Renderables;

/// <summary>A pattern type that simply fills the whole render buffer with a single character.</summary>
[PublicAPI]
public class FillPattern : Pattern
{
    /// <summary>The char with which to fill the render buffer.</summary>
    [PublicAPI]
    public required char Character { get; set; }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer) => buffer.Fill(Character);
}
