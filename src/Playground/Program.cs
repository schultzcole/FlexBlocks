using System;
using FlexBlocks.Blocks;

var block = new BorderBlock
{
    Border = Border.Square,
    // Padding = new Padding(0, 1),
    HorizontalSizing = Sizing.Fill,
    VerticalSizing = Sizing.Fill,
    // HorizontalContentAlignment = Alignment.Center,
    // VerticalContentAlignment = Alignment.Center,
    Content = new RandomStringBlock
    {
        Width = 10,
        Height = 10
    }
};

await FlexBlocksDriver.Run(block);

Console.WriteLine("Goodbye!");
