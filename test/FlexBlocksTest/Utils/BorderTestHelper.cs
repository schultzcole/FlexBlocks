using FlexBlocks.BlockProperties;
using Xunit.Abstractions;

namespace FlexBlocksTest.Utils;

public static class BorderTestHelper
{
    public static string Demo(Border border)
    {
        return $"""
        {border.TopLeft}{border.Top}{border.TopT}{border.TopRight}
        {border.Left} {border.InteriorVertical}{border.Right}
        {border.LeftT}{border.InteriorHorizontal}{border.InteriorJunction}{border.RightT}
        {border.BottomLeft}{border.Bottom}{border.BottomT}{border.BottomRight}
        """;
    }

    public static void OutputBorderDemos(ITestOutputHelper output, string actual, string expected)
    {
        output.WriteLine("\nActual");
        output.WriteLine(actual);
        output.WriteLine("\nExpected");
        output.WriteLine(expected);
    }
}
