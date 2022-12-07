﻿using System;
using System.Threading;
using FlexBlocks.Blocks;
using CommunityToolkit.HighPerformance;

namespace Playground;

internal class Program
{
    public static void Main(string[] args)
    {
        var loop = true;

        Console.CancelKeyPress += (_, evt) =>
        {
            evt.Cancel = true;
            loop = false;
        };

        Console.Clear();
        Console.CursorVisible = false;

        var width = Math.Min(Console.BufferWidth, Console.WindowWidth);
        var height = Math.Min(Console.BufferHeight, Console.WindowHeight);
        var bufferSize = width * height;
        Span<char> buffer1d = new char[bufferSize];
        buffer1d.Fill(' ');
        Span2D<char> buffer = Span2D<char>.DangerousCreate(ref buffer1d.DangerousGetReference(), height, width, 0);

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
                Width = 80,
                Height = 30
            }
        };

        while (loop)
        {
            var rootDesiredSize = block.CalcDesiredSize(buffer.GetSize());
            var rootBuffer = buffer.Slice(
                row: 0,
                column: 0,
                height: rootDesiredSize.Height,
                width: rootDesiredSize.Width
            );
            block.Render(rootBuffer);

            Console.SetCursorPosition(0, 0);
            Console.Out.Write(buffer1d);
            Console.SetCursorPosition(0, 0);
            // break;

            Thread.Sleep(50);
        }

        Console.CursorVisible = true;
        Console.WriteLine("Goodbye!");
    }
}
