using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;

namespace FlexBlocks.Blocks.Debug;

/// <summary>Fills a specified area (or the full extent of its parent) with random characters</summary>
public class RandomStringBlock : UiBlock
{
    private const string VALID_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public DesiredBlockSize DesiredSize { get; set; }

    public BlockLength Width
    {
        get => DesiredSize.Width;
        set => DesiredSize = DesiredSize with { Width = value };
    }
    public BlockLength Height
    {
        get => DesiredSize.Height;
        set => DesiredSize = DesiredSize with { Height = value };
    }

    public override DesiredBlockSize CalcDesiredSize(BlockSize _) => DesiredSize;

    public override void Render(Span2D<char> buffer)
    {
        for (int col = 0; col < buffer.Width; col++)
        {
            for (int row = 0; row < buffer.Height; row++)
            {
                buffer[row, col] = VALID_CHARS[Random.Shared.Next(VALID_CHARS.Length)];
            }
        }
    }
}
