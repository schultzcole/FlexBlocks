using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public static class Borders
{
    /// <summary>Renders all sides and corners as a filled block.</summary>
    [PublicAPI]
    public static Border Block { get; } = new('\u2588');

    /// <summary>Renders a border with straight edges and sharp, square corners.</summary>
    [PublicAPI]
    public static Border Square { get; } = new(
        topLeft: '\u250c',
        topRight: '\u2510',
        bottomRight: '\u2518',
        bottomLeft: '\u2514',
        horizontal: '\u2500',
        vertical: '\u2502'
    );

    /// <summary>Renders a border with straight edges and rounded corners.</summary>
    [PublicAPI]
    public static Border Rounded { get; } = new(
        topLeft: '\u256d',
        topRight: '\u256e',
        bottomRight: '\u256f',
        bottomLeft: '\u2570',
        horizontal: '\u2500',
        vertical: '\u2502'
    );

    public static class Debug
    {
        /// <summary>Renders a border for debug purposes with each side of the border showing
        /// the initial of that side (T, R, B, L).</summary>
        [PublicAPI]
        public static Border DebugSide { get; } = new(
            TopLeft: '/',
            Top: 'T',
            TopRight: '\\',
            Right: 'R',
            BottomRight: '/',
            Bottom: 'B',
            BottomLeft: '\\',
            Left: 'L'
        );
    }
}
