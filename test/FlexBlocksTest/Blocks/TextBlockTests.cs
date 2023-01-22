using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;

namespace FlexBlocksTest.Blocks;

public class TextBlockTests
{
    public class CalcMaxSize
    {
        [Fact]
        public void Should_always_return_unbounded()
        {
            var textBlock = new TextBlock();
            var actualMaxSize = textBlock.CalcMaxSize();
            actualMaxSize.Should().Be(UnboundedBlockSize.Unbounded);
        }
    }

    public class CalcSize
    {
        [Fact]
        public void Should_return_given_width_and_one_row_when_string_is_shorter_than_buffer_width()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var actualSize = textBlock.CalcSize(BlockSize.From(30, 3));
            actualSize.Should().Be(BlockSize.From(30, 1));
        }

        [Fact]
        public void Should_return_given_width_and_two_rows_when_string_wraps_on_word_boundary()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var actualSize = textBlock.CalcSize(BlockSize.From(11, 3));
            actualSize.Should().Be(BlockSize.From(11, 2));
        }

        [Fact]
        public void Should_return_given_width_and_two_rows_when_string_wraps_mid_word()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var actualSize = textBlock.CalcSize(BlockSize.From(15, 3));
            actualSize.Should().Be(BlockSize.From(15, 2));
        }

        [Fact]
        public void Should_return_given_width_and_two_rows_when_string_wraps_on_hyphen()
        {
            var textBlock = new TextBlock { Text = "alpha bravo-charlie" };
            var actualSize = textBlock.CalcSize(BlockSize.From(12, 3));
            actualSize.Should().Be(BlockSize.From(12, 2));
        }

        [Fact]
        public void Should_return_given_width_and_two_rows_when_string_contains_newline()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\ncharlie" };
            var actualSize = textBlock.CalcSize(BlockSize.From(30, 3));
            actualSize.Should().Be(BlockSize.From(30, 2));
        }
    }

    public class Render
    {
        [Fact]
        public void Should_render_to_one_row_when_string_is_shorter_than_buffer_width()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var buffer = new char[3, 30];
            var bufferSpan = new Span2D<char>(buffer);
            bufferSpan.Fill(' ');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo charlie           ",
                "                              ",
                "                              ",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_on_word_boundary()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var buffer = new char[3, 11];
            var bufferSpan = new Span2D<char>(buffer);
            bufferSpan.Fill(' ');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo",
                "charlie    ",
                "           ",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_mid_word()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var buffer = new char[3, 14];
            var bufferSpan = new Span2D<char>(buffer);
            bufferSpan.Fill(' ');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo   ",
                "charlie       ",
                "              ",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_on_hyphen()
        {
            var textBlock = new TextBlock { Text = "alpha bravo-charlie" };
            var buffer = new char[3, 14];
            var bufferSpan = new Span2D<char>(buffer);
            bufferSpan.Fill(' ');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo-  ",
                "charlie       ",
                "              ",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_three_rows_when_string_wraps_exactly_on_hyphen()
        {
            var textBlock = new TextBlock { Text = "alpha bravo-charlie" };
            var buffer = new char[3, 11];
            var bufferSpan = new Span2D<char>(buffer);
            bufferSpan.Fill(' ');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha      ",
                "bravo-     ",
                "charlie    ",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_contains_newline()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\ncharlie" };
            var buffer = new char[3, 30];
            var bufferSpan = new Span2D<char>(buffer);
            bufferSpan.Fill(' ');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo                   ",
                "charlie                       ",
                "                              ",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }
    }
}
