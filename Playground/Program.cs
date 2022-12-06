using System;
using System.Threading;
using CharConsole.Blocks;
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
            BorderType = BorderType.Rounded,
            Padding = new Padding(4, 8),
            HorizontalSizing = Sizing.Content,
            HorizontalContentAlignment = Alignment.End,
            Content = new RandomStringBlock
            {
                Width = 80,
                Height = 30
            }
        };

        while (loop)
        {
            block.Render(buffer);

            Console.SetCursorPosition(0, 0);
            Console.Out.Write(buffer1d);
            Console.SetCursorPosition(0, 0);

            Thread.Sleep(50);
        }

        Console.CursorVisible = true;
        Console.WriteLine("Goodbye!");
    }
}
