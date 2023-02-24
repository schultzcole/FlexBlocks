using System.Collections.Generic;
using CommunityToolkit.HighPerformance;
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
    public class CalcMaxSize
    {
        [Fact]
        public void Should_return_zero_if_contents_is_null()
        {
            var block = new FlexBlock { Contents = null };
            var actualMaxSize = block.CalcMaxSize();
            actualMaxSize.Should().Be(UnboundedBlockSize.Zero);
        }

        [Fact]
        public void Should_return_zero_if_contents_is_empty()
        {
            var block = new FlexBlock { Contents = new List<UiBlock>() };
            var actualMaxSize = block.CalcMaxSize();
            actualMaxSize.Should().Be(UnboundedBlockSize.Zero);
        }

        [Fact]
        public void Should_return_sum_of_contents_widths_when_all_are_bounded()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new BoundedBlock { Width = BlockLength.From(13) },
                    new BoundedBlock { Width = BlockLength.From(7) },
                }
            };
            var actualMaxSize = block.CalcMaxSize();
            actualMaxSize.Width.Should().Be(BlockLength.From(20));
        }

        [Fact]
        public void Should_return_unbounded_when_at_least_one_child_is_unbounded()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new BoundedBlock { Width = BlockLength.From(13) },
                    new BoundedBlock { Width = BlockLength.Unbounded },
                }
            };
            var actualMaxSize = block.CalcMaxSize();
            actualMaxSize.Width.Should().Be(BlockLength.Unbounded);
        }

        [Fact]
        public void Should_return_height_of_tallest_child()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new BoundedBlock { Height = BlockLength.From(13) },
                    new BoundedBlock { Height = BlockLength.From(7) },
                }
            };
            var actualMaxSize = block.CalcMaxSize();
            actualMaxSize.Height.Should().Be(BlockLength.From(13));
        }

        [Fact]
        public void Should_return_unbounded_height_when_at_least_one_child_is_unbounded()
        {
            var block = new FlexBlock {
                Contents = new List<UiBlock>
                {
                    new BoundedBlock { Height = BlockLength.From(13) },
                    new BoundedBlock { Height = BlockLength.Unbounded },
                }
            };
            var actualMaxSize = block.CalcMaxSize();
            actualMaxSize.Height.Should().Be(BlockLength.Unbounded);
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
                    new BoundedBlock { Width = BlockLength.From(13) },
                    new BoundedBlock { Width = BlockLength.From(7) },
                }
            };
            var maxSize = BlockSize.From(30, 1);
            var actualSize = block.CalcSize(maxSize);
            actualSize.Width.Should().Be(20);
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
                    new BoundedBlock
                    {
                        Background = Patterns.Fill('¤'),
                        MaxSize = UnboundedBlockSize.Unbounded
                    },
                },

            };
            var buffer = new char[4, 8];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('×');

            var container = new SimpleBlockContainer();
            container.RenderBlock(block, bufferSpan);

            var expected = new[]
            {
                "¤¤¤¤¤¤¤¤",
                "¤¤¤¤¤¤¤¤",
                "¤¤¤¤¤¤¤¤",
                "¤¤¤¤¤¤¤¤",
            }.ToCharGrid();

            _output.WriteCharGrid(buffer, expected);

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_multiple_unbound_children_evenly_distributed()
        {
            var block = new FlexBlock
            {
                Background = null,
                Contents = new List<UiBlock> {
                    new BoundedBlock
                    {
                        Background = Patterns.Fill('¤'),
                        MaxSize = UnboundedBlockSize.Unbounded
                    },
                    new BoundedBlock
                    {
                        Background = Patterns.Fill('◊'),
                        MaxSize = UnboundedBlockSize.Unbounded
                    },
                },

            };
            var buffer = new char[4, 8];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('×');

            var container = new SimpleBlockContainer();
            container.RenderBlock(block, bufferSpan);

            var expected = new[]
            {
                "¤¤¤¤◊◊◊◊",
                "¤¤¤¤◊◊◊◊",
                "¤¤¤¤◊◊◊◊",
                "¤¤¤¤◊◊◊◊",
            }.ToCharGrid();

            _output.WriteCharGrid(buffer, expected);

            buffer.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_render_one_bound_and_one_unbound_child()
        {
            var block = new FlexBlock
            {
                Background = null,
                Contents = new List<UiBlock> {
                    new BoundedBlock
                    {
                        Background = Patterns.Fill('¤'),
                        MaxSize = UnboundedBlockSize.From(BlockLength.From(2), BlockLength.Unbounded)
                    },
                    new BoundedBlock
                    {
                        Background = Patterns.Fill('◊'),
                        MaxSize = UnboundedBlockSize.Unbounded
                    },
                },

            };
            var buffer = new char[4, 8];
            var bufferSpan = buffer.AsSpan2D();
            bufferSpan.Fill('×');

            var container = new SimpleBlockContainer();
            container.RenderBlock(block, bufferSpan);

            var expected = new[]
            {
                "¤¤◊◊◊◊◊◊",
                "¤¤◊◊◊◊◊◊",
                "¤¤◊◊◊◊◊◊",
                "¤¤◊◊◊◊◊◊",
            }.ToCharGrid();

            _output.WriteCharGrid(buffer, expected);

            buffer.Should().BeEquivalentTo(expected);
        }
    }
}
