using System.Collections.Generic;
using System.Linq;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;

namespace Playground.Blocks;

public class CommandPanel : CustomBlock
{
    /// <inheritdoc />
    public override UiBlock Content { get; }

    public CommandPanel(IEnumerable<string> commands)
    {
        Content = new BorderBlock
        {
            Border = Borders.Square,
            Content = new FlexBlock
            {
                HorizontalSizing = Sizing.Fill,
                VerticalSizing = Sizing.Content,
                HorizontalContentAlignment = Alignment.End,
                Wrapping = FlexWrapping.Wrap,
                Contents = commands
                    .Select(x => new BorderBlock
                        { Border = Borders.Square, Content = new TextBlock { Text = x } }
                    )
                    .Cast<UiBlock>()
                    .ToList()
            }
        };
    }
}
