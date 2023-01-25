using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks;

/// <summary>
/// Renders text.
/// </summary>
public class TextBlock : UiBlock
{
    private string? _text;

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

    public uint TabWidth { get; set; } = 4;

    private MeasureResult? _measureResult;

    /// <inheritdoc />
    public override UnboundedBlockSize CalcMaxSize() => UnboundedBlockSize.Unbounded;

    /// <inheritdoc />
    public override BlockSize CalcSize(BlockSize maxSize)
    {
        LayoutText(maxSize);

        return _measureResult?.Size() ?? BlockSize.From(0, 0);
    }

    /// <inheritdoc />
    public override void Render(Span2D<char> buffer)
    {
        var size = buffer.BlockSize();
        LayoutText(size);

        _measureResult?.RenderLines(buffer);
    }

    /// <summary>Lays out the text in this text block in accordance with the given size restrictions.</summary>
    private void LayoutText(BlockSize maxSize)
    {
        if (Text is null) return;
        if (_measureResult?.Size() == maxSize) return;

        EnsureMeasureResult();

        var expandedText = ExpandText(Text, (int)TabWidth);
        var expandedTextSpan = expandedText.Span;

        var currentRowStart = 0; // index relative to start of string
        var endOfLastWordInRow = 0; // index relative to start of current line
        var startOfLastWordInRow = 0; // index relative to start of current line
        for (var i = 0; i < expandedTextSpan.Length; i++)
        {
            var positionInRow = i - currentRowStart;
            var currentChar = expandedTextSpan[i];

            if (currentChar == '\n')
            {
                _measureResult.AddLine(expandedText.Slice(currentRowStart, positionInRow));
                currentRowStart = i + 1;
                endOfLastWordInRow = 0;
                startOfLastWordInRow = 0;
                continue;
            }

            var currentBreakability = IsBreakable(currentChar);
            var prevBreakability = i > 0 ? IsBreakable(expandedTextSpan[i - 1]) : Breakability.NoBreak;
            if (currentBreakability == Breakability.Break && prevBreakability != Breakability.Break)
            {
                endOfLastWordInRow = positionInRow;
            }
            else if (currentBreakability != Breakability.Break && prevBreakability == Breakability.Break)
            {
                startOfLastWordInRow = positionInRow;
            }
            else if (currentBreakability == Breakability.NoBreak && prevBreakability == Breakability.BreakAndKeep)
            {
                endOfLastWordInRow = positionInRow;
                startOfLastWordInRow = positionInRow;
            }

            var overflowingLine = positionInRow >= maxSize.Width;
            var isLastChar = i + 1 >= expandedTextSpan.Length;

            if (overflowingLine && currentBreakability != Breakability.Break)
            {
                _measureResult.AddLine(expandedText.Slice(currentRowStart, endOfLastWordInRow));
                i = currentRowStart + startOfLastWordInRow;
                currentRowStart = i;
                endOfLastWordInRow = 0;
                startOfLastWordInRow = 0;

                if (_measureResult.LineCount >= maxSize.Height)
                {
                    break;
                }
            }
            else if (isLastChar && currentBreakability == Breakability.NoBreak)
            {
                endOfLastWordInRow = positionInRow + 1;
            }
        }

        if (endOfLastWordInRow > 0)
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

    /// <summary>Expands tab characters in the given text into <paramref name="tabWidth"/> spaces.</summary>
    /// <param name="text">The text to expand.</param>
    /// <param name="tabWidth">Number of spaces that a tab should be rendered as.</param>
    public static ReadOnlyMemory<char> ExpandText(string text, int tabWidth)
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
        tabStr.Fill(' ');

        var lastTab = 0; // index within the given text just after the last tab character
        var writeIndex = 0; // index within the new string array
        for (int i = 0; i < textSpan.Length; i++)
        {
            if (textSpan[i] != '\t') continue;

            if (i - lastTab > 0)
            {
                var textToWrite = textSpan[lastTab..i];
                textToWrite.CopyTo(newStrSpan[writeIndex..]);
                writeIndex += textToWrite.Length;
            }

            if (i + 1 < textSpan.Length)
            {
                tabStr.CopyTo(newStrSpan[writeIndex..]);
                writeIndex += tabStr.Length;
            }

            lastTab = i + 1;
        }

        textSpan[lastTab..].CopyTo(newStrSpan[writeIndex..]);

        return newStr.AsMemory();
    }

    /// <summary>Gets the length that a given line will occupy in the final rendered buffer.</summary>
    private static int ExpandedStringLength(string line, int tabWidth)
    {
        var length = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\t')
            {
                if (i + 1 >= line.Length)
                {
                    break;
                }

                length += tabWidth;
            }
            else
            {
                length++;
            }
        }

        return length;
    }

    private static Breakability IsBreakable(char c) => c switch
    {
        ' ' => Breakability.Break,
        '-' => Breakability.BreakAndKeep,
        _   => Breakability.NoBreak
    };

    private enum Breakability { NoBreak, Break, BreakAndKeep }

    private class MeasureResult
    {
        private readonly List<ReadOnlyMemory<char>> _rawLines = new();

        public int LineCount => _rawLines.Count;

        public void ClearLines()
        {
            _rawLines.Clear();
        }

        public void AddLine(ReadOnlyMemory<char> line)
        {
            _rawLines.Add(line);
        }

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
