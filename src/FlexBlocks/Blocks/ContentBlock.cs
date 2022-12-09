using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

public enum Sizing { Fill, Content }

public enum Alignment { Start, Center, End }

/// <summary>A block type that contains another single block, can align that block within itself,
/// and can either size itself to its content or grow to fill available space.</summary>
public abstract class ContentBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    public Alignment HorizontalContentAlignment { get; set; } = Alignment.Start;
    public Alignment VerticalContentAlignment { get; set; } = Alignment.Start;

    public Sizing HorizontalSizing { get; set; } = Sizing.Content;
    public Sizing VerticalSizing { get; set; } = Sizing.Content;

    /// <inheritdoc />
    public override RerenderMode GetRerenderModeForChild(UiBlock child)
    {
        if (child != Content)
        {
            throw new Exception("This child isn't my child...");
        }

        // If this block is sized according to its child in either dimension, our size might change
        return HorizontalSizing == Sizing.Content || VerticalSizing == Sizing.Content
            ? RerenderMode.DesiredSizeChanged
            : RerenderMode.InPlace;
    }

    /// <inheritdoc />
    public override BlockSize CalcDesiredSize(BlockSize maxSize)
    {
        if (HorizontalSizing == Sizing.Fill && VerticalSizing == Sizing.Fill)
        {
            return maxSize;
        }

        var maxContentSize = CalcMaxContentSize(maxSize);
        var contentSize = Content?.CalcDesiredSize(maxContentSize) ?? maxContentSize;

        return (HorizontalSizing, VerticalSizing) switch
        {
            (Sizing.Content, Sizing.Content) => contentSize.Constrain(maxSize),
            (Sizing.Content, Sizing.Fill)    => maxSize.ConstrainWidth(contentSize),
            (Sizing.Fill, Sizing.Content)    => maxSize.ConstrainHeight(contentSize),

            // (Fill, Fill) case is covered above
            _ => throw new UnreachableException($"Unknown sizing values. H={HorizontalSizing}, V={VerticalSizing}")
        };
    }

    /// <summary>Calculates the maximum available size for this block's content.</summary>
    protected virtual BlockSize CalcMaxContentSize(BlockSize maxSize) => maxSize;

    /// <summary>Renders this block's content to the given buffer, possibly aligned (<see cref="Alignment"/>).</summary>
    protected virtual void RenderContent(Span2D<char> contentBuffer)
    {
        if (Content is null) return;

        var alignedBuffer = ComputeAlignedContentBuffer(contentBuffer);

        RenderChild(Content, alignedBuffer);
    }

    /// <summary>Determines what subset of the total content buffer this block's content should be rendered to,
    /// according to the alignment settings.</summary>
    protected Span2D<char> ComputeAlignedContentBuffer(Span2D<char> buffer)
    {
        if (Content is null) return buffer;

        var maxSize = buffer.BlockSize();
        var contentSize = Content?.CalcDesiredSize(maxSize) ?? maxSize;

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
                var length = sanitizedSize;// + margin % 2; // if there would be uneven margins, let the content be 1 unit larger
                return (margin, length);
            case Alignment.End: return (maxSize - sanitizedSize, sanitizedSize);
            default: throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "Unknown alignment type");
        }
    }
}
