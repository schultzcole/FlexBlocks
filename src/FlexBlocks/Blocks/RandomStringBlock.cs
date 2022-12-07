using CommunityToolkit.HighPerformance;

namespace FlexBlocks.Blocks;

public class RandomStringBlock : UiBlock
{
    private const string VALID_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public int? Width { get; set; }
    public int? Height { get; set; }

    public override BlockSize CalcDesiredSize(BlockSize maxSize) =>
        (Width, Height) switch
        {
            (null, null)            => maxSize,
            ({ } width, null)       => maxSize.ConstrainWidth(width),
            (null, { } height)      => maxSize.ConstrainHeight(height),
            ({ } width, { } height) => maxSize.Constrain(width, height)
        };

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
