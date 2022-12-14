using CommunityToolkit.HighPerformance;

namespace FlexBlocks.BlockProperties;

/// <summary>Represents the size of a block</summary>
/// <param name="Width">How many horizontal characters this block takes up.</param>
/// <param name="Height">How many vertical characters this block takes up.</param>
public readonly record struct BlockSize(int Width, int Height)
{
    /// <summary>Returns a new block size that is shrunk by the amount of padding on each side.</summary>
    public BlockSize ShrinkByPadding(Padding padding) => new(
        Width - padding.Left - padding.Right,
        Height - padding.Top - padding.Bottom
    );
}

/// <summary>Represents the desired width of a block, with the option to be unbounded.</summary>
/// <param name="Width">How many horizontal characters this block takes up. This can be unbounded.</param>
/// <param name="Height">How many vertical characters this block takes up. This can be unbounded.</param>
public readonly record struct DesiredBlockSize(BlockLength Width, BlockLength Height)
{
    public static DesiredBlockSize Unbounded { get; } = new(BlockLength.Unbounded, BlockLength.Unbounded);

    public static DesiredBlockSize From(int? width, int? height) =>
        new(BlockLength.From(width), BlockLength.From(height));

    public static DesiredBlockSize From(BlockLength width, BlockLength height) => new(width, height);

    /// <summary>
    /// Constrains this desired block size to be no larger than the given block size.
    /// This necessarily means the resulting block size will not be infinite in either dimension.
    /// </summary>
    public BlockSize Constrain(BlockSize maxSize)
    {
        var width = Width.Value is not null ? Math.Min(Width.Value.Value, maxSize.Width) : maxSize.Width;
        var height = Height.Value is not null ? Math.Min(Height.Value.Value, maxSize.Height) : maxSize.Height;

        return new BlockSize(width, height);
    }

    /// <summary>Returns the minimum of the two desired block sizes in both dimensions.</summary>
    public static DesiredBlockSize Min(DesiredBlockSize left, DesiredBlockSize right) =>
        new(
            BlockLength.Min(left.Width, right.Width),
            BlockLength.Min(left.Height, right.Height)
        );

    /// <summary>Returns the maximum of the two desired block sizes in both dimensions.</summary>
    public static DesiredBlockSize Max(DesiredBlockSize left, DesiredBlockSize right) =>
        new(
            BlockLength.Max(left.Width, right.Width),
            BlockLength.Max(left.Height, right.Height)
        );
}

public static class BlockSizeExtensions
{
    /// <summary>Returns a block size that has the same width and height as the given span.</summary>
    public static BlockSize BlockSize<T>(this Span2D<T> span) => new(span.Width, span.Height);
}
