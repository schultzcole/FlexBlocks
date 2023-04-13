using FlexBlocks.BlockProperties;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static FlexBlocks.BlockProperties.Corner;
using static FlexBlocks.BlockProperties.Edge;
using static FlexBlocks.BlockProperties.Side;
using static FlexBlocks.BlockProperties.LineStyle;

namespace FlexBlocksTest.BlockProperties;

public class StyledBorderBuilderTests
{
    [Theory]
    [InlineData(TopLeft,     None,  None,  null)]
    [InlineData(TopLeft,     None,  Thin,  '─')]
    [InlineData(TopLeft,     Thin,  None,  '│')]
    [InlineData(TopLeft,     Thin,  Thin,  '┌')]
    [InlineData(TopRight,    Thick, Thin,  '┒')]
    [InlineData(BottomRight, Thin,  Thick, '┙')]
    [InlineData(BottomLeft,  Thick, Thick, '┗')]
    [InlineData(TopLeft,     Dual,  Thin,  '╓')]
    [InlineData(TopRight,    Thin,  Dual,  '╕')]
    [InlineData(BottomRight, Dual,  Dual,  '╝')]
    [InlineData(BottomLeft,  Dual,  Thick, '╙')] // there is no character for double/thick pairings
    public void StyleCorner_should_return_correct_character(
        Corner corner,
        LineStyle vStyle,
        LineStyle hStyle,
        char? expected
    )
    {
        var actual = StyledBorderBuilder.StyleCorner(corner, vStyle, hStyle);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(Horizontal, None,  null)]
    [InlineData(Vertical,   None,  null)]
    [InlineData(Horizontal, Thin,  '─')]
    [InlineData(Vertical,   Thin,  '│')]
    [InlineData(Horizontal, Thick, '━')]
    [InlineData(Vertical,   Thick, '┃')]
    [InlineData(Horizontal, Dual,  '═')]
    [InlineData(Vertical,   Dual,  '║')]
    public void StyleEdge_should_return_correct_character(
        Edge edge,
        LineStyle style,
        char? expected
    )
    {
        var actual = StyledBorderBuilder.StyleEdge(edge, style);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(None,  None,  null)]
    [InlineData(None,  Thin,  '─')]
    [InlineData(Thin,  None,  '│')]
    [InlineData(Thin,  Thin,  '┼')]
    [InlineData(Thin,  Thick, '┿')]
    [InlineData(Thick, Thin,  '╂')]
    [InlineData(Thick, Thick, '╋')]
    [InlineData(Dual,  Dual,  '╬')]
    [InlineData(Dual,  Thin,  '╫')]
    [InlineData(Thin,  Dual,  '╪')]
    [InlineData(Dual,  Thick, '╫')]
    [InlineData(Thick, Dual,  '╪')]
    public void StyleCenterIntersection_should_return_correct_character(
        LineStyle vStyle,
        LineStyle hStyle,
        char? expected
    )
    {
        var actual = StyledBorderBuilder.StyleCenterIntersection(vStyle, hStyle);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(Top,    None,  None,  null)]
    [InlineData(Top,    None,  Thin,  '─')]
    [InlineData(Top,    Thin,  None,  '│')]
    [InlineData(Top,    Thin,  Thin,  '┬')]
    [InlineData(Top,    Thin,  Thick, '┯')]
    [InlineData(Top,    Thick, Thin,  '┰')]
    [InlineData(Top,    Thick, Thick, '┳')]
    [InlineData(Right,  Thin,  Thin,  '┤')]
    [InlineData(Right,  Thin,  Thick, '┥')]
    [InlineData(Right,  Thick, Thin,  '┨')]
    [InlineData(Right,  Thick, Thick, '┫')]
    [InlineData(Bottom, Thin,  Thin,  '┴')]
    [InlineData(Bottom, Thin,  Thick, '┷')]
    [InlineData(Bottom, Thick, Thin,  '┸')]
    [InlineData(Bottom, Thick, Thick, '┻')]
    [InlineData(Left,   Thin,  Thin,  '├')]
    [InlineData(Left,   Thin,  Thick, '┝')]
    [InlineData(Left,   Thick, Thin,  '┠')]
    [InlineData(Left,   Thick, Thick, '┣')]
    [InlineData(Top,    Dual,  Dual,  '╦')]
    [InlineData(Top,    Thin,  Dual,  '╤')]
    [InlineData(Top,    Dual,  Thin,  '╥')]
    [InlineData(Right,  Dual,  Dual,  '╣')]
    [InlineData(Bottom, Dual,  Dual,  '╩')]
    [InlineData(Left,   Dual,  Dual,  '╠')]
    public void StyleSideIntersection_should_return_correct_character(
        Side intersection,
        LineStyle vStyle,
        LineStyle hStyle,
        char? expected
    )
    {
        var actual = StyledBorderBuilder.StyleSideIntersection(intersection, vStyle, hStyle);
        actual.Should().Be(expected);
    }

    public class Build
    {
        private readonly ITestOutputHelper _output;
        public Build(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_return_all_thin_border_when_called_with_all_thin()
        {
            var border = Borders.Builder().All(Thin).Build();
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
        public void Should_return_all_thin_border_when_called_with_all_thick()
        {
            var border = Borders.Builder().All(Thick).Build();
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
            var border = Borders.Builder().All(Dual).Build();
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
            var border = Borders.Builder().Outer(Thin).Inner(Dual).Build();
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
