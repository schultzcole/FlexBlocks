using CommunityToolkit.HighPerformance;

namespace FlexBlocks.BlockProperties;

/// <summary>Represents the size of a block</summary>
/// <param name="Width">How many horizontal characters this block takes up.</param>
/// <param name="Height">How many vertical characters this block takes up.</param>
public readonly record struct BlockSize(int Width, int Height)
{
    public static BlockSize Zero { get; } = From(0, 0);

    public static BlockSize From(int width, int height) => new(Math.Max(width, 0), Math.Max(height, 0));

    /// <summary>Returns a new block size that is shrunk by the amount of padding on each side.</summary>
    public BlockSize ShrinkByPadding(Padding padding) => From(
        Width - padding.Left - padding.Right,
        Height - padding.Top - padding.Bottom
    );

    /// <summary>
    /// Returns a new BlockSize where the width and height are each reduced to those in
    /// <paramref name="maxSize"/> if the width and height in this BlockSize are larger than that.
    /// </summary>
    public BlockSize Constrain(BlockSize maxSize) =>
        From(Math.Min(Width, maxSize.Width), Math.Min(Height, maxSize.Height));

    /// <summary>
    /// Returns a new BlockSize where the width and height are each reduced to those in
    /// <paramref name="maxSize"/> if the width and height in this BlockSize are larger than that.
    /// </summary>
    public BlockSize Constrain(UnboundedBlockSize maxSize)
    {
        var width = maxSize.Width.Value is not null ? Math.Min(Width, maxSize.Width.Value.Value) : Width;
        var height = maxSize.Height.Value is not null ? Math.Min(Height, maxSize.Height.Value.Value) : Height;

        return From(width, height);
    }
}

/// <summary>
/// Represents the size a block "wants", given no size limitations.
/// This size can be unbounded in either dimension.
/// </summary>
/// <param name="Width">How many horizontal characters this block takes up. This can be unbounded.</param>
/// <param name="Height">How many vertical characters this block takes up. This can be unbounded.</param>
public readonly record struct UnboundedBlockSize(BlockLength Width, BlockLength Height)
{
    /// <summary>Whether this UnboundedBlockSize is bounded in both dimensions.</summary>
    public bool IsBounded => Width.IsBounded && Height.IsBounded;

    /// <summary>Whether this UnboundedBlockSize is bounded in either dimension.</summary>
    public bool IsUnbounded => Width.IsUnbounded || Height.IsUnbounded;

    public static UnboundedBlockSize Unbounded { get; } = new(BlockLength.Unbounded, BlockLength.Unbounded);

    public static UnboundedBlockSize Zero { get; } = From(0, 0);

    public static UnboundedBlockSize From(int? width, int? height) => new(width, height);

    public static UnboundedBlockSize From(BlockLength width, BlockLength height) => new(width, height);

    public static UnboundedBlockSize From(BlockSize blockSize) => From(blockSize.Width, blockSize.Height);

    /// <summary>
    /// Constrains this desired block size to be no larger than the given block size.
    /// This necessarily means the resulting block size will not be infinite in either dimension.
    /// </summary>
    public BlockSize Constrain(BlockSize maxSize)
    {
        var width = Width.Value is not null ? Math.Min(Width.Value.Value, maxSize.Width) : maxSize.Width;
        var height = Height.Value is not null ? Math.Min(Height.Value.Value, maxSize.Height) : maxSize.Height;

        return BlockSize.From(width, height);
    }

    public UnboundedBlockSize Constrain(UnboundedBlockSize maxSize) => Min(this, maxSize);

    /// <summary>Returns the minimum of the two desired block sizes in both dimensions.</summary>
    public static UnboundedBlockSize Min(UnboundedBlockSize left, UnboundedBlockSize right) =>
        new(
            BlockLength.Min(left.Width, right.Width),
            BlockLength.Min(left.Height, right.Height)
        );

    /// <summary>Returns the maximum of the two desired block sizes in both dimensions.</summary>
    public static UnboundedBlockSize Max(UnboundedBlockSize left, UnboundedBlockSize right) =>
        new(
            BlockLength.Max(left.Width, right.Width),
            BlockLength.Max(left.Height, right.Height)
        );

    /// <inheritdoc />
    public override string ToString() =>
        Width.IsUnbounded && Height.IsUnbounded
            ? $"{nameof(UnboundedBlockSize)} {{ Unbounded }}"
            : $"{nameof(UnboundedBlockSize)} {{ {nameof(Width)} = {Width}, {nameof(Height)} = {Height} }}";
}

public static class BlockSizeExtensions
{
    /// <summary>Returns a block size that has the same width and height as the given span.</summary>
    public static BlockSize BlockSize<T>(this Span2D<T> span) => new(span.Width, span.Height);
}
