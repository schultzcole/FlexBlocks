using System.Collections.Generic;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FlexBlocksTest.Blocks;

public class FlexBlockTests
{
    public class GetBounds
    {
        [Fact]
        public void Should_return_bounded_if_contents_is_null()
        {
            var block = new FlexBlock { Contents = null };
            var actualMaxSize = block.GetBounds();
            actualMaxSize.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_bounded_if_contents_is_empty()
        {
            var block = new FlexBlock { Contents = new List<UiBlock>() };
            var actualMaxSize = block.GetBounds();
            actualMaxSize.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_bounded_when_all_are_bounded()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new BoundedBlock { Width = 13, Height = 1 },
                    new BoundedBlock { Width = 7, Height = 1 },
                }
            };
            var actualMaxSize = block.GetBounds();
            actualMaxSize.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_unbounded_width_when_at_least_one_child_is_unbounded()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Width = 13, Height = 1 },
                    new FixedSizeBlock { Width = BlockLength.Unbounded, Height = 1 },
                }
            };
            var actualBounds = block.GetBounds();
            actualBounds.Should().Be(BlockBounds.From(Bounding.Unbounded, Bounding.Bounded));
        }

        [Fact]
        public void Should_return_unbounded_height_when_at_least_one_child_is_unbounded()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Height = 13, Width = 1 },
                    new FixedSizeBlock { Height = BlockLength.Unbounded, Width = 1 },
                }
            };
            var actualMaxSize = block.GetBounds();
            actualMaxSize.Should().Be(BlockBounds.From(Bounding.Bounded, Bounding.Unbounded));
        }
    }

    public class CalcSize
    {
        [Fact]
        public void Should_return_zero_when_contents_is_null()
        {
            var block = new FlexBlock { Contents = null };
            var maxSize = BlockSize.From(5, 5);
            var actualSize = block.CalcSize(maxSize);
            actualSize.Should().Be(BlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_is_empty()
        {
            var block = new FlexBlock { Contents = new List<UiBlock>() };
            var maxSize = BlockSize.From(5, 5);
            var actualSize = block.CalcSize(maxSize);
            actualSize.Should().Be(BlockSize.Zero);
        }

        [Fact]
        public void Should_return_sum_of_widths_when_max_size_is_large_enough()
        {
            var block = new FlexBlock
            {
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Width = 13 },
                    new FixedSizeBlock { Width = 7 },
                }
            };
            var maxSize = BlockSize.From(30, 1);
            var actualSize = block.CalcSize(maxSize);
            var expectedSize = BlockSize.From(20, 1);
            actualSize.Should().Be(expectedSize);
        }

        [Fact]
        public void Should_not_include_children_that_overextend_beyond_max_width()
        {
            var block = new FlexBlock
            {
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Width = 7 },
                    new FixedSizeBlock { Width = 11 },
                    new FixedSizeBlock { Width = 17 },
                }
            };
            var maxSize = BlockSize.From(30, 1);
            var actualSize = block.CalcSize(maxSize);
            var expectedSize = BlockSize.From(18, 1);
            actualSize.Should().Be(expectedSize);
        }

        [Fact]
        public void Should_wrap_children_to_next_line_when_they_extend_beyond_max_width_and_wrapping_is_enabled()
        {
            var block = new FlexBlock
            {
                Wrapping = FlexWrapping.Wrap,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 1) },
                    new FixedSizeBlock { Size = UnboundedBlockSize.From(7, 1) },
                    new FixedSizeBlock { Size = UnboundedBlockSize.From(17, 1) },
                }
            };
            var maxSize = BlockSize.From(20, 2);
            var actualSize = block.CalcSize(maxSize);
            var expectedSize = BlockSize.From(17, 2);
            actualSize.Should().Be(expectedSize);
        }

        [Fact]
        public void Should_wrap_children_and_properly_allocate_width_when_child_is_unbounded()
        {
            var block = new FlexBlock
            {
                Wrapping = FlexWrapping.Wrap,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Size = UnboundedBlockSize.From(7, 1) },
                    new FixedSizeBlock { Size = UnboundedBlockSize.From(BlockLength.Unbounded, 1) },
                    new FixedSizeBlock { Size = UnboundedBlockSize.From(7, 1) },
                }
            };
            var maxSize = BlockSize.From(13, 2);
            var actualSize = block.CalcSize(maxSize);
            var expectedSize = BlockSize.From(13, 2);
            actualSize.Should().Be(expectedSize);
        }
    }

    public class Render
    {
        private readonly ITestOutputHelper _output;
        public Render(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_render_single_unbound_child_to_full_width()
        {
            var block = new FlexBlock
            {
                Background = null,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.Unbounded },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new[]
            {
                "11111111",
                "11111111",
                "11111111",
                "11111111",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_multiple_unbound_children_evenly_distributed()
        {
            var block = new FlexBlock
            {
                Background = null,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.Unbounded },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.Unbounded },
                },

            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new[]
            {
                "11112222",
                "11112222",
                "11112222",
                "11112222",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_one_bound_and_one_unbound_child()
        {
            var block = new FlexBlock
            {
                Background = null,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 2, Height = BlockLength.Unbounded },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.Unbounded },
                },

            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new[]
            {
                "11222222",
                "11222222",
                "11222222",
                "11222222",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_bounded_height_child_at_desired_height()
        {
            var block = new FlexBlock
            {
                Background = null,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 2, Height = 2 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.Unbounded },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new[]
            {
                "11222222",
                "11222222",
                "××222222",
                "××222222",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_not_render_horizontally_overflowing_children_if_they_are_too_wide_for_parent_with_nowrap()
        {
            var block = new FlexBlock
            {
                Background = null,
                Wrapping = FlexWrapping.NoWrap,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 2 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 2 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 7, Height = 2 },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new[]
            {
                "111222××",
                "111222××",
                "××××××××",
                "××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_not_render_vertically_overflowing_children()
        {
            var block = new FlexBlock
            {
                Background = null,
                Wrapping = FlexWrapping.Wrap,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 2 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 6, Height = 2 },
                    new FixedSizeBlock { Background = Patterns.Fill('4'), Width = 2, Height = 1 },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 4);

            var expected = new[]
            {
                "111222××",
                "111222××",
                "×××222××",
                "××××××44",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_wrap_children_to_next_row_when_wrapping_is_enabled()
        {
            var block = new FlexBlock
            {
                Background = null,
                Wrapping = FlexWrapping.Wrap,
                Contents = new List<UiBlock> {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 2 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 7, Height = 2 },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 8, 5);

            var expected = new[]
            {
                "111222××",
                "111222××",
                "×××222××",
                "3333333×",
                "3333333×",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_unbounded_children_to_fill_remainder_of_line_when_wrapping_is_enabled()
        {
            var block = new FlexBlock
            {
                Background = null,
                Wrapping = FlexWrapping.Wrap,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 7, Height = 1 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = BlockLength.Unbounded, Height = 1 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = BlockLength.Unbounded, Height = 1 },
                    new FixedSizeBlock { Background = Patterns.Fill('4'), Width = 8, Height = 1 },
                    new FixedSizeBlock { Background = Patterns.Fill('5'), Width = BlockLength.Unbounded, Height = 1 },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 14, 2);

            var expected = new[]
            {
                "11111112222333",
                "44444444555555",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_wrap_blocks_correctly_when_buffer_width_is_less_than_buffer_height()
        {
            var block = new FlexBlock
            {
                Background = null,
                Wrapping = FlexWrapping.Wrap,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 2, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 2, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 2, Height = 3 },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 3, 9);

            var expected = new[]
            {
                "11×",
                "11×",
                "11×",
                "22×",
                "22×",
                "22×",
                "33×",
                "33×",
                "33×",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_children_vertically_when_flex_dir_is_vertical()
        {
            var block = new FlexBlock
            {
                Background = null,
                Direction = FLexDirection.Vertical,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 3, Height = 3 },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 9, 9);

            var expected = new[]
            {
                "111××××××",
                "111××××××",
                "111××××××",
                "222××××××",
                "222××××××",
                "222××××××",
                "333××××××",
                "333××××××",
                "333××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_unbounded_children_vertically_when_flex_dir_is_vertical()
        {
            var block = new FlexBlock
            {
                Background = null,
                Direction = FLexDirection.Vertical,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.Unbounded },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.Unbounded },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Size = UnboundedBlockSize.Unbounded },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 6, 9);

            var expected = new[]
            {
                "111111",
                "111111",
                "111111",
                "222222",
                "222222",
                "222222",
                "333333",
                "333333",
                "333333",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_wrap_children_to_top_of_next_column_when_flex_dir_is_vertical()
        {
            var block = new FlexBlock
            {
                Background = null,
                Wrapping = FlexWrapping.Wrap,
                Direction = FLexDirection.Vertical,
                Contents = new List<UiBlock>
                {
                    new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 3 },
                    new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 3, Height = 3 },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 9, 7);

            var expected = new[]
            {
                "111333×××",
                "111333×××",
                "111333×××",
                "222××××××",
                "222××××××",
                "222××××××",
                "×××××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
