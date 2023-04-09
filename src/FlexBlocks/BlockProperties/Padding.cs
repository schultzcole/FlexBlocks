using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

/// <summary>Represents a padding offset from each of the four sides of a rectangle.</summary>
[PublicAPI]
public record Padding(int Top, int Right, int Bottom, int Left)
{
    /// <summary>Returns a new Padding object with 0 offset on all sides.</summary>
    [PublicAPI]
    public static Padding Zero => new(0);

    /// <summary>Returns a new Padding object with 1 offset on all sides.</summary>
    [PublicAPI]
    public static Padding One => new(1);

    [PublicAPI]
    public int Top { get; set; } = Top;
    [PublicAPI]
    public int Right { get; set; } = Right;
    [PublicAPI]
    public int Bottom { get; set; } = Bottom;
    [PublicAPI]
    public int Left { get; set; } = Left;

    [PublicAPI]
    public Padding(int padding) : this(padding, padding, padding, padding) { }
    [PublicAPI]
    public Padding(int vPadding, int hPadding) : this(vPadding, hPadding, vPadding, hPadding) { }

    /// <summary>Returns a new padding where the offset on all sides is above or equal to the given
    /// <paramref name="threshold"/>.</summary>
    [PublicAPI]
    public Padding Dilate(int threshold) => new(
        Math.Max(Top, threshold),
        Math.Max(Right, threshold),
        Math.Max(Bottom, threshold),
        Math.Max(Left, threshold)
    );

    /// <summary>Returns a new padding with the given <paramref name="amount"/> added to each side.</summary>
    [PublicAPI]
    public Padding Expand(int amount) => new(Top + amount, Right + amount, Bottom + amount, Left + amount);
}
