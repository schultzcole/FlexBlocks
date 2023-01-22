using System;
using CommunityToolkit.HighPerformance;

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
}
