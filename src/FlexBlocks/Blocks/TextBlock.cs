using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks;

/// <summary>
/// Renders text.
/// </summary>
[PublicAPI]
public sealed class TextBlock : UiBlock
{
    private string? _text;

    /// <summary>The text to display in this block</summary>
    public string? Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _measureResult?.ClearLines();
            }

            _text = value;
        }
    }

    /// <summary>The number of spaces a tab character should expand to in the final text.</summary>
    public uint TabWidth { get; set; } = 4;

    private MeasureResult? _measureResult;

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize()
    {
        LayoutText(BlockSize.From(int.MaxValue, int.MaxValue));

        return _measureResult?.Size() ?? UnboundedBlockSize.Zero;
    }

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        LayoutText(maxSize);

        return _measureResult?.Size() ?? BlockSize.Zero;
    }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        var size = buffer.BlockSize();
        LayoutText(size);

        _measureResult?.RenderLines(buffer);
    }

    private const char TAB = '\t';
    private const char NEWLINE = '\n';
    private const char SPACE = ' ';
    private const char HYPHEN = '-';
    private const char ELLIPSIS = '…';

    /// <summary>Lays out the text in this text block in accordance with the given size restrictions.</summary>
    private void LayoutText(BlockSize maxSize)
    {
        if (Text is null) return;
        if (_measureResult?.Size() == maxSize) return;

        EnsureMeasureResult();

        var expandedText = PreprocessText(Text, (int)TabWidth);
        var expandedTextSpan = expandedText.Span;

        var currentRowStart = 0;      // index relative to start of string
        var endOfLastWordInRow = 0;   // index relative to start of current line
        var startOfLastWordInRow = 0; // index relative to start of current line
        var verticalOverflow = false;
        for (var i = 0; i < expandedTextSpan.Length; i++)
        {
            var positionInRow = i - currentRowStart;
            var currentChar = expandedTextSpan[i];

            // special character handling
            if (currentChar == NEWLINE)
            {
                if (_measureResult.LineCount + 1 >= maxSize.Height)
                {
                    verticalOverflow = true;
                    break;
                }
                _measureResult.AddLine(expandedText.Slice(currentRowStart, positionInRow));

                currentRowStart = i + 1;
                endOfLastWordInRow = 0;
                startOfLastWordInRow = 0;
                continue;
            }

            // compute word boundaries
            var currentBreakability = IsBreakable(currentChar);
            var prevBreakability = i > 0 ? IsBreakable(expandedTextSpan[i - 1]) : Breakability.NoBreak;
            if (currentBreakability == Breakability.BreakAndDrop && prevBreakability != Breakability.BreakAndDrop)
            {
                endOfLastWordInRow = positionInRow;
            }
            else if (currentBreakability != Breakability.BreakAndDrop && prevBreakability == Breakability.BreakAndDrop)
            {
                startOfLastWordInRow = positionInRow;
            }
            else if (currentBreakability == Breakability.NoBreak && prevBreakability == Breakability.BreakAndKeep)
            {
                endOfLastWordInRow = positionInRow;
                startOfLastWordInRow = positionInRow;
            }

            // Wrap when overflowing current line
            var overflowingLine = positionInRow >= maxSize.Width;
            var isLastChar = i + 1 >= expandedTextSpan.Length;

            if (overflowingLine && _measureResult.LineCount + 1 >= maxSize.Height)
            {
                // we're overflowing and this is the last line that will fit in the buffer
                if (endOfLastWordInRow == 0)
                {
                    // if this row consists of just a single word, display as much of the word as we can
                    endOfLastWordInRow = positionInRow - 1;
                }

                verticalOverflow = true;
                break;
            }

            if (overflowingLine && currentBreakability != Breakability.BreakAndDrop)
            {
                positionInRow = 0;
                int wrapPoint;
                if (startOfLastWordInRow == 0)
                {
                    // If there is only one word in this row, wrap it mid-word at the current character
                    wrapPoint = i - currentRowStart;
                }
                else
                {
                    // If there is more than one word in this row, wrap at the end of the last full word
                    wrapPoint = endOfLastWordInRow;
                    i = currentRowStart + startOfLastWordInRow;
                }

                _measureResult.AddLine(expandedText.Slice(currentRowStart, wrapPoint));

                currentRowStart = i;
                endOfLastWordInRow = 0;
                startOfLastWordInRow = 0;
            }

            if (isLastChar && currentBreakability == Breakability.NoBreak)
            {
                endOfLastWordInRow = positionInRow + 1;
            }
        }

        // add the last line, with an ellipsis if we overflowed
        if (verticalOverflow)
        {
            var lastLine = new char[Math.Min(maxSize.Width, endOfLastWordInRow + 1)];
            expandedText.Slice(currentRowStart, endOfLastWordInRow).CopyTo(lastLine);
            lastLine[^1] = ELLIPSIS;
            _measureResult.AddLine(lastLine);
        }
        else
        {
            _measureResult.AddLine(expandedText.Slice(currentRowStart, endOfLastWordInRow));
        }
    }

    /// <summary>Ensures that measure result has been initialized.</summary>
    [MemberNotNull(nameof(_measureResult))]
    private void EnsureMeasureResult()
    {
        if (_measureResult is not null)
        {
            _measureResult.ClearLines();
        }
        else
        {
            _measureResult = new MeasureResult();
        }
    }

    /// <summary>
    /// Preprocesses text prior to layout and rendering.
    /// Removes non-printable characters and expands tab characters in the given text into <paramref name="tabWidth"/>
    /// spaces.
    /// </summary>
    public static ReadOnlyMemory<char> PreprocessText(string text, int tabWidth)
    {
        var expandedLen = ExpandedStringLength(text, tabWidth);

        if (expandedLen == text.Length)
        {
            return text.AsMemory();
        }

        var textSpan = text.AsSpan();
        var newStr = new char[expandedLen];
        var newStrSpan = newStr.AsSpan();

        Span<char> tabStr = stackalloc char[tabWidth];
        tabStr.Fill(SPACE);

        var copyStart = 0;  // read start index from source span
        var writeIndex = 0; // write start index in destination span

        void copySpanToStr(int copyEnd, ReadOnlySpan<char> source, Span<char> dest)
        {
            if (copyEnd - copyStart > 0)
            {
                var textToWrite = source[copyStart..copyEnd];
                textToWrite.CopyTo(dest[writeIndex..]);
                writeIndex += textToWrite.Length;
            }
            copyStart = copyEnd + 1;
        }

        for (int i = 0; i < textSpan.Length; i++)
        {
            var currentChar = textSpan[i];
            if (currentChar == TAB)
            {
                copySpanToStr(i, textSpan, newStrSpan);

                if (i + 1 >= textSpan.Length) continue;

                tabStr.CopyTo(newStrSpan[writeIndex..]);
                writeIndex += tabStr.Length;
            }
            else if (IsSkippable(currentChar))
            {
                copySpanToStr(i, textSpan, newStrSpan);
            }
        }

        textSpan[copyStart..].CopyTo(newStrSpan[writeIndex..]);

        return newStr.AsMemory();
    }

    /// <summary>Gets the length that a given line will occupy in the final rendered buffer.</summary>
    private static int ExpandedStringLength(string line, int tabWidth)
    {
        var length = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == TAB)
            {
                if (i + 1 >= line.Length)
                {
                    break;
                }

                length += tabWidth;
            }
            else if (IsSkippable(line[i]))
            {
                // do nothing
            }
            else
            {
                length++;
            }
        }

        return length;
    }

    /// <summary>Whether this character should be skipped when outputting text to the buffer.</summary>
    private static bool IsSkippable(char c) =>
        char.GetUnicodeCategory(c) is UnicodeCategory.Control
        && c is not NEWLINE or TAB;

    /// <summary>Determines whether it is legal to break on the given character.</summary>
    private static Breakability IsBreakable(char c) => c switch
    {
        SPACE  => Breakability.BreakAndDrop,
        HYPHEN => Breakability.BreakAndKeep,
        _      => Breakability.NoBreak
    };

    /// <summary>Whether it is safe to break a line on a character.</summary>
    private enum Breakability { NoBreak, BreakAndDrop, BreakAndKeep }

    /// <summary>Stores computed text layout</summary>
    private class MeasureResult
    {
        private readonly List<ReadOnlyMemory<char>> _rawLines = new();

        public int LineCount => _rawLines.Count;

        public void ClearLines()
        {
            _rawLines.Clear();
        }

        public void AddLine(ReadOnlyMemory<char> line) => _rawLines.Add(line);

        public void RenderLines(Span2D<char> buffer)
        {
            for (var i = 0; i < _rawLines.Count; i++)
            {
                _rawLines[i].Span.CopyTo(buffer.GetRowSpan(i));
            }
        }

        public BlockSize Size() => BlockSize.From(_rawLines.Max(s => s.Length), _rawLines.Count);
    }
}
