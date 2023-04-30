using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FluentAssertions;
using Xunit;

namespace FlexBlocksTest.Blocks;

public class FixedSizeBlockTests
{
    public class GetBounds
    {
        [Fact]
        public void Should_return_bounded_when_size_is_bounded()
        {
            var block = new FixedSizeBlock { Width = 5, Height = 5 };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_unbounded_horizontal_when_width_is_unbounded()
        {
            var block = new FixedSizeBlock { Width = BlockLength.Unbounded, Height = 5 };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(Bounding.Unbounded, Bounding.Bounded));
        }

        [Fact]
        public void Should_return_unbounded_vertical_when_height_is_unbounded()
        {
            var block = new FixedSizeBlock { Width = 5, Height = BlockLength.Unbounded };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(Bounding.Bounded, Bounding.Unbounded));
        }

        [Fact]
        public void Should_return_unbounded_when_size_is_unbounded()
        {
            var block = new FixedSizeBlock { Size = UnboundedBlockSize.Unbounded };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.Unbounded);
        }
    }
}
