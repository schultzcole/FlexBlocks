﻿using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

/// <summary>
/// <see cref="CustomBlock"/> allows for defining a hierarchy of content that will be rendered, without needing to
/// specify the internal details of how to measure and render the content.
/// </summary>
public abstract class CustomBlock : UiBlock
{
    public abstract UiBlock Content { get; }

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize() => Content.CalcMaxSize();

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize) => Content.CalcSize(maxSize);

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer) => RenderChild(Content, buffer);
}