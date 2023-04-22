using FlexBlocks.BlockProperties;
using Xunit.Abstractions;
using static FlexBlocks.BlockProperties.BorderInnerEdge;
using static FlexBlocks.BlockProperties.BorderOuterCorner;
using static FlexBlocks.BlockProperties.BorderOuterEdge;

namespace FlexBlocksTest.Utils;

public static class BorderTestHelper
{
    private const char BLANK = '×';

    public static string Demo(IBorder border)
    {
        var tl = border.OuterCorner(TopLeft) ?? BLANK;
        var tr = border.OuterCorner(TopRight) ?? BLANK;
        var br = border.OuterCorner(BottomRight) ?? BLANK;
        var bl = border.OuterCorner(BottomLeft) ?? BLANK;

        var t = border.OuterEdge(Top) ?? BLANK;
        var r = border.OuterEdge(Right) ?? BLANK;
        var b = border.OuterEdge(Bottom) ?? BLANK;
        var l = border.OuterEdge(Left) ?? BLANK;

        var tj = border.OuterJunction(Top) ?? BLANK;
        var rj = border.OuterJunction(Right) ?? BLANK;
        var bj = border.OuterJunction(Bottom) ?? BLANK;
        var lj = border.OuterJunction(Left) ?? BLANK;

        var iv = border.InnerEdge(Vertical) ?? BLANK;
        var ih = border.InnerEdge(Horizontal) ?? BLANK;

        var ij = border.InnerJunction() ?? BLANK;

        return $"""
        {tl}{t}{tj}{tr}
        {l} {iv}{r}
        {lj}{ih}{ij}{rj}
        {bl}{b}{bj}{br}
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
