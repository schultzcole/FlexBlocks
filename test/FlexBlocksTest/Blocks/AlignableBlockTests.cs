using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FlexBlocksTest.Blocks;

public class AlignableBlockTests
{
    public class GetBounds
    {
        [Theory]
        [InlineData(Sizing.Fill,    Sizing.Fill,    Bounding.Unbounded, Bounding.Unbounded)]
        [InlineData(Sizing.Content, Sizing.Fill,    Bounding.Bounded,   Bounding.Unbounded)]
        [InlineData(Sizing.Fill,    Sizing.Content, Bounding.Unbounded, Bounding.Bounded)]
        [InlineData(Sizing.Content, Sizing.Content, Bounding.Bounded,   Bounding.Bounded)]
        public void Should_return_correct_size_with_no_content(
            Sizing hSizing,
            Sizing vSizing,
            Bounding expectedHoriz,
            Bounding expectedVert
        )
        {
            var block = new AlignableBlock { HorizontalSizing = hSizing, VerticalSizing = vSizing };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(expectedHoriz, expectedVert));
        }

        [Theory]
        [InlineData(Sizing.Fill,    Sizing.Fill,    Bounding.Unbounded, Bounding.Unbounded)]
        [InlineData(Sizing.Content, Sizing.Fill,    Bounding.Bounded,   Bounding.Unbounded)]
        [InlineData(Sizing.Fill,    Sizing.Content, Bounding.Unbounded, Bounding.Bounded)]
        [InlineData(Sizing.Content, Sizing.Content, Bounding.Bounded,   Bounding.Bounded)]
        public void Should_return_correct_size_with_fixed_size_content(
            Sizing hSizing,
            Sizing vSizing,
            Bounding expectedHoriz,
            Bounding expectedVert
        )
        {
            var content = new FixedSizeBlock { Width = 5, Height = 7 };
            var block = new AlignableBlock { Content = content, HorizontalSizing = hSizing, VerticalSizing = vSizing };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(expectedHoriz, expectedVert));
        }

        [Theory]
        [CombinatorialData]
        public void Should_return_correct_size_with_unbounded_size_content(Sizing hSizing, Sizing vSizing)
        {
            var content = new FixedSizeBlock { Size = UnboundedBlockSize.Unbounded };
            var block = new AlignableBlock { Content = content, HorizontalSizing = hSizing, VerticalSizing = vSizing };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.Unbounded);
        }
    }

    public class CalcSize
    {
        [Theory]
        [InlineData(Sizing.Fill, Sizing.Fill, 13, 17)]
        [InlineData(Sizing.Content, Sizing.Fill, 0, 17)]
        [InlineData(Sizing.Fill, Sizing.Content, 13, 0)]
        [InlineData(Sizing.Content, Sizing.Content, 0, 0)]
        public void Should_return_correct_size_with_no_content(
            Sizing hSizing,
            Sizing vSizing,
            int expectedWidth,
            int expectedHeight
        )
        {
            var block = new AlignableBlock { HorizontalSizing = hSizing, VerticalSizing = vSizing };
            var actualSize = block.CalcSize(BlockSize.From(13, 17));
            actualSize.Should().Be(BlockSize.From(expectedWidth, expectedHeight));
        }


        [Theory]
        [InlineData(Sizing.Fill, Sizing.Fill, 13, 17)]
        [InlineData(Sizing.Content, Sizing.Fill, 5, 17)]
        [InlineData(Sizing.Fill, Sizing.Content, 13, 7)]
        [InlineData(Sizing.Content, Sizing.Content, 5, 7)]
        public void Should_return_correct_size_with_fixed_size_content(
            Sizing hSizing,
            Sizing vSizing,
            int expectedWidth,
            int expectedHeight
        )
        {
            var content = new FixedSizeBlock { Width = 5, Height = 7 };
            var block = new AlignableBlock { Content = content, HorizontalSizing = hSizing, VerticalSizing = vSizing };
            var actualSize = block.CalcSize(BlockSize.From(13, 17));
            actualSize.Should().Be(BlockSize.From(expectedWidth, expectedHeight));
        }

        [Theory]
        [InlineData(Sizing.Fill, Sizing.Fill)]
        [InlineData(Sizing.Content, Sizing.Fill)]
        [InlineData(Sizing.Fill, Sizing.Content)]
        [InlineData(Sizing.Content, Sizing.Content)]
        public void Should_return_correct_size_with_unbounded_size_content(Sizing hSizing, Sizing vSizing)
        {
            var content = new FixedSizeBlock { Size = UnboundedBlockSize.Unbounded };
            var block = new AlignableBlock { Content = content, HorizontalSizing = hSizing, VerticalSizing = vSizing };
            var actualSize = block.CalcSize(BlockSize.From(13, 17));
            actualSize.Should().Be(BlockSize.From(13, 17));
        }
    }

    public class Render
    {
        private readonly ITestOutputHelper _output;
        public Render(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_render_content_in_top_left_when_hAlign_is_start_and_vAlign_is_start()
        {
            var block = new AlignableBlock
            {
                Background = null,
                Content = new FixedSizeBlock { Background = Patterns.Fill('¤'), Width = 2, Height = 2 },
                HorizontalContentAlignment = Alignment.Start,
                VerticalContentAlignment = Alignment.Start,
                HorizontalSizing = Sizing.Fill,
                VerticalSizing = Sizing.Fill,
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new []
            {
                "¤¤××××××",
                "¤¤××××××",
                "××××××××",
                "××××××××",
            }.ToCharGrid();


            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_content_in_middle_right_when_hAlign_is_end_and_vAlign_is_center()
        {
            var block = new AlignableBlock
            {
                Background = null,
                Content = new FixedSizeBlock { Background = Patterns.Fill('¤'), Width = 2, Height = 2 },
                HorizontalContentAlignment = Alignment.End,
                VerticalContentAlignment = Alignment.Center,
                HorizontalSizing = Sizing.Fill,
                VerticalSizing = Sizing.Fill,
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new []
            {
                "××××××××",
                "××××××¤¤",
                "××××××¤¤",
                "××××××××",
            }.ToCharGrid();


            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_content_in_bottom_center_when_hAlign_is_center_and_vAlign_is_end()
        {
            var block = new AlignableBlock
            {
                Background = null,
                Content = new FixedSizeBlock { Background = Patterns.Fill('¤'), Width = 2, Height = 2 },
                HorizontalContentAlignment = Alignment.Center,
                VerticalContentAlignment = Alignment.End,
                HorizontalSizing = Sizing.Fill,
                VerticalSizing = Sizing.Fill,
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new []
            {
                "××××××××",
                "××××××××",
                "×××¤¤×××",
                "×××¤¤×××",
            }.ToCharGrid();


            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
