using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

/// <summary>
/// Renders the same character for all borders
/// </summary>
[PublicAPI]
public class SimpleBorder : IBorder
{
    public char Character { get; set; }

    /// <inheritdoc />
    public char? OuterEdge(
        BorderOuterEdge edge,
        BorderAccent accent = BorderAccent.Normal
    ) => Character;

    /// <inheritdoc />
    public char? OuterCorner(
        BorderOuterCorner corner,
        BorderAccent verticalAccent = BorderAccent.Normal,
        BorderAccent horizontalAccent = BorderAccent.Normal
    ) => Character;

    /// <inheritdoc />
    public char? OuterJunction(
        BorderOuterEdge edge,
        BorderAccent outerAccent = BorderAccent.Normal,
        BorderAccent innerAccent = BorderAccent.Normal
    ) => Character;

    /// <inheritdoc />
    public char? InnerEdge(
        BorderInnerEdge edge,
        BorderAccent accent = BorderAccent.Normal
    ) => Character;

    /// <inheritdoc />
    public char? InnerJunction(
        BorderAccent verticalAccent = BorderAccent.Normal,
        BorderAccent horizontalAccent = BorderAccent.Normal
    ) => Character;
}
