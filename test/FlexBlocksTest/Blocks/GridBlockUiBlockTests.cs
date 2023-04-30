using System;
using System.Collections.Generic;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FlexBlocksTest.Blocks;

public class GridBlockUiBlockTests
{
    public class GetBounds
    {
        [Fact]
        public void Should_return_bounded_when_contents_null()
        {
            var block = new GridBlock();
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_bounded_when_contents_has_zero_rows()
        {
            var block = new GridBlock { Contents = new UiBlock?[0, 1] };
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_bounded_when_contents_has_zero_columns()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 0] };
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_bounded_when_contents_has_row_and_column_but_no_blocks()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 1] };
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.Bounded);
        }

        [Fact]
        public void Should_return_bounded_when_all_children_are_bounded()
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
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.Bounded);
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
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.From(Bounding.Unbounded, Bounding.Bounded));
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
            var actual = block.GetBounds();
            actual.Should().Be(BlockBounds.From(Bounding.Bounded, Bounding.Unbounded));
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

        [Fact]
        public void Should_return_size_of_first_column_and_row_if_additional_columns_or_rows_overflow()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 5) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 5) }
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 5) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 5) },
                    }
                }
            };
            var actual = block.CalcSize(BlockSize.From(9, 6));
            actual.Should().Be(BlockSize.From(5, 5));
        }

        [Fact]
        public void Should_return_size_including_full_exterior_borders()
        {
            var block = new GridBlock
            {
                Border = Borders.Line,
                Contents = new UiBlock?[,] { { new FixedSizeBlock { Size = UnboundedBlockSize.From(2, 2) } } }
            };

            var actual = block.CalcSize(BlockSize.From(10, 8));
            actual.Should().Be(BlockSize.From(4, 4));
        }

        [Fact]
        public void Should_return_size_including_partial_exterior_borders()
        {
            var block = new GridBlock
            {
                Border = Borders.LineBuilder().Top(LineStyle.Thin).Bottom(LineStyle.Thin).Build(),
                Contents = new UiBlock?[,] { { new FixedSizeBlock { Size = UnboundedBlockSize.From(2, 2) } } }
            };

            var actual = block.CalcSize(BlockSize.From(10, 8));
            actual.Should().Be(BlockSize.From(2, 4));
        }

        [Fact]
        public void Should_return_size_with_multiple_rows_and_cols_including_exterior_and_interior_borders()
        {
            var block = new GridBlock
            {
                Border = Borders.Line,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 5) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(2, 2) }
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(3, 3) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(4, 4) },
                    }
                }
            };

            var actual = block.CalcSize(BlockSize.From(15, 15));
            actual.Should().Be(BlockSize.From(12, 12));
        }

        [Fact]
        public void Should_return_size_with_multiple_rows_and_cols_including_exterior_borders()
        {
            var block = new GridBlock
            {
                Border = Borders.LineBuilder().Outer(LineStyle.Thin).Build(),
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(5, 5) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(2, 2) }
                    },
                    {
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(3, 3) },
                        new FixedSizeBlock { Size = UnboundedBlockSize.From(4, 4) },
                    }
                }
            };

            var actual = block.CalcSize(BlockSize.From(15, 15));
            actual.Should().Be(BlockSize.From(11, 11));
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

        [Fact]
        public void Should_render_exterior_border()
        {
            var block = new GridBlock
            {
                Border = Borders.Line,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('.'), Size = UnboundedBlockSize.From(3, 3) },
                    },
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 6, 6);

            var expected = new[]
            {
                "┌───┐×",
                "│...│×",
                "│...│×",
                "│...│×",
                "└───┘×",
                "××××××",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_interior_borders()
        {
            var block = new GridBlock
            {
                Border = Borders.LineBuilder().Inner(LineStyle.Thin).Build(),
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 2, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Width = BlockLength.Unbounded, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 2, Height = 2 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('4'), Width = 2, Height = BlockLength.Unbounded },
                        new FixedSizeBlock { Background = Patterns.Fill('5'), Size = UnboundedBlockSize.Unbounded },
                        new FixedSizeBlock { Background = Patterns.Fill('6'), Width = 2, Height = BlockLength.Unbounded },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('7'), Width = 2, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('8'), Width = BlockLength.Unbounded, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('9'), Width = 2, Height = 2 },
                    },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 12, 12);

            var expected = new[]
            {
                "11│222222│33",
                "11│222222│33",
                "──┼──────┼──",
                "44│555555│66",
                "44│555555│66",
                "44│555555│66",
                "44│555555│66",
                "44│555555│66",
                "44│555555│66",
                "──┼──────┼──",
                "77│888888│99",
                "77│888888│99",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_exterior_and_interior_borders_with_fixed_size_children()
        {
            var block = new GridBlock
            {
                Border = Borders.Line,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Size = UnboundedBlockSize.From(5, 5) },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Size = UnboundedBlockSize.From(2, 2) }
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Size = UnboundedBlockSize.From(3, 3) },
                        new FixedSizeBlock { Background = Patterns.Fill('4'), Size = UnboundedBlockSize.From(4, 4) },
                    }
                }
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 12, 12);

            var expected = new[]
            {
                "┌─────┬────┐",
                "│11111│22  │",
                "│11111│22  │",
                "│11111│    │",
                "│11111│    │",
                "│11111│    │",
                "├─────┼────┤",
                "│333  │4444│",
                "│333  │4444│",
                "│333  │4444│",
                "│     │4444│",
                "└─────┴────┘",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_exterior_and_interior_borders_with_unbounded_children()
        {
            var block = new GridBlock
            {
                Border = Borders.Line,
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 2, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Width = BlockLength.Unbounded, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 2, Height = 2 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('4'), Width = 2, Height = BlockLength.Unbounded },
                        new FixedSizeBlock { Background = Patterns.Fill('5'), Size = UnboundedBlockSize.Unbounded },
                        new FixedSizeBlock { Background = Patterns.Fill('6'), Width = 2, Height = BlockLength.Unbounded },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('7'), Width = 2, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('8'), Width = BlockLength.Unbounded, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('9'), Width = 2, Height = 2 },
                    },
                },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 12, 12);

            var expected = new[]
            {
                "┌──┬────┬──┐",
                "│11│2222│33│",
                "│11│2222│33│",
                "├──┼────┼──┤",
                "│44│5555│66│",
                "│44│5555│66│",
                "│44│5555│66│",
                "│44│5555│66│",
                "├──┼────┼──┤",
                "│77│8888│99│",
                "│77│8888│99│",
                "└──┴────┴──┘",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_borders_with_interior_accent()
        {
            var block = new GridBlock
            {
                Border = Borders.LineBuilder().All(LineStyle.Thin).Accent(LineStyle.Dual).Build(),
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 3, Height = 2 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('4'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('5'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('6'), Width = 3, Height = 2 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('7'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('8'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('9'), Width = 3, Height = 2 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('0'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('A'), Width = 3, Height = 2 },
                        new FixedSizeBlock { Background = Patterns.Fill('B'), Width = 3, Height = 2 },
                    },
                },
                AccentRows = new List<Index> { 0, ^1 },
                AccentColumns = new List<Index> { 1 },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 13, 13);

            var expected = new[]
            {
                "┌───┬───╥───┐",
                "│111│222║333│",
                "│111│222║333│",
                "╞═══╪═══╬═══╡",
                "│444│555║666│",
                "│444│555║666│",
                "├───┼───╫───┤",
                "│777│888║999│",
                "│777│888║999│",
                "╞═══╪═══╬═══╡",
                "│000│AAA║BBB│",
                "│000│AAA║BBB│",
                "└───┴───╨───┘",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_interior_borders_in_only_one_direction()
        {
            var block = new GridBlock
            {
                Border = Borders.LineBuilder().InnerVertical(LineStyle.Thin).Accent(LineStyle.Dual).Build(),
                Contents = new UiBlock?[,]
                {
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('1'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('2'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('3'), Width = 3, Height = 1 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('4'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('5'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('6'), Width = 3, Height = 1 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('7'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('8'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('9'), Width = 3, Height = 1 },
                    },
                    {
                        new FixedSizeBlock { Background = Patterns.Fill('0'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('A'), Width = 3, Height = 1 },
                        new FixedSizeBlock { Background = Patterns.Fill('B'), Width = 3, Height = 1 },
                    },
                },
                AccentRows = new List<Index> { 0 },
            };

            var actual = BlockRenderTestHelper.RenderBlock(block, 11, 5);

            var expected = new[]
            {
                "111│222│333",
                "═══╪═══╪═══",
                "444│555│666",
                "777│888│999",
                "000│AAA│BBB",
            }.ToCharGrid();

            _output.WriteCharGrid(actual, expected);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
