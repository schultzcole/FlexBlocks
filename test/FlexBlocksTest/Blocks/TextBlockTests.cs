﻿using CommunityToolkit.HighPerformance;
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
    }

    public class Render
    {
        [Fact]
        public void Should_render_to_one_row_when_string_is_shorter_than_buffer_width()
        {
            var textBlock = new TextBlock { Text = "ab cd ef" };
            var buffer = new char[3, 30];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "ab cd ef����������������������",
                "������������������������������",
                "������������������������������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_on_word_boundary()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var buffer = new char[3, 11];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo",
                "charlie����",
                "�����������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_mid_word()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie" };
            var buffer = new char[3, 14];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo���",
                "charlie�������",
                "��������������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_on_hyphen()
        {
            var textBlock = new TextBlock { Text = "alpha bravo-charlie" };
            var buffer = new char[3, 14];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo-��",
                "charlie�������",
                "��������������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_three_rows_when_string_wraps_exactly_on_hyphen()
        {
            var textBlock = new TextBlock { Text = "alpha bravo-charlie" };
            var buffer = new char[3, 11];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha������",
                "bravo-�����",
                "charlie����",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_contains_newline()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\ncharlie" };
            var buffer = new char[3, 30];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo�������������������",
                "charlie�����������������������",
                "������������������������������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_when_string_wraps_mid_tab()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\tcharlie", TabWidth = 4};
            var buffer = new char[3, 13];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo��",
                "charlie������",
                "�������������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_to_two_rows_with_no_leading_spaces_when_string_wraps_at_start_of_tab()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\tcharlie", TabWidth = 4};
            var buffer = new char[3, 11];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo",
                "charlie����",
                "�����������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_new_paragraph_with_leading_tabs()
        {
            var textBlock = new TextBlock { Text = "alpha bravo\n\tcharlie", TabWidth = 4};
            var buffer = new char[3, 15];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo����",
                "    charlie����",
                "���������������",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_all_that_can_fit_if_string_is_larger_than_buffer()
        {
            var textBlock = new TextBlock { Text = "alpha bravo charlie delta echo", TabWidth = 4};
            var buffer = new char[2, 15];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('�');
            textBlock.Render(bufferSpan);

            var expected = new []
            {
                "alpha bravo����",
                "charlie delta��",
            }.ToCharGrid();

            buffer.Should().BeEquivalentTo(expected);
        }
    }

    public class ExpandText
    {
        [Fact]
        public void Should_return_same_text_when_input_has_no_tabs()
        {
            var actualExpandedText = TextBlock.ExpandText("some text", 2);
            actualExpandedText.ToArray().Should().BeEquivalentTo("some text");
        }

        [Fact]
        public void Should_return_text_with_spaces_instead_of_tabs()
        {
            var actualExpandedText = TextBlock.ExpandText("some\ttext", 2);
            actualExpandedText.ToArray().Should().BeEquivalentTo("some  text");
        }

        [Fact]
        public void Should_return_text_with_spaces_instead_of_two_tabs()
        {
            var actualExpandedText = TextBlock.ExpandText("some\tmore\ttext", 2);
            actualExpandedText.ToArray().Should().BeEquivalentTo("some  more  text");
        }

        [Fact]
        public void Should_return_text_with_spaces_instead_of_two_tabs_in_a_row()
        {
            var actualExpandedText = TextBlock.ExpandText("some\t\ttext", 2);
            actualExpandedText.ToArray().Should().BeEquivalentTo("some    text");
        }
    }
}