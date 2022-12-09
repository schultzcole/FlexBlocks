﻿using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks.Debug;

/// A simple container block that simply renders the number of times it's content has been rendered.
public class DebugRenderCountBlock : UiBlock
{
    private int _renderCount;

    public required UiBlock Content { get; init; }

    public override bool ShouldRerenderWithChildren => true;

    public override BlockSize CalcDesiredSize(BlockSize maxSize) => Content.CalcDesiredSize(maxSize);

    public override void Render(Span2D<char> buffer)
    {
        RenderChild(Content, buffer);

        _renderCount++;
        _renderCount.ToString().CopyTo(buffer.GetRowSpan(0));
    }
}
