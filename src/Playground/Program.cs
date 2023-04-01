using System;
using FlexBlocks;
using Playground.Blocks;

Console.Title = "FlexBlocks Playground";

Console.WriteLine("Hello, World!");
await FlexBlocksDriver.Run(new MainPanel());
Console.WriteLine("Goodbye!");
