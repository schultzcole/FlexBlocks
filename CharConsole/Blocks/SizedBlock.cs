using System.Diagnostics;

namespace CharConsole.Blocks;

public enum Sizing
{
    Fill,
    Content
}

/// A block that can be sized to either fill its container in both dimensions, or collapse to the size of its content
public abstract class SizedBlock : ContentBlock
{
    public Sizing HorizontalSizing { get; set; } = Sizing.Content;
    public Sizing VerticalSizing { get; set; } = Sizing.Content;

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
            (Sizing.Content, Sizing.Fill) => maxSize.ConstrainWidth(contentSize),
            (Sizing.Fill, Sizing.Content) => maxSize.ConstrainHeight(contentSize),

            // (Fill, Fill) case is covered above
            _ => throw new UnreachableException($"Unknown sizing values. H={HorizontalSizing}, V={VerticalSizing}")
        };
    }
}
