using System.Numerics;

namespace FlexBlocks.BlockProperties;

/// <summary>Represents a potentially unbounded length in a single dimension.</summary>
public record struct BlockLength :
    IAdditionOperators<BlockLength, int?, BlockLength>,
    IAdditionOperators<BlockLength, double?, BlockLength>,
    IAdditionOperators<BlockLength, BlockLength, BlockLength>,
    ISubtractionOperators<BlockLength, int?, BlockLength>,
    ISubtractionOperators<BlockLength, double?, BlockLength>,
    ISubtractionOperators<BlockLength, BlockLength, BlockLength>,
    IMultiplyOperators<BlockLength, int?, BlockLength>,
    IMultiplyOperators<BlockLength, double?, BlockLength>,
    IMultiplyOperators<BlockLength, BlockLength, BlockLength>,
    IDivisionOperators<BlockLength, int?, BlockLength>,
    IDivisionOperators<BlockLength, double?, BlockLength>,
    IDivisionOperators<BlockLength, BlockLength, BlockLength>,
    IModulusOperators<BlockLength, int?, BlockLength>,
    IModulusOperators<BlockLength, double?, BlockLength>,
    IModulusOperators<BlockLength, BlockLength, BlockLength>,
    IComparisonOperators<BlockLength, BlockLength, bool>,
    IComparisonOperators<BlockLength, int?, bool>,
    IComparisonOperators<BlockLength, double?, bool>
{
    public int? Value { get; private init; }

    /// <summary>An unbounded length.</summary>
    public static BlockLength Unbounded { get; } = new();

    /// <summary>Creates a new length with the given length value.</summary>
    public static BlockLength From(int? value) => new() { Value = value };

    public BlockLength() { }

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

    /// <summary>Adds a length with a nullable double. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator +(BlockLength length, double? value) => From((int?)(length.Value + value));

    /// <summary>Subtracts a nullable double from a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator -(BlockLength length, double? value) => From((int?)(length.Value - value));

    /// <summary>Divides a length by a nullable double. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator /(BlockLength length, double? value) => From((int?)(length.Value / value));

    /// <summary>Multiplies a length with a nullable double. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator *(BlockLength length, double? value) => From((int?)(length.Value * value));

    /// <summary>Moduluses a length by a nullable double. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator %(BlockLength length, double? value) => From((int?)(length.Value % value));

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

    /// <summary>Adds a nullable double with a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator +(double? value, BlockLength length) => From((int?)(length.Value + value));

    /// <summary>Subtracts a length from a nullable double. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator -(double? value, BlockLength length) => From((int?)(length.Value - value));

    /// <summary>Divides a nullable double by a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator /(double? value, BlockLength length) => From((int?)(length.Value / value));

    /// <summary>Multiplies a nullable double with a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator *(double? value, BlockLength length) => From((int?)(length.Value * value));

    /// <summary>Moduluses a nullable double by a length. If the length is unbounded or if the value is null, the result will be unbounded</summary>
    public static BlockLength operator %(double? value, BlockLength length) => From((int?)(length.Value % value));

    /// <inheritdoc />
    public static bool operator >(BlockLength left, BlockLength right) => left.Value > right.Value;

    /// <inheritdoc />
    public static bool operator >=(BlockLength left, BlockLength right) => left.Value >= right.Value;

    /// <inheritdoc />
    public static bool operator <(BlockLength left, BlockLength right) => left.Value < right.Value;

    /// <inheritdoc />
    public static bool operator <=(BlockLength left, BlockLength right) => left.Value <= right.Value;

    /// <inheritdoc />
    public static bool operator ==(BlockLength left, int? right) => left.Value == right;

    /// <inheritdoc />
    public static bool operator !=(BlockLength left, int? right) => left.Value != right;

    /// <inheritdoc />
    public static bool operator >(BlockLength left, int? right) => left.Value > right;

    /// <inheritdoc />
    public static bool operator >=(BlockLength left, int? right) => left.Value >= right;

    /// <inheritdoc />
    public static bool operator <(BlockLength left, int? right) => left.Value < right;

    /// <inheritdoc />
    public static bool operator <=(BlockLength left, int? right) => left.Value <= right;

    /// <inheritdoc />
    public static bool operator ==(BlockLength left, double? right) => left.Value == right;

    /// <inheritdoc />
    public static bool operator !=(BlockLength left, double? right) => left.Value != right;

    /// <inheritdoc />
    public static bool operator >(BlockLength left, double? right) => left.Value > right;

    /// <inheritdoc />
    public static bool operator >=(BlockLength left, double? right) => left.Value >= right;

    /// <inheritdoc />
    public static bool operator <(BlockLength left, double? right) => left.Value < right;

    /// <inheritdoc />
    public static bool operator <=(BlockLength left, double? right) => left.Value <= right;
}
