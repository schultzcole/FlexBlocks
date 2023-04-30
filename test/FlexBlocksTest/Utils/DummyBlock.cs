using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;

namespace FlexBlocksTest.Utils;

/// <summary>Doesn't do anything</summary>
public class DummyBlock : UiBlock
{
    public int Id { get; }
    public DummyBlock(int id) { Id = id; }

    public override BlockBounds GetBounds() => throw new System.NotImplementedException();
    public override BlockSize CalcSize(BlockSize maxSize) => throw new System.NotImplementedException();
    public override void Render(Span2D<char> buffer) => throw new System.NotImplementedException();
}
