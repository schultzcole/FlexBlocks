using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FlexBlocksTest.Blocks;

public class GridBlockTests
{
    public class CalcMaxSize
    {
        [Fact]
        public void Should_return_zero_when_contents_null()
        {
            var block = new GridBlock();
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_has_zero_rows()
        {
            var block = new GridBlock { Contents = new UiBlock?[0, 1] };
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_has_zero_columns()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 0] };
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_has_row_and_column_but_no_blocks()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 1] };
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.Zero);
        }

        [Fact]
        public void Should_return_bounded_size_when_all_children_are_bounded()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(1, 2) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(3, 5) },
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(8, 13) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(21, 34) },
                    },
                },
            };
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.From(29, 39));
        }

        [Fact]
        public void Should_return_unbounded_width_when_at_least_one_child_has_unbounded_width()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(BlockLength.Unbounded, 2) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(3, 5) },
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(8, 13) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(21, 34) },
                    },
                },
            };
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.From(BlockLength.Unbounded, 39));
        }

        [Fact]
        public void Should_return_unbounded_height_when_at_least_one_child_has_unbounded_height()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(1, 2) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(3, 5) },
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(8, BlockLength.Unbounded) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(21, 34) },
                    },
                },
            };
            var actual = block.CalcMaxSize();
            actual.Should().Be(UnboundedBlockSize.From(29, BlockLength.Unbounded));
        }
    }

    public class CalcSize
    {
        [Fact]
        public void Should_return_zero_when_contents_null()
        {
            var block = new GridBlock();
            var actual = block.CalcSize(BlockSize.From(7, 4));
            actual.Should().Be(BlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_has_zero_rows()
        {
            var block = new GridBlock { Contents = new UiBlock?[0, 1] };
            var actual = block.CalcSize(BlockSize.From(7, 4));
            actual.Should().Be(BlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_has_zero_columns()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 0] };
            var actual = block.CalcSize(BlockSize.From(7, 4));
            actual.Should().Be(BlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_when_contents_has_row_and_column_but_no_blocks()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 1] };
            var actual = block.CalcSize(BlockSize.From(7, 4));
            actual.Should().Be(BlockSize.Zero);
        }

        [Fact]
        public void Should_return_size_of_single_bounded_child()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    { new FixedSizeBlock { Size = UnboundedBlockSize.From(4, 2) } }
                }
            };
            var actual = block.CalcSize(BlockSize.From(7, 4));
            actual.Should().Be(BlockSize.From(4, 2));
        }

        [Fact]
        public void Should_return_full_size_when_single_child_is_unbounded()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    { new FixedSizeBlock { Size = UnboundedBlockSize.Unbounded } }
                }
            };
            var actual = block.CalcSize(BlockSize.From(7, 4));
            actual.Should().Be(BlockSize.From(7, 4));
        }

        [Fact]
        public void Should_return_sum_of_children_when_multiple_fixed_size_children()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(1, 1) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(1, 3) }
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(4, 1) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 2) },
                    }
                }
            };
            var actual = block.CalcSize(BlockSize.From(20, 10));
            actual.Should().Be(BlockSize.From(9, 5));
        }
    }

    public class Render
    {
        private readonly ITestOutputHelper _output;
        public Render(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Should_render_nothing_when_contents_null()
        {
            var block = new GridBlock();

            var actual = BlockRenderTestHelper.RenderBlock(block, 7, 4);

            var expected = new[]
            {
                "×××××××",
                "×××××××",
                "×××××××",
                "×××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_nothing_when_contents_has_zero_rows()
        {
            var block = new GridBlock { Contents = new UiBlock?[0, 1] };

            var actual = BlockRenderTestHelper.RenderBlock(block, 7, 4);

            var expected = new[]
            {
                "×××××××",
                "×××××××",
                "×××××××",
                "×××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_nothing_when_contents_has_zero_columns()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 0] };

            var actual = BlockRenderTestHelper.RenderBlock(block, 7, 4);

            var expected = new[]
            {
                "×××××××",
                "×××××××",
                "×××××××",
                "×××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_nothing_when_contents_has_row_and_column_but_no_blocks()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 1] };

            var actual = BlockRenderTestHelper.RenderBlock(block, 7, 4);

            var expected = new[]
            {
                "×××××××",
                "×××××××",
                "×××××××",
                "×××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_single_child()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    { new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.From(4, 2) } }
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 7, 4);

            var expected = new[]
            {
                "1111×××",
                "1111×××",
                "×××××××",
                "×××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_bounded_children()
        {
            var block = new GridBlock
            {
                Background = null,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.From(1, 1) },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.From(2, 3) },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Size = UnboundedBlockSize.From(4, 1) },
                        new FixedSizeBlock { Background = Patterns.Fill('4'), Size = UnboundedBlockSize.From(5, 2) },
                    }
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 9, 5);

            var expected = new[]
            {
                "1×××22×××",
                "××××22×××",
                "××××22×××",
                "333344444",
                "××××44444",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_unbounded_children_at_end_of_grid()
        {
            var block = new GridBlock
            {
                Background = null,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.From(3, 2) },
                        null,
                    },
                    {
                        null,
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.Unbounded },
                    }
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 9, 5);

            var expected = new[]
            {
                "111××××××",
                "111××××××",
                "×××222222",
                "×××222222",
                "×××222222",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_unbounded_children_at_beginning_of_grid()
        {
            var block = new GridBlock
            {
                Background = null,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.Unbounded },
                        null,
                    },
                    {
                        null,
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.From(3, 2) },
                    }
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 9, 5);

            var expected = new[]
            {
                "111111×××",
                "111111×××",
                "111111×××",
                "××××××222",
                "××××××222",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_distribute_remaining_space_evenly_amongst_unbounded_children()
        {
            var block = new GridBlock
            {
                Background = null,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.From(1, 3) },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.Unbounded },
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Size = UnboundedBlockSize.Unbounded },
                    },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 9, 5);

            var expected = new[]
            {
                "122223333",
                "122223333",
                "122223333",
                "×22223333",
                "×22223333",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
