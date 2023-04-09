using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

/// <summary>
/// An abstract block that sizes itself and aligns content within itself based on its content.
/// The exact nature of the content is determined by the subclass.
/// </summary>
[PublicAPI]
public abstract class AlignableBlock : UiBlock
{
    [PublicAPI]
    public Alignment HorizontalContentAlignment { get; set; } = Alignment.Start;
    [PublicAPI]
    public Alignment VerticalContentAlignment { get; set; } = Alignment.Start;

    [PublicAPI]
    public Sizing HorizontalSizing { get; set; } = Sizing.Content;
    [PublicAPI]
    public Sizing VerticalSizing { get; set; } = Sizing.Content;

    /// <inheritdoc />
    public override RerenderMode GetRerenderModeForChild(UiBlock child)
    {
        // If this block is sized according to its child in either dimension, our size might change
        return HorizontalSizing == Sizing.Content || VerticalSizing == Sizing.Content
            ? RerenderMode.DesiredSizeChanged
            : RerenderMode.InPlace;
    }

    /// <summary>Whether this alignable block has any content to align.</summary>
    [PublicAPI]
    public abstract bool HasContent { get; }

    /// <summary>Computes the max size of the content contained by this.</summary>
    protected abstract UnboundedBlockSize? CalcContentMaxSize();

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize()
    {
        if (HorizontalSizing == Sizing.Fill && VerticalSizing == Sizing.Fill)
        {
            return UnboundedBlockSize.Unbounded;
        }

        var contentSize = CalcContentMaxSize() ?? UnboundedBlockSize.Zero;

        return (HorizontalSizing, VerticalSizing) switch
        {
            (Sizing.Content, Sizing.Content) => contentSize,
            (Sizing.Content, Sizing.Fill)    => contentSize with { Height = BlockLength.Unbounded },
            (Sizing.Fill, Sizing.Content)    => contentSize with { Width = BlockLength.Unbounded },

            // (Fill, Fill) case is covered above
            _ => throw new UnreachableException($"Unknown sizing values. H={HorizontalSizing}, V={VerticalSizing}")
        };
    }

    /// <summary>Computes the size of this block's contents given a concrete maximum size.</summary>
    protected abstract BlockSize? CalcContentSize(BlockSize maxSize);

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        if (HorizontalSizing == Sizing.Fill && VerticalSizing == Sizing.Fill)
        {
            return maxSize;
        }

        var contentSize = CalcContentSize(maxSize) ?? BlockSize.Zero;

        return (HorizontalSizing, VerticalSizing) switch
        {
            (Sizing.Content, Sizing.Content) => contentSize,
            (Sizing.Content, Sizing.Fill)    => contentSize with { Height = maxSize.Height },
            (Sizing.Fill, Sizing.Content)    => contentSize with { Width = maxSize.Width },

            // (Fill, Fill) case is covered above
            _ => throw new UnreachableException($"Unknown sizing values. H={HorizontalSizing}, V={VerticalSizing}")
        };
    }

    /// <summary>Determines what subset of the total content buffer this block's content should be rendered to,
    /// according to the alignment settings.</summary>
    protected Span2D<char> ComputeAlignedContentBuffer(Span2D<char> buffer)
    {
        if (!HasContent) return buffer;

        var maxSize = buffer.BlockSize();
        var contentSize = CalcContentSize(maxSize)!.Value.Constrain(maxSize);

        var hDimension = ComputeAlignedDimension(maxSize.Width, contentSize.Width, HorizontalContentAlignment);
        var vDimension = ComputeAlignedDimension(maxSize.Height, contentSize.Height, VerticalContentAlignment);

        return buffer.Slice(
            row: vDimension.start,
            column: hDimension.start,
            height: vDimension.length,
            width: hDimension.length
        );
    }

    /// <summary>Calculates a slice of the full length of a single dimension that the content should be rendered to.</summary>
    private static (int start, int length) ComputeAlignedDimension(int maxSize, int desiredSize, Alignment alignment)
    {
        var sanitizedSize = Math.Min(maxSize, desiredSize);

        switch (alignment)
        {
            case Alignment.Start: return (0, sanitizedSize);
            case Alignment.Center:
                var margin = (maxSize - sanitizedSize) / 2;
                var length = sanitizedSize;
                return (margin, length);
            case Alignment.End: return (maxSize - sanitizedSize, sanitizedSize);
            default: throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "Unknown alignment type");
        }
    }
}
