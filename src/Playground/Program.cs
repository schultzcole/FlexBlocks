using System;
using FlexBlocks;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;
using FlexBlocks.Blocks.Debug;
using FlexBlocks.Renderables;
using FlexBlocks.Renderables.Debug;
using Playground;

var block = new BorderBlock
{
    Overlay = new CompositeRenderable
    {
        new DimensionsOverlay(),
        new RenderCountOverlay(),
        new FrametimeOverlay { ShowFps = true },
    },
    Border = Borders.Square,
    Padding = new Padding(2, 5),
    HorizontalSizing = Sizing.Fill,
    VerticalSizing = Sizing.Fill,
    Content = new BorderBlock
    {
        Overlay = new CompositeRenderable
        {
            new DimensionsOverlay(),
            new RenderCountOverlay(),
        },
        Border = Borders.Square,
        Padding = new Padding(1, 3),
        HorizontalSizing = Sizing.Content,
        VerticalSizing = Sizing.Fill,
        Content = new MyAnimatedBoundedBlock
        {
            DesiredSize = DesiredBlockSize.From(11, 15),
            Content = new RandomStringBlock(),
        },
    },
};

Console.Title = "FlexBlocks Playground";

Console.WriteLine("Hello, World!");
await FlexBlocksDriver.Run(block);
Console.WriteLine("Goodbye!");
