namespace FlexBlocks.Blocks;

/// Defines a set of characters to use for each corner and side of a border
public record Border(
    char TopLeft,
    char Top,
    char TopRight,
    char Right,
    char BottomRight,
    char Bottom,
    char BottomLeft,
    char Left)
{
    /// Creates a new Border where every side and corner uses the same character.
    public Border(char all)
        : this(all, all, all, all, all, all, all, all) { }

    /// Creates a new Border where the corners are blank and the horizontal and vertical sides use the given characters.
    public Border(char horizontal, char vertical)
        : this(' ', horizontal, ' ', vertical, ' ', horizontal, ' ', vertical) { }

    /// Creates a new Border where each corner has a unique character and the horizontal and vertical sides use the given characters.
    public Border(char topLeft, char topRight, char bottomRight, char bottomLeft, char horizontal, char vertical)
        : this(topLeft, horizontal, topRight, vertical, bottomRight, horizontal, bottomLeft, vertical) { }

    /// Renders all sides and corners as a filled block
    public static Border Block { get; } = new('\u2588');

    /// Renders a border with straight edges and sharp, square corners
    public static Border Square { get; } = new(
        topLeft: '\u250c',
        topRight: '\u2510',
        bottomRight: '\u2518',
        bottomLeft: '\u2514',
        horizontal: '\u2500',
        vertical: '\u2502'
    );

    /// Renders a border with straight edges and rounded corners
    public static Border Rounded { get; } = new(
        topLeft: '\u256d',
        topRight: '\u256e',
        bottomRight: '\u256f',
        bottomLeft: '\u2570',
        horizontal: '\u2500',
        vertical: '\u2502'
    );

    /// Renders a border for debug purposes with each side of the border showing the initial of that side (T, R, B, L)
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
