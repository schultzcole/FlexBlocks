namespace CharConsole.Blocks;

public record Padding(int Top, int Right, int Bottom, int Left)
{
    public int Top { get; set; } = Top;
    public int Right { get; set; } = Right;
    public int Bottom { get; set; } = Bottom;
    public int Left { get; set; } = Left;

    public Padding(int padding) : this(padding, padding, padding, padding) { }
    public Padding(int vPadding, int hPadding) : this(vPadding, hPadding, vPadding, hPadding) { }

    /// Ensures padding on all sides is at least <paramref name="threshold"/>.
    public Padding Dilate(int threshold)
    {
        return new Padding(
            Math.Max(Top, threshold),
            Math.Max(Right, threshold),
            Math.Max(Bottom, threshold),
            Math.Max(Left, threshold)
        );
    }
}
