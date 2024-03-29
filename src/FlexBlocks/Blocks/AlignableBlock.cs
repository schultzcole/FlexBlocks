﻿using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

public enum Sizing { Fill, Content }

public enum Alignment { Start, Center, End }

/// <summary>
/// A block that can size itself according to its content and can align its content within itself.
/// </summary>
[PublicAPI]
public sealed class AlignableBlock : UiBlock
{
    public Alignment HorizontalContentAlignment { get; set; } = Alignment.Start;

    public Alignment VerticalContentAlignment { get; set; } = Alignment.Start;

    public Sizing HorizontalSizing { get; set; } = Sizing.Content;

    public Sizing VerticalSizing { get; set; } = Sizing.Content;

    public Sizing Sizing
    {
        set
        {
            HorizontalSizing = value;
            VerticalSizing = value;
        }
    }

    public UiBlock? Content { get; set; }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        if (Content is null) return;

        var alignedBuffer = ComputeAlignedContentBuffer(buffer);

        RenderChild(Content, alignedBuffer);
    }

    /// <inheritdoc />
    public override RerenderMode GetRerenderModeForChild(UiBlock child)
    {
        // If this block is sized according to its child in either dimension, our size might change
        return HorizontalSizing == Sizing.Content || VerticalSizing == Sizing.Content
            ? RerenderMode.DesiredSizeChanged
            : RerenderMode.InPlace;
    }

    /// <inheritdoc />
    public override BlockBounds GetBounds()
    {
        if (HorizontalSizing == Sizing.Fill && VerticalSizing == Sizing.Fill)
        {
            return BlockBounds.Unbounded;
        }

        var contentBounds = Content?.GetBounds() ?? BlockBounds.Bounded;

        return (HorizontalSizing, VerticalSizing) switch
        {
            (Sizing.Content, Sizing.Content) => contentBounds,
            (Sizing.Content, Sizing.Fill)    => contentBounds with { Vertical = Bounding.Unbounded },
            (Sizing.Fill, Sizing.Content)    => contentBounds with { Horizontal = Bounding.Unbounded },

            // (Fill, Fill) case is covered above
            _ => throw new UnreachableException($"Unknown sizing values. H={HorizontalSizing}, V={VerticalSizing}")
        };
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        if (HorizontalSizing == Sizing.Fill && VerticalSizing == Sizing.Fill)
        {
            return maxSize;
        }

        var contentSize = Content?.CalcSize(maxSize) ?? BlockSize.Zero;

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
    private Span2D<char> ComputeAlignedContentBuffer(Span2D<char> buffer)
    {
        if (Content is null) return buffer;

        var maxSize = buffer.BlockSize();
        var contentSize = Content.CalcSize(maxSize).Constrain(maxSize);

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
