using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace FlexBlocks.BlockProperties;

/// <summary>Represents a potentially unbounded length in a single dimension.</summary>
public record struct BlockLength :
    IAdditionOperators<BlockLength, BlockLength, BlockLength>,
    ISubtractionOperators<BlockLength, BlockLength, BlockLength>,
    IMultiplyOperators<BlockLength, BlockLength, BlockLength>,
    IDivisionOperators<BlockLength, BlockLength, BlockLength>,
    IModulusOperators<BlockLength, BlockLength, BlockLength>,
    IComparisonOperators<BlockLength, BlockLength, bool>,
    IAdditionOperators<BlockLength, int?, BlockLength>,
    ISubtractionOperators<BlockLength, int?, BlockLength>,
    IMultiplyOperators<BlockLength, int?, BlockLength>,
    IDivisionOperators<BlockLength, int?, BlockLength>,
    IModulusOperators<BlockLength, int?, BlockLength>,
    IComparisonOperators<BlockLength, int?, bool>
{
    public int? Value { get; private init; }

    /// <summary>Whether this BlockLength is bounded.</summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsBounded => Value.HasValue;

    /// <summary>Whether this BlockLength is unbounded.</summary>
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsUnbounded => !IsBounded;

    /// <summary>An unbounded length.</summary>
    public static BlockLength Unbounded { get; } = new();

    /// <summary>Zero length.</summary>
    public static BlockLength Zero { get; } = From(0);

    /// <summary>Creates a new length with the given length value.</summary>
    public static BlockLength From(int? value) => new() { Value = value };

    public static implicit operator BlockLength(int? val) => From(val);

    public BlockLength() { }

    /// <inheritdoc />
    public override string ToString() =>
        IsUnbounded
            ? $"{nameof(BlockLength)} {{ Unbounded }}"
            : $"{nameof(BlockLength)} {{ {nameof(Value)} = {Value} }}";

    public static BlockLength Min(BlockLength left, BlockLength right) => left < right ? left : right;
    public static BlockLength Max(BlockLength left, BlockLength right) => left > right ? left : right;

    /// <summary>Adds two block lengths. If either is unbounded, the result will be unbounded.</summary>
    public static BlockLength operator +(BlockLength length, BlockLength other) => From(length.Value + other.Value);

    /// <summary>Subtracts two block lengths. If either is unbounded, the result will be unbounded.</summary>
    public static BlockLength operator -(BlockLength length, BlockLength other) => From(length.Value - other.Value);

    /// <summary>Multiplies two block lengths. If either is unbounded, the result will be unbounded.</summary>
    public static BlockLength operator *(BlockLength length, BlockLength other) => From(length.Value * other.Value);

    /// <summary>Divides two block lengths. If either is unbounded, the result will be unbounded.</summary>
    public static BlockLength operator /(BlockLength length, BlockLength other) => From(length.Value / other.Value);

    /// <summary>Moduluses two block lengths. If either is unbounded, the result will be unbounded.</summary>
    public static BlockLength operator %(BlockLength length, BlockLength other) => From(length.Value % other.Value);

    /// <summary>Adds a length with a nullable int. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator +(BlockLength length, int? value) => From(length.Value + value);

    /// <summary>Subtracts a nullable int from a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator -(BlockLength length, int? value) => From(length.Value - value);

    /// <summary>Divides a length by a nullable int. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator /(BlockLength length, int? value) => From(length.Value / value);

    /// <summary>Multiplies a length with a nullable int. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator *(BlockLength length, int? value) => From(length.Value * value);

    /// <summary>Moduluses a length by a nullable int. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator %(BlockLength length, int? value) => From(length.Value % value);

    /// <summary>Adds a nullable int with a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator +(int? value, BlockLength length) => From(length.Value + value);

    /// <summary>Subtracts a length from a nullable int. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator -(int? value, BlockLength length) => From(length.Value - value);

    /// <summary>Divides a nullable int by a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator /(int? value, BlockLength length) => From(length.Value / value);

    /// <summary>Multiplies a nullable int with a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator *(int? value, BlockLength length) => From(length.Value * value);

    /// <summary>Moduluses a nullable int by a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator %(int? value, BlockLength length) => From(length.Value % value);

    /// <inheritdoc />
    public static bool operator >(BlockLength left, BlockLength right) =>
        UnboundedNumberComparisons.GreaterThan(left.Value, right.Value);

    /// <inheritdoc />
    public static bool operator >=(BlockLength left, BlockLength right) =>
        UnboundedNumberComparisons.GreaterThanOrEqual(left.Value, right.Value);

    /// <inheritdoc />
    public static bool operator <(BlockLength left, BlockLength right) =>
        UnboundedNumberComparisons.LessThan(left.Value, right.Value);

    /// <inheritdoc />
    public static bool operator <=(BlockLength left, BlockLength right) =>
        UnboundedNumberComparisons.LessThanOrEqual(left.Value, right.Value);

    /// <inheritdoc />
    public static bool operator ==(BlockLength left, int? right) =>
        UnboundedNumberComparisons.Equal(left.Value, right);

    /// <inheritdoc />
    public static bool operator !=(BlockLength left, int? right) =>
        UnboundedNumberComparisons.NotEqual(left.Value, right);

    /// <inheritdoc />
    public static bool operator >(BlockLength left, int? right) =>
        UnboundedNumberComparisons.GreaterThan(left.Value, right);

    /// <inheritdoc />
    public static bool operator >=(BlockLength left, int? right) =>
        UnboundedNumberComparisons.GreaterThanOrEqual(left.Value, right);

    /// <inheritdoc />
    public static bool operator <(BlockLength left, int? right) =>
        UnboundedNumberComparisons.LessThan(left.Value, right);

    /// <inheritdoc />
    public static bool operator <=(BlockLength left, int? right) =>
        UnboundedNumberComparisons.LessThanOrEqual(left.Value, right);
}

/// <summary>
/// Generic comparison functions for nullable numbers where a value of null represents "unbounded" or "infinity"
/// </summary>
public static class UnboundedNumberComparisons
{
    public static bool GreaterThan<T>(T? left, T? right) where T : unmanaged, IComparisonOperators<T, T, bool> =>
        (left, right) switch
        {
            (_, null)      => false,
            (null, _)      => true,
            ({ } l, { } r) => l > r
        };

    public static bool GreaterThanOrEqual<T>(T? left, T? right) where T : unmanaged, IComparisonOperators<T, T, bool> =>
        (left, right) switch
        {
            (null, null)   => true,
            (_, null)      => false,
            (null, _)      => true,
            ({ } l, { } r) => l >= r
        };

    public static bool LessThan<T>(T? left, T? right) where T : unmanaged, IComparisonOperators<T, T, bool> =>
        (left, right) switch
        {
            (null, _)      => false,
            (_, null)      => true,
            ({ } l, { } r) => l < r
        };

    public static bool LessThanOrEqual<T>(T? left, T? right) where T : unmanaged, IComparisonOperators<T, T, bool> =>
        (left, right) switch
        {
            (null, null)   => true,
            (null, _)      => false,
            (_, null)      => true,
            ({ } l, { } r) => l <= r
        };

    public static bool Equal<T>(T? left, T? right) where T : unmanaged, IComparisonOperators<T, T, bool> =>
        left == right;

    public static bool NotEqual<T>(T? left, T? right) where T : unmanaged, IComparisonOperators<T, T, bool> =>
        left != right;
}
