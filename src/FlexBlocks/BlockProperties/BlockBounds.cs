using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public enum Bounding { Bounded, Unbounded }

[PublicAPI]
public record struct BlockBounds(
    Bounding Horizontal,
    Bounding Vertical
)
{
    public static BlockBounds Bounded => new(Bounding.Bounded,     Bounding.Bounded);
    public static BlockBounds Unbounded => new(Bounding.Unbounded, Bounding.Unbounded);

    public static BlockBounds From(Bounding horizontal, Bounding vertical) => new(horizontal, vertical);

    public bool IsBounded => Horizontal == Bounding.Bounded && Vertical == Bounding.Bounded;
    public bool IsUnbounded => Horizontal == Bounding.Unbounded && Vertical == Bounding.Unbounded;

    public override string ToString()
    {
        if (IsBounded) return $"{nameof(BlockBounds)} {{ Bounded }}";
        if (IsUnbounded) return $"{nameof(BlockBounds)} {{ Unbounded }}";

        return $"{nameof(BlockBounds)} {{ {nameof(Horizontal)} = {Horizontal}, {nameof(Vertical)} = {Vertical} }}";
    }

    public static BlockBounds Max(BlockBounds a, BlockBounds b) => new(
        BoundingExtensions.Max(a.Horizontal, b.Horizontal),
        BoundingExtensions.Max(a.Vertical,   b.Vertical)
    );

    public static BlockBounds Min(BlockBounds a, BlockBounds b) => new(
        BoundingExtensions.Min(a.Horizontal, b.Horizontal),
        BoundingExtensions.Min(a.Vertical,   b.Vertical)
    );
}

public static class BoundingExtensions
{
    public static Bounding Max(Bounding a, Bounding b) => (a, b) switch
    {
        (Bounding.Unbounded, _) => Bounding.Unbounded,
        (_, Bounding.Unbounded) => Bounding.Unbounded,
        _                       => Bounding.Bounded
    };

    public static Bounding Min(Bounding a, Bounding b) => (a, b) switch
    {
        (Bounding.Bounded, _) => Bounding.Bounded,
        (_, Bounding.Bounded) => Bounding.Bounded,
        _                     => Bounding.Unbounded
    };
}
