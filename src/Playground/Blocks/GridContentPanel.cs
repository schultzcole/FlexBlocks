using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;

namespace Playground.Blocks;

public class GridContentPanel : CustomBlock
{
    private const int EDGE_HEIGHT = 5;
    private const int EDGE_WIDTH = 11;

    public override UiBlock Content { get; } =
        new GridBlock
        {
            Contents = new UiBlock[,]
            {
                {
                    new FixedSizeBlock { Background = Patterns.CheckerboardPattern('█', '▒'), Size = UnboundedBlockSize.From(EDGE_WIDTH, EDGE_HEIGHT) },
                    new FixedSizeBlock {
                        Size = UnboundedBlockSize.From(BlockLength.Unbounded, EDGE_HEIGHT),
                        Content = new BorderBlock
                        {
                            Border = Borders.Line,
                            Content = new AlignableBlock { Sizing = Sizing.Fill }
                        },
                    },
                    new FixedSizeBlock { Background = Patterns.CheckerboardPattern('█', '▒'), Size = UnboundedBlockSize.From(EDGE_WIDTH, EDGE_HEIGHT) },
                },
                {
                    new FixedSizeBlock {
                        Size = UnboundedBlockSize.From(EDGE_WIDTH, BlockLength.Unbounded),
                        Content = new BorderBlock
                        {
                            Border = Borders.Line,
                            Content = new AlignableBlock { Sizing = Sizing.Fill }
                        },
                    },
                    new ContentPanel(),
                    new FixedSizeBlock {
                        Size = UnboundedBlockSize.From(EDGE_WIDTH, BlockLength.Unbounded),
                        Content = new BorderBlock
                        {
                            Border = Borders.Line,
                            Content = new AlignableBlock { Sizing = Sizing.Fill }
                        },
                    },
                },
                {
                    new FixedSizeBlock { Background = Patterns.CheckerboardPattern('█', '▒'), Size = UnboundedBlockSize.From(EDGE_WIDTH, EDGE_HEIGHT) },
                    new FixedSizeBlock {
                        Size = UnboundedBlockSize.From(BlockLength.Unbounded, EDGE_HEIGHT),
                        Content = new BorderBlock
                        {
                            Border = Borders.Line,
                            Content = new AlignableBlock { Sizing = Sizing.Fill }
                        },
                    },
                    new FixedSizeBlock { Background = Patterns.CheckerboardPattern('█', '▒'), Size = UnboundedBlockSize.From(EDGE_WIDTH, EDGE_HEIGHT) },
                },
            }
        };
}
