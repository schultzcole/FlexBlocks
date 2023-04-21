using System.Collections.Generic;
using System.Linq;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;

namespace Playground.Blocks;

public class SidebarPanel : CustomBlock
{
    public override UiBlock Content { get; }

    public SidebarPanel(IEnumerable<string> items)
    {
        Content = new BorderBlock
        {
            Overlay = new TitleOverlay { Title = "Sidebar", Offset = 2 },
            Border = Borders.Line,
            Padding = new Padding(1, 3),
            Content = new FixedSizeBlock
            {
                Size = UnboundedBlockSize.From(30, BlockLength.Unbounded),
                Content = new FlexBlock
                {
                    Direction = FLexDirection.Vertical,
                    Wrapping = FlexWrapping.Wrap,
                    Contents = items
                        .Select(x => new TextBlock { Text = x })
                        .Cast<UiBlock>()
                        .ToList()
                },
            }
        };
    }
}
