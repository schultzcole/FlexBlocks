using FlexBlocks.BlockProperties;
using Xunit.Abstractions;
using static FlexBlocks.BlockProperties.BorderInnerEdge;
using static FlexBlocks.BlockProperties.BorderOuterCorner;
using static FlexBlocks.BlockProperties.BorderOuterEdge;

namespace FlexBlocksTest.Utils;

public static class BorderTestHelper
{
    public static string Demo(IBorder border)
    {
        var tl = border.OuterCorner(TopLeft);
        var tr = border.OuterCorner(TopRight);
        var br = border.OuterCorner(BottomRight);
        var bl = border.OuterCorner(BottomLeft);

        var t = border.OuterEdge(Top);
        var r = border.OuterEdge(Right);
        var b = border.OuterEdge(Bottom);
        var l = border.OuterEdge(Left);

        var tj = border.OuterJunction(Top);
        var rj = border.OuterJunction(Right);
        var bj = border.OuterJunction(Bottom);
        var lj = border.OuterJunction(Left);

        var iv = border.InnerEdge(Vertical);
        var ih = border.InnerEdge(Horizontal);

        var ij = border.InnerJunction();

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
