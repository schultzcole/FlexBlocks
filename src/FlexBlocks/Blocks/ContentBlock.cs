using System.Diagnostics;
using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

public enum Sizing { Fill, Content }

public enum Alignment { Start, Center, End }

/// A block type that contains another single block, can align that block within itself,
/// and can either size itself to its content or grow to fill available space.
public abstract class ContentBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    public Alignment HorizontalContentAlignment { get; set; } = Alignment.Start;
    public Alignment VerticalContentAlignment { get; set; } = Alignment.Start;

    public Sizing HorizontalSizing { get; set; } = Sizing.Content;
    public Sizing VerticalSizing { get; set; } = Sizing.Content;

    public override bool ShouldRecomputeDesiredSize =>
        Content is not null
        && HorizontalSizing == Sizing.Content
        && VerticalSizing == Sizing.Content
        && Content.ShouldRecomputeDesiredSize;

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

    protected virtual BlockSize CalcMaxContentSize(BlockSize maxSize) => maxSize;

    protected virtual void RenderContent(Span2D<char> contentBuffer)
    {
        if (Content is null) return;

        var alignedBuffer = CalcAlignedContentBuffer(contentBuffer);
        Content.Render(alignedBuffer);
    }

    protected virtual Span2D<char> CalcAlignedContentBuffer(Span2D<char> buffer)
    {
        if (Content is null) return buffer;

        var maxSize = buffer.BlockSize();
        var contentSize = Content?.CalcDesiredSize(maxSize) ?? maxSize;

        var hDimension = CalcAlignedDimension(maxSize.Width, contentSize.Width, HorizontalContentAlignment);
        var vDimension = CalcAlignedDimension(maxSize.Height, contentSize.Height, VerticalContentAlignment);

        return buffer.Slice(
            row: vDimension.start,
            column: hDimension.start,
            height: vDimension.length,
            width: hDimension.length
        );
    }

    private static (int start, int length) CalcAlignedDimension(int maxSize, int desiredSize, Alignment alignment)
    {
        var sanitizedSize = Math.Min(maxSize, desiredSize);

        switch (alignment)
        {
            case Alignment.Start: return (0, sanitizedSize);
            case Alignment.Center:
                var margin = (maxSize - sanitizedSize) / 2;
                // if there would be uneven margins, let the content be 1 unit larger
                return (margin, sanitizedSize + margin % 2);
            case Alignment.End: return (maxSize - sanitizedSize, sanitizedSize);
            default: throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "Unknown alignment type");
        }
    }
}
