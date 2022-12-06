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
            (null, null) => maxSize,
            ({}, null)   => maxSize.ConstrainWidth(Width.Value),
            (null, {})   => maxSize.ConstrainHeight(Height.Value),
            _            => maxSize.Constrain(Width.Value, Height.Value)
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
