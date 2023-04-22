using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;

namespace Playground.Blocks;

public class GridContentPanel : CustomBlock
{
    private const int EDGE_HEIGHT = 5;
    private const int EDGE_WIDTH = 11;

    private static readonly Pattern cornerBackground = Patterns.CheckerboardPattern('█', '▒');

    public override UiBlock Content { get; } =
        new GridBlock
        {
            Overlay = new TitleOverlay { Title = "Content", Offset = ^(EDGE_WIDTH + 4) },
            Border = Borders.Line,
            Contents = new UiBlock[,]
            {
                {
                    new FixedSizeBlock { Background = cornerBackground, Width = EDGE_WIDTH, Height = EDGE_HEIGHT },
                    new FixedSizeBlock { Width = BlockLength.Unbounded, Height = EDGE_HEIGHT },
                    new FixedSizeBlock { Background = cornerBackground, Width = EDGE_WIDTH, Height = EDGE_HEIGHT },
                },
                {
                    new FixedSizeBlock { Width = EDGE_WIDTH, Height = BlockLength.Unbounded },
                    new ContentPanel(),
                    new FixedSizeBlock { Width = EDGE_WIDTH, Height = BlockLength.Unbounded },
                },
                {
                    new FixedSizeBlock { Background = cornerBackground, Width = EDGE_WIDTH, Height = EDGE_HEIGHT },
                    new FixedSizeBlock { Width = BlockLength.Unbounded, Height = EDGE_HEIGHT },
                    new FixedSizeBlock { Background = cornerBackground, Width = EDGE_WIDTH, Height = EDGE_HEIGHT },
                },
            }
        };
}
