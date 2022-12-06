using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

/// Base class of UI components
public abstract class UiBlock
{
    public abstract BlockSize CalcDesiredSize(BlockSize maxSize);

    public abstract void Render(Span2D<char> buffer);
}
