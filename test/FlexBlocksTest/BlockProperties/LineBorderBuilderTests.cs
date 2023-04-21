using FlexBlocks.BlockProperties;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static FlexBlocks.BlockProperties.LineStyle;

namespace FlexBlocksTest.BlockProperties;

public class LineBorderBuilderTests
{
    public class Build
    {
        private readonly ITestOutputHelper _output;
        public Build(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_return_all_thin_border_when_called_with_all_thin()
        {
            var border = new LineBorderBuilder().All(Thin).Build();
            var actual = BorderTestHelper.Demo(border);
            var expected =
                """
                ┌─┬┐
                │ ││
                ├─┼┤
                └─┴┘
                """;
            BorderTestHelper.OutputBorderDemos(_output, actual, expected);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_all_thick_border_when_called_with_all_thick()
        {
            var border = new LineBorderBuilder().All(Thick).Build();
            var actual = BorderTestHelper.Demo(border);
            var expected =
                """
                ┏━┳┓
                ┃ ┃┃
                ┣━╋┫
                ┗━┻┛
                """;
            BorderTestHelper.OutputBorderDemos(_output, actual, expected);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_all_dual_border_when_called_with_all_dual()
        {
            var border = new LineBorderBuilder().All(Dual).Build();
            var actual = BorderTestHelper.Demo(border);
            var expected =
                """
                ╔═╦╗
                ║ ║║
                ╠═╬╣
                ╚═╩╝
                """;
            BorderTestHelper.OutputBorderDemos(_output, actual, expected);
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_return_border_with_correct_inner_and_outer_style()
        {
            var border = new LineBorderBuilder().Outer(Thin).Inner(Dual).Build();
            var actual = BorderTestHelper.Demo(border);
            var expected =
                """
                ┌─╥┐
                │ ║│
                ╞═╬╡
                └─╨┘
                """;
            BorderTestHelper.OutputBorderDemos(_output, actual, expected);
            actual.Should().Be(expected);
        }
    }
}
