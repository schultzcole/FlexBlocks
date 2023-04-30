using CommunityToolkit.HighPerformance;
using FlexBlocks.BlockProperties;
using JetBrains.Annotations;

namespace FlexBlocks.Blocks.Debug;

/// <summary>Fills a block with random characters.</summary>
[PublicAPI]
public sealed class RandomStringBlock : UiBlock
{
    private const string VALID_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public override BlockBounds GetBounds() => BlockBounds.Unbounded;

    public override BlockSize CalcSize(BlockSize maxSize) => maxSize;

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
