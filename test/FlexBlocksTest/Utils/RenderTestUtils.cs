using System;
using CommunityToolkit.HighPerformance;
using Xunit.Abstractions;

namespace FlexBlocksTest.Utils;

public static class RenderTestUtils
{
    public static char[,] ToCharGrid(this string[] array)
    {
        var height = array.Length;
        var width = array[0].Length;

        var resultArray = new char[height, width];

        for (var index = 0; index < array.Length; index++)
        {
            var str = array[index];
            if (str.Length != width)
            {
                throw new InvalidOperationException("Input array is not a rectangular jagged array!");
            }
            str.AsSpan().CopyTo(resultArray.GetRowSpan(index));
        }

        return resultArray;
    }

    public static void WriteCharGrid(this ITestOutputHelper outputHelper, char[,] actual, char[,] expected)
    {
        outputHelper.WriteLine("\nActual");
        outputHelper.WriteCharGrid(actual);
        outputHelper.WriteLine("\nExpected");
        outputHelper.WriteCharGrid(expected);
    }

    public static void WriteCharGrid(this ITestOutputHelper outputHelper, char[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            outputHelper.WriteLine(grid.GetRowSpan(i).ToString());
        }
    }
}
