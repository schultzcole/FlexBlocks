using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

/// <summary>Represents a padding offset from each of the four sides of a rectangle.</summary>
[PublicAPI]
public record Padding(int Top, int Right, int Bottom, int Left)
{
    /// <summary>Returns a new Padding object with 0 offset on all sides.</summary>
    public static Padding Zero => new(0);

    /// <summary>Returns a new Padding object with 1 offset on all sides.</summary>
    public static Padding One => new(1);

    public int Top { get; set; } = Top;
    public int Right { get; set; } = Right;
    public int Bottom { get; set; } = Bottom;
    public int Left { get; set; } = Left;

    public Padding(int padding) : this(padding, padding, padding, padding) { }
    public Padding(int vPadding, int hPadding) : this(vPadding, hPadding, vPadding, hPadding) { }

    /// <summary>Returns a new padding where the offset on all sides is above or equal to the given
    /// <paramref name="threshold"/>.</summary>
    public Padding Dilate(int threshold) => new(
        Math.Max(Top, threshold),
        Math.Max(Right, threshold),
        Math.Max(Bottom, threshold),
        Math.Max(Left, threshold)
    );

    /// <summary>Returns a new padding with the given <paramref name="amount"/> added to each side.</summary>
    public Padding Expand(int amount) => new(Top + amount, Right + amount, Bottom + amount, Left + amount);

    public static Padding operator +(Padding first, Padding second) => new(
        first.Top + second.Top,
        first.Right + second.Right,
        first.Bottom + second.Bottom,
        first.Left + second.Left
    );
}
