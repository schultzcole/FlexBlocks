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

        var width = Console.BufferWidth;
        var height = Console.BufferHeight;
        Span<char> buffer1d = stackalloc char[width * height];
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

            for (var row = 0; row < buffer.Height; row++)
            {
                Console.SetCursorPosition(0, row);
                Console.Out.Write(buffer.GetRowSpan(row));
            }

            Thread.Sleep(50);
        }

        Console.CursorVisible = true;
        Console.WriteLine("Goodbye!");
    }
}
