using JetBrains.Annotations;

namespace FlexBlocks.BlockProperties;

[PublicAPI]
public static class Borders
{
    /// <summary>Renders all sides and corners as a filled block.</summary>
    public static IBorder Block { get; } = new SimpleBorder { Character = '█' };

    /// <summary>Renders a border with straight edges and sharp, square corners.</summary>
    /// <example><code>
    /// ┌──┬─┐
    /// │  │ │
    /// ├──┼─┤
    /// └──┴─┘
    /// </code></example>
    public static IBorder Line { get; } = new LineBorderBuilder().All(LineStyle.Thin).Build();

    /// <summary>Renders a thick-lined border.</summary>
    /// <example><code>
    /// ┏━━┳━┓
    /// ┃  ┃ ┃
    /// ┣━━╋━┫
    /// ┗━━┻━┛
    /// </code></example>
    public static IBorder ThickLine { get; } = new LineBorderBuilder().All(LineStyle.Thick).Build();

    /// <summary>Renders a border with doubled edges.</summary>
    /// <example><code>
    /// ╔══╦═╗
    /// ║  ║ ║
    /// ╠══╬═╣
    /// ╚══╩═╝
    /// </code></example>
    public static IBorder DoubleLine { get; } = new LineBorderBuilder().All(LineStyle.Dual).Build();

    public static LineBorderBuilder LineBuilder() => new();
}
