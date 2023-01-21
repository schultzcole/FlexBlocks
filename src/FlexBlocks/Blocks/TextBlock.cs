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
    public override UnboundedBlockSize CalcMaxSize() => UnboundedBlockSize.Unbounded;

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        return LayoutText(maxSize, Span2D<char>.Empty, false);
    }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        LayoutText(buffer.BlockSize(), buffer, true);
    }

    private BlockSize LayoutText(BlockSize maxSize, Span2D<char> buffer, bool renderToBuffer)
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
                {
                    writeRow++;
                    writeCol = 0;
                    break;
                }
                case '\r':
                {
                    writeCol = 0;
                    break;
                }
                case '\t':
                {
                    lastBreakableChar = i;
                    for (int t = 0; t < TabWidth; t++)
                    {
                        lastWordStartInRow = writeCol + 1;

                        if (renderToBuffer)
                        {
                            buffer[writeRow, writeCol] = ' ';
                        }

                        var wrapResult = IncrementPosition(maxSize.Width, textSpan,
                            ref i, ref writeRow, ref writeCol, ref lastBreakableChar, ref lastWordStartInRow
                        );

                        HandleWrapResult(buffer, wrapResult, writeRow, ref lastWordStartInRow, renderToBuffer);

                        if (wrapResult != WrapResult.DidntWrap) break;
                    }

                    break;
                }
                default:
                {
                    if (renderToBuffer)
                    {
                        buffer[writeRow, writeCol] = current;
                    }

                    if (IsBreakable(current) != BreakMode.NotBreakable)
                    {
                        lastBreakableChar = i;
                        lastWordStartInRow = writeCol + 1;
                    }

                    var wrapResult = IncrementPosition(maxSize.Width, textSpan,
                        ref i, ref writeRow, ref writeCol, ref lastBreakableChar, ref lastWordStartInRow
                    );

                    HandleWrapResult(buffer, wrapResult, writeRow, ref lastWordStartInRow, renderToBuffer);

                    break;
                }
            }

            if ((writeRow + 1 == maxSize.Height && writeCol >= maxSize.Width) || writeRow + 1 > maxSize.Height)
            {
                break;
            }
        }

        return BlockSize.From(maxSize.Width, writeRow + 1);
    }

    /// <summary>
    /// Increments to the next row and/or column to write to.
    /// </summary>
    /// <returns>Whether we wrapped to the next line</returns>
    private static WrapResult IncrementPosition(
        int maxWidth,
        ReadOnlySpan<char> textSpan,
        ref int i,
        ref int row,
        ref int col,
        ref int lastBreakableChar,
        ref int lastWordStartInRow
    )
    {
        if (col + 1 < maxWidth)
        {
            col++;
            return WrapResult.DidntWrap;
        }

        row++;
        col = 0;

        if (i + 1 < textSpan.Length)
        {
            // we're going to overflow
            var next = textSpan[i + 1];
            if (lastWordStartInRow == 0) return WrapResult.Wrap;

            switch (IsBreakable(next))
            {
                case BreakMode.BreakAndKeepWithPrevious:
                case BreakMode.NotBreakable:
                    i = lastBreakableChar;
                    return WrapResult.WrapClear;
                case BreakMode.BreakAndRemove:
                    i++;
                    break;
            }
        }

        return WrapResult.Wrap;
    }

    /// What type of wrap, if any, occurred when we incremented the write position?
    private enum WrapResult
    {
        // No wrap occurred, proceed as normal
        DidntWrap,
        // Wrapped in the middle of a word; need to clear the line so that we can start rewriting it on the next line
        WrapClear,
        // Wrapped, but on a breakable character. Proceed as normal
        Wrap,
    }

    private void HandleWrapResult(Span2D<char> buffer, WrapResult wrapResult, int writeRow, ref int lastWordStartInRow, bool renderToBuffer)
    {
        if (wrapResult != WrapResult.WrapClear) return;

        if (renderToBuffer)
        {
            buffer.GetRowSpan(writeRow - 1)[lastWordStartInRow..].Fill(' ');
        }
        lastWordStartInRow = 0;
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

    /// The "breakableness" of a text character
    private enum BreakMode
    {
        // Not allowed to break on this character, wrap the whole word to the next line
        NotBreakable,
        // Breaking on this character is allowed, and don't try to render it after wrapping
        BreakAndRemove,
        // Breaking on this character is allowed, but don't erase it from the current line and don't try to render it on the next line
        BreakAndKeepWithPrevious
    }
}
