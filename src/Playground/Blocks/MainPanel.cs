using System.Collections.Generic;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Renderables;
using FlexBlocks.Renderables.Debug;

namespace Playground.Blocks;

public class MainPanel : CustomBlock
{
    private static readonly string[] alphabet =
    {
        "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliet", "Kilo", "Lima",
        "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey",
        "X-Ray", "Yankee", "Zulu"
    };

    public override UiBlock Content { get; } =
        new BorderBlock
        {
            Overlay = new CompositeRenderable
            {
                new FrametimeOverlay { ShowFps = true },
                new TitleOverlay { Title = " FlexBlocks Demo ", Offset = 2 }
            },
            Border = Borders.Line,
            Padding = new Padding(1, 3),
            Content = new FlexBlock
            {
                Contents = new List<UiBlock>
                {
                    new SidebarPanel(alphabet),
                    new FixedSizeBlock
                    {
                        Size = UnboundedBlockSize.Unbounded,
                        Content = new FlexBlock
                        {
                            Direction = FLexDirection.Vertical,
                            Contents = new List<UiBlock>
                            {
                                new GridContentPanel(),
                                new CommandPanel(alphabet),
                            }
                        },
                    }
                }
            }
        };
}
