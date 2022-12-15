using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

/// <summary>
/// Renders text.
/// </summary>
public class TextBlock : UiBlock
{
    public string? Text { get; set; }

    public uint TabWidth { get; set; } = 4;

    /// <inheritdoc />
    public override DesiredBlockSize CalcDesiredSize(BlockSize maxSize) => DesiredBlockSize.Unbounded;

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        var textSpan = Text.AsSpan();

        var writeRow = 0;
        var writeCol = 0;

        var lastBreakableChar = 0;
        var lastWordStartInRow = 0;
        for (int i = 0; i < textSpan.Length; i++)
        {
            var current = textSpan[i];

            switch (current)
            {
                case '\n':
                    writeRow++;
                    writeCol = 0;
                    break;
                case '\r':
                    writeCol = 0;
                    break;
                case '\t':
                    lastBreakableChar = i;
                    for (int t = 0; t < TabWidth; t++)
                    {
                        lastWordStartInRow = writeCol + 1;

                        buffer[writeRow, writeCol] = ' ';
                        var wrapped = IncrementPosition(buffer, textSpan,
                            ref i, ref writeRow, ref writeCol, ref lastBreakableChar, ref lastWordStartInRow
                        );

                        if (wrapped) break;
                    }

                    break;
                default:
                    buffer[writeRow, writeCol] = current;

                    if (IsBreakable(current) != BreakMode.NotBreakable)
                    {
                        lastBreakableChar = i;
                        lastWordStartInRow = writeCol + 1;
                    }

                    IncrementPosition(buffer, textSpan,
                        ref i, ref writeRow, ref writeCol, ref lastBreakableChar, ref lastWordStartInRow
                    );

                    break;
            }

            if ((writeRow + 1 == buffer.Height && writeCol >= buffer.Width) || writeRow + 1 > buffer.Height)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Increments to the next row and/or column to write to.
    /// </summary>
    /// <returns>Whether we wrapped to the next line</returns>
    private static bool IncrementPosition(
        Span2D<char> buffer,
        ReadOnlySpan<char> textSpan,
        ref int i,
        ref int row,
        ref int col,
        ref int lastBreakableChar,
        ref int lastWordStartInRow
    )
    {
        if (col + 1 < buffer.Width)
        {
            col++;
            return false;
        }

        row++;
        col = 0;

        if (i + 1 < textSpan.Length)
        {
            // we're going to overflow
            var next = textSpan[i + 1];
            if (lastWordStartInRow == 0) return true;

            switch (IsBreakable(next))
            {
                case BreakMode.BreakAndKeepWithPrevious:
                case BreakMode.NotBreakable:
                    buffer.GetRowSpan(row - 1)[lastWordStartInRow..].Fill(' ');
                    i = lastBreakableChar;
                    break;
                case BreakMode.BreakAndRemove:
                    i++;
                    break;
            }
        }


        lastWordStartInRow = 0;
        return true;
    }

    private static BreakMode IsBreakable(char c)
    {
        return c switch
        {
            ' ' or '\n' => BreakMode.BreakAndRemove,
            '-' or '\t' => BreakMode.BreakAndKeepWithPrevious,
            _           => BreakMode.NotBreakable
        };
    }
}

internal enum BreakMode { NotBreakable, BreakAndRemove, BreakAndKeepWithPrevious }
