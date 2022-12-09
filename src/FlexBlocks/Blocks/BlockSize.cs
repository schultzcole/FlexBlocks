using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

/// Represents the size of a block
/// <param name="Width">How many horizontal characters this block takes up.</param>
/// <param name="Height">How many vertical characters this block takes up.</param>
public readonly record struct BlockSize(
    int Width,
    int Height
)
{
    /// <summary>Constrains this block size to be no larger than another. Effectively a 2d minimum.</summary>
    public BlockSize Constrain(BlockSize other) => new(Math.Min(Width, other.Width), Math.Min(Height, other.Height));

    /// <summary>Constrains this block size to be no larger than the given width and height. Effectively a 2d minimum.</summary>
    public BlockSize Constrain(int width, int height) => new(Math.Min(Width, width), Math.Min(Height, height));

    /// <summary>Constrains this block's width to be no wider than another.</summary>
    public BlockSize ConstrainWidth(BlockSize other) => this with { Width = Math.Min(Width, other.Width) };

    /// <summary>Constrains this block's width to be no wider than a specified value.</summary>
    public BlockSize ConstrainWidth(int width) => this with { Width = Math.Min(Width, width) };

    /// <summary>Constrains this block's height to be no taller than another.</summary>
    public BlockSize ConstrainHeight(BlockSize other) => this with { Height = Math.Min(Height, other.Height) };

    /// <summary>Constrains this block's height to be no taller than a specified value.</summary>
    public BlockSize ConstrainHeight(int height) => this with { Height = Math.Min(Height, height) };
}

public static class BlockSizeExtensions
{
    public static BlockSize BlockSize<T>(this Span2D<T> span) => new(span.Width, span.Height);
}
