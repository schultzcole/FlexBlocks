using JetBrains.Annotations;

namespace FlexBlocks.Renderables;

/// <summary>
/// A Collection of pre-defined <see cref="Pattern"/>s.
/// </summary>
[PublicAPI]
public static class Patterns
{
    /// <summary>A pattern that clears the render buffer.</summary>
    public static FillPattern BlankPattern => new() { Character = ' ' };

    /// <summary>A pattern that fills the render buffer with a single given character.</summary>
    public static FillPattern Fill(char c) => new() { Character = c };

    /// <summary>A pattern that alternates between the two given characters.</summary>
    public static CheckerboardPattern CheckerboardPattern(char a, char b) => new() { Characters = (a, b) };
}
