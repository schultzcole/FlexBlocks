using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

public enum Alignment { Start, Center, End, }

/// A block type that contains another single block and can align that block within itself.
public abstract class ContentBlock : UiBlock
{
    public UiBlock? Content { get; set; }

    public Alignment HorizontalContentAlignment { get; set; } = Alignment.Start;
    public Alignment VerticalContentAlignment { get; set; } = Alignment.Start;

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

        var maxSize = buffer.GetSize();
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
