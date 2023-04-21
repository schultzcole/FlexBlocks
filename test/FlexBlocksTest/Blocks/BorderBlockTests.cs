using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FlexBlocksTest.Blocks;

public class BorderBlockTests
{
    public class EffectivePadding
    {
        [Fact]
        public void Should_return_zero_when_no_border_and_no_padding()
        {
            var block = new BorderBlock();
            var actual = block.EffectivePadding;
            var expected = new Padding(0, 0, 0, 0);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_one_when_border_and_no_padding()
        {
            var block = new BorderBlock { Border = Borders.Line };
            var actual = block.EffectivePadding;
            var expected = new Padding(1, 1, 1, 1);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_existing_padding_when_no_border_and_padding()
        {
            var block = new BorderBlock { Padding = new Padding(1, 2, 3, 4) };
            var actual = block.EffectivePadding;
            var expected = new Padding(1, 2, 3, 4);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_combined_padding_when_border_and_padding()
        {
            var block = new BorderBlock
            {
                Border = Borders.Line,
                Padding = new Padding(1, 2, 3, 4)
            };
            var actual = block.EffectivePadding;
            var expected = new Padding(2, 3, 4, 5);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_partial_padding_when_partial_border()
        {
            var block = new BorderBlock
            {
                Border = Borders.LineBuilder().Top(LineStyle.Thin).Right(LineStyle.Thin).Bottom(LineStyle.Thin).Build()
            };
            var actual = block.EffectivePadding;
            var expected = new Padding(1, 1, 1, 0);
            actual.Should().Be(expected);
        }
    }

    public class Render
    {
        private readonly ITestOutputHelper _output;
        public Render(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_not_render_a_border_if_null()
        {
            var block = new BorderBlock
            {
                Content = new AlignableBlock { Background = Patterns.Fill('.'), Sizing = Sizing.Fill }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 6, 3);

            var expected = new[]
            {
                "......",
                "......",
                "......",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_border()
        {
            var block = new BorderBlock
            {
                Border = Borders.Line,
                Content = new AlignableBlock { Background = Patterns.Fill('.'), Sizing = Sizing.Fill }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 6, 4);

            var expected = new[]
            {
                "┌────┐",
                "│....│",
                "│....│",
                "└────┘",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_borders_with_null_side()
        {
            var block = new BorderBlock
            {
                Border = Borders.LineBuilder().All(LineStyle.Thin).Right(LineStyle.None).Build(),
                Content = new AlignableBlock { Background = Patterns.Fill('.'), Sizing = Sizing.Fill }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 6, 4);

            var expected = new[]
            {
                "┌─────",
                "│.....",
                "│.....",
                "└─────",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_borders_with_null_adjacent_sides()
        {
            var block = new BorderBlock
            {
                Border = Borders.LineBuilder().Top(LineStyle.Thin).Left(LineStyle.Thin).Build(),
                Content = new AlignableBlock { Background = Patterns.Fill('.'), Sizing = Sizing.Fill }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 6, 4);

            var expected = new[]
            {
                "┌─────",
                "│.....",
                "│.....",
                "│.....",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
