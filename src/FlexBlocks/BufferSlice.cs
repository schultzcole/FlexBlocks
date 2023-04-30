using CommunityToolkit.HighPerformance;

namespace FlexBlocks;

/// <summary>Stores information about the slice of the main render buffer that a particular block occupies.</summary>
internal record BufferSlice(int Column, int Row, int Width, int Height)
{
    /// <summary>Whether this slice occupies any actual area.</summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;
}

internal static class Span2DExtensions
{
    public static Span2D<T> Slice<T>(this Span2D<T> span, BufferSlice slice) =>
        span.Slice(slice.Row, slice.Column, slice.Height, slice.Width);
}
