using FlexBlocks.Blocks;
using FlexBlocksTest.Utils;
using FluentAssertions;
using Xunit;

namespace FlexBlocksTest.Blocks;

public class GridBlockTests
{
    public class AddColumn
    {
        [Fact]
        public void Should_add_single_column_to_null_contents()
        {
            var block = new GridBlock();
            block.AddColumn(new DummyBlock(1), new DummyBlock(2));

            var expected = new UiBlock?[,]
            {
                { new DummyBlock(1) },
                { new DummyBlock(2) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_add_column_to_nonnull_empty_contents()
        {
            var block = new GridBlock { Contents = new UiBlock?[0, 1] };
            block.AddColumn(new DummyBlock(1), new DummyBlock(2));

            var expected = new UiBlock?[,]
            {
                { new DummyBlock(1) },
                { new DummyBlock(2) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_add_column_to_existing_contents()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    { new DummyBlock(1) },
                    { new DummyBlock(2) },
                }
            };
            block.AddColumn(new DummyBlock(3), new DummyBlock(4));

            var expected = new UiBlock?[,]
            {
                { new DummyBlock(1), new DummyBlock(3) },
                { new DummyBlock(2), new DummyBlock(4) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_add_column_even_if_it_has_fewer_rows_than_the_grid()
        {
            var block = new GridBlock { Contents = new UiBlock?[3, 1] };
            block.AddColumn(new DummyBlock(1), new DummyBlock(2));

            var expected = new UiBlock?[,]
            {
                { null, new DummyBlock(1) },
                { null, new DummyBlock(2) },
                { null, null },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_expand_number_of_rows_and_add_column_if_it_has_more_rows_than_the_grid()
        {
            var block = new GridBlock { Contents = new UiBlock?[2, 1] };

            block.AddColumn(new DummyBlock(1), new DummyBlock(2), new DummyBlock(3));

            var expected = new UiBlock?[,]
            {
                { null, new DummyBlock(1) },
                { null, new DummyBlock(2) },
                { null, new DummyBlock(3) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }
    }

    public class AddRow
    {
        [Fact]
        public void Should_add_single_row_to_null_contents()
        {
            var block = new GridBlock();
            block.AddRow(new DummyBlock(1), new DummyBlock(2));

            var expected = new UiBlock?[,]
            {
                { new DummyBlock(1), new DummyBlock(2) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_add_row_to_nonnull_empty_contents()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 0] };
            block.AddRow(new DummyBlock(1), new DummyBlock(2));

            var expected = new UiBlock?[,]
            {
                { new DummyBlock(1), new DummyBlock(2) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_add_row_to_existing_contents()
        {
            var block = new GridBlock
            {
                Contents = new UiBlock?[,]
                {
                    { new DummyBlock(1), new DummyBlock(2) },
                }
            };
            block.AddRow(new DummyBlock(3), new DummyBlock(4));

            var expected = new UiBlock?[,]
            {
                { new DummyBlock(1), new DummyBlock(2) },
                { new DummyBlock(3), new DummyBlock(4) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_add_roweven_if_it_has_fewer_columns_than_the_grid()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 3] };
            block.AddRow(new DummyBlock(1), new DummyBlock(2));

            var expected = new UiBlock?[,]
            {
                { null, null, null },
                { new DummyBlock(1), new DummyBlock(2), null },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_expand_number_of_columns_and_add_row_if_it_has_more_columns_than_the_grid()
        {
            var block = new GridBlock { Contents = new UiBlock?[1, 2] };

            block.AddRow(new DummyBlock(1), new DummyBlock(2), new DummyBlock(3));

            var expected = new UiBlock?[,]
            {
                { null, null, null },
                { new DummyBlock(1), new DummyBlock(2), new DummyBlock(3) },
            };
            block.Contents.Should().BeEquivalentTo(expected);
        }
    }
}
