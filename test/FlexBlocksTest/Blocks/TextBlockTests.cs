﻿using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FlexBlocksTest.Blocks;

public class TextBlockTests
{
    public class GetBounds
    {
        [Theory]
        [InlineData(null)]
        [InlineData("alpha bravo charlie")]
        [InlineData("alpha bravo\ncharlie")]
        [InlineData("alpha bravo\tcharlie")]
        public void Should_return_bounded(string? text)
        {
            var textBlock = new TextBlock { Text = text };
            var actualMaxSize = textBlock.GetBounds();
            actualMaxSize.Should().Be(BlockBounds.Bounded);
        }
    }

    public class CalcSize
    {
        [Fact]
        public void Should_return_given_width_and_one_row_when_string_is_shorter_than_buffer_width()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var actualSize = textBlock.CalcSize(BlockSize.From(30, 3));
            actualSize.Should().Be(BlockSize.From(19, 1));
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
            actualSize.Should().Be(BlockSize.From(11, 2));
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
            actualSize.Should().Be(BlockSize.From(11, 2));
        }

        [Fact]
        public void Should_not_return_greater_height_than_will_fit_in_max_height_when_newline_precedes_vertical_overflow()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\n\ncharlie delta" };
            var actualSize = textBlock.CalcSize(BlockSize.From(7, 3));
            actualSize.Should().Be(BlockSize.From(5, 3));
        }
    }

    public class Render
    {
        private readonly ITestOutputHelper _output;
        public Render(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_render_to_one_row_when_string_is_shorter_than_buffer_width()
        {
            var textBlock = new TextBlock { Background = null, Text = "ab cd ef" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 30, 3);

            var expected = new []
            {
                "ab cd ef××××××××××××××××××××××",
                "××××××××××××××××××××××××××××××",
                "××××××××××××××××××××××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_on_word_boundary()
        {
            var textBlock = new TextBlock { Background = null,  Text = "alpha bravo charlie" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 11, 3);

            var expected = new []
            {
                "alpha bravo",
                "charlie××××",
                "×××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_mid_word()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo charlie" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 14, 3);

            var expected = new []
            {
                "alpha bravo×××",
                "charlie×××××××",
                "××××××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_on_hyphen()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo-charlie" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 14, 3);

            var expected = new []
            {
                "alpha bravo-××",
                "charlie×××××××",
                "××××××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_three_rows_when_string_wraps_exactly_on_hyphen()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo-charlie" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 11, 3);

            var expected = new []
            {
                "alpha××××××",
                "bravo-×××××",
                "charlie××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_contains_newline()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo\ncharlie" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 30, 3);

            var expected = new []
            {
                "alpha bravo×××××××××××××××××××",
                "charlie×××××××××××××××××××××××",
                "××××××××××××××××××××××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_mid_tab()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo\tcharlie", TabWidth = 4};

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 13, 3);

            var expected = new []
            {
                "alpha bravo××",
                "charlie××××××",
                "×××××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_with_no_leading_spaces_when_string_wraps_at_start_of_tab()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo\tcharlie", TabWidth = 4};

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 11, 3);

            var expected = new []
            {
                "alpha bravo",
                "charlie××××",
                "×××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_new_paragraph_with_leading_tabs()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo\n\tcharlie", TabWidth = 4};

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 15, 3);

            var expected = new []
            {
                "alpha bravo××××",
                "    charlie××××",
                "×××××××××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_all_that_can_fit_if_string_is_larger_than_buffer()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo charlie delta echo" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 15, 2);

            var expected = new []
            {
                "alpha bravo××××",
                "charlie delta…×",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_ellipsize_multi_word_text_too_long_for_single_line_buffer()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 10, 1);

            var expected = new []
            {
                "alpha…××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_ellipsize_single_word_too_long_for_single_line_buffer()
        {
            var textBlock = new TextBlock { Background = null, Text = "november" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 7, 1);

            var expected = new []
            {
                "novemb…",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_wrap_mid_word_if_word_is_wider_than_buffer()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha biotechnological charlie" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 15, 3);

            var expected = new []
            {
                "alpha××××××××××",
                "biotechnologica",
                "l charlie××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_wrap_mid_word_if_word_is_one_character_wider_than_buffer()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 4, 2);

            var expected = new []
            {
                "alph",
                "a×××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_not_overflow_buffer_when_adding_ellipsis_when_wrap_happens_at_buffer_boundary()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 5, 1);

            var expected = new []
            {
                "alph…",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_ellipsize_text_with_a_newline_as_the_last_character_before_vertical_overflow()
        {
            var textBlock = new TextBlock { Background = null, Text = "alpha bravo\n\ncharlie delta" };

            var actual = BlockRenderTestHelper.RenderBlock(textBlock, 7, 3);

            var expected = new []
            {
                "alpha××",
                "bravo××",
                "…××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }
    }

    public class PreprocessText
    {
        [Fact]
        public void Should_return_same_text_when_input_has_no_tabs()
        {
            var actualExpandedText = TextBlock.PreprocessText("some text", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("some text");
        }

        [Fact]
        public void Should_return_text_with_spaces_instead_of_tabs()
        {
            var actualExpandedText = TextBlock.PreprocessText("some\ttext", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("some  text");
        }

        [Fact]
        public void Should_return_text_with_spaces_instead_of_two_tabs()
        {
            var actualExpandedText = TextBlock.PreprocessText("some\tmore\ttext", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("some  more  text");
        }

        [Fact]
        public void Should_return_text_with_spaces_instead_of_two_tabs_in_a_row()
        {
            var actualExpandedText = TextBlock.PreprocessText("some\t\ttext", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("some    text");
        }

        [Fact]
        public void Should_not_expand_trailing_tab()
        {
            var actualExpandedText = TextBlock.PreprocessText("some text\t", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("some text");
        }

        [Fact]
        public void Should_not_remove_newline()
        {
            var actualExpandedText = TextBlock.PreprocessText("some\ntext", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("some\ntext");
        }

        [Fact]
        public void Should_remove_control_characters()
        {
            var actualExpandedText = TextBlock.PreprocessText("some\b\rtext", 2);
            actualExpandedText.ToString().Should().BeEquivalentTo("sometext");
        }
    }
}
