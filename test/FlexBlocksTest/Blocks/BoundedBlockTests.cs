using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FluentAssertions;
using Xunit;

namespace FlexBlocksTest.Blocks;

public class BoundedBlockTests
{
    public class GetBounds
    {
        [Theory]
        [CombinatorialData]
        public void Should_return_bounded_when_no_content(
            [CombinatorialValues(null, 5)] int? blockWidth,
            [CombinatorialValues(null, 5)] int? blockHeight
        )
        {
            var block = new BoundedBlock { Width = blockWidth, Height = blockHeight };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.Bounded);
        }

        [Theory]
        [InlineData(5,    5,    Bounding.Bounded,   Bounding.Bounded)]
        [InlineData(null, 5,    Bounding.Unbounded, Bounding.Bounded)]
        [InlineData(5,    null, Bounding.Bounded,   Bounding.Unbounded)]
        [InlineData(null, null, Bounding.Unbounded, Bounding.Unbounded)]
        public void Should_return_content_bounds_when_unbounded(
            int? contentWidth,
            int? contentHeight,
            Bounding expectedHorizBounding,
            Bounding expectedVertBounding
        )
        {
            var content = new FixedSizeBlock { Width = contentWidth, Height = contentHeight };
            var block = new BoundedBlock { Content = content, MaxSize = UnboundedBlockSize.Unbounded };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(expectedHorizBounding, expectedVertBounding));
        }

        [Theory]
        [InlineData(5,    5,    Bounding.Bounded,   Bounding.Bounded)]
        [InlineData(null, 5,    Bounding.Unbounded, Bounding.Bounded)]
        [InlineData(5,    null, Bounding.Bounded,   Bounding.Unbounded)]
        [InlineData(null, null, Bounding.Unbounded, Bounding.Unbounded)]
        public void Should_return_own_bounds_when_content_is_unbounded(
            int? blockWidth,
            int? blockHeight,
            Bounding expectedHorizBounding,
            Bounding expectedVertBounding
        )
        {
            var content = new FixedSizeBlock { Size = UnboundedBlockSize.Unbounded };
            var block = new BoundedBlock { Content = content, Width = blockWidth, Height = blockHeight};
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(expectedHorizBounding, expectedVertBounding));
        }
    }

    public class CalcSize
    {
        [Theory]
        [CombinatorialData]
        public void Should_return_zero_when_no_content(
            [CombinatorialValues(null, 5)] int? blockWidth,
            [CombinatorialValues(null, 5)] int? blockHeight
        )
        {
            var block = new BoundedBlock { Width = blockWidth, Height = blockHeight };
            var actualSize = block.CalcSize(BlockSize.From(5, 5));
            actualSize.Should().Be(BlockSize.Zero);
        }

        [Theory]
        [InlineData(5, 5, 5, 5)]
        [InlineData(null, 5, 13, 5)]
        [InlineData(5, null, 5, 17)]
        [InlineData(null, null, 13, 17)]
        public void Should_return_content_size_when_unbounded(
            int? contentWidth,
            int? contentHeight,
            int expectedWidth,
            int expectedHeight
        )
        {
            var content = new FixedSizeBlock { Width = contentWidth, Height = contentHeight };
            var block = new BoundedBlock { Content = content, MaxSize = UnboundedBlockSize.Unbounded };
            var actualSize = block.CalcSize(BlockSize.From(13, 17));
            actualSize.Should().Be(BlockSize.From(expectedWidth, expectedHeight));
        }

        [Theory]
        [InlineData(5, 5, 5, 5)]
        [InlineData(10, 5, 7, 5)]
        [InlineData(null, 5, 7, 5)]
        [InlineData(5, 12, 5, 11)]
        [InlineData(5, null, 5, 11)]
        public void Should_return_bounded_size(
            int? contentWidth,
            int? contentHeight,
            int expectedWidth,
            int expectedHeight
        )
        {
            var content = new FixedSizeBlock { Width = contentWidth, Height = contentHeight };
            var block = new BoundedBlock { Content = content, Width = 7, Height = 11 };
            var actualSize = block.CalcSize(BlockSize.From(13, 17));
            actualSize.Should().Be(BlockSize.From(expectedWidth, expectedHeight));
        }
    }
}
