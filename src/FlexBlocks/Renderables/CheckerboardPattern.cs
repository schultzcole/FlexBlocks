using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Renderables;

/// <summary>A pattern that alternates between two characters in a checkerboard pattern.</summary>
public class CheckerboardPattern : Pattern
{
    /// <summary>The chars with which to fill the render buffer.</summary>
    public required (char, char) Characters { get; set; }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        var (a, b) = Characters;
        Span<char> chars = stackalloc char[] { a, b };

        int currentChar = 0;
        for (int row = 0; row < buffer.Height; row++)
        {
            for (int col = 0; col < buffer.Width; col++)
            {
                buffer[row, col] = chars[currentChar];
                currentChar = (currentChar + 1) % 2;
            }

            currentChar = (row + 1) % 2;
        }
    }
}
