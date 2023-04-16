using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;

namespace Benchmarks.GridBlockBenchmarks;

[BenchmarkCategory(nameof(UiBlock), nameof(GridBlock), nameof(UiBlock.CalcSize))]
public class CalcSize
{
    public static IEnumerable<(int rows, int cols)> Sizes => new[]
    {
        (2, 2),
        (10, 10),
        (32, 32),
    };

    [ParamsSource(nameof(Sizes))]
    public (int rows, int cols) Size { get; set; }

    private GridBlock _nullCellsBlock = null!;
    private GridBlock _boundedCellsBlock = null!;

    private const int BLOCK_SIZE = 5;

    [GlobalSetup]
    public void Setup()
    {
        _nullCellsBlock = new GridBlock { Contents = new UiBlock[Size.rows, Size.cols] };

        _boundedCellsBlock = new GridBlock { Contents = new UiBlock[Size.rows, Size.cols] };
        for (int row = 0; row < Size.rows; row++)
        for (int col = 0; col < Size.cols; col++)
        {
            _boundedCellsBlock.Contents[row, col] = new FixedSizeBlock { Width = BLOCK_SIZE, Height = BLOCK_SIZE };
        }
    }

    [Benchmark]
    public BlockSize NullCells() => _nullCellsBlock.CalcSize(BlockSize.From(int.MaxValue, int.MaxValue));

    [Benchmark]
    [Arguments(.25)]
    [Arguments(.75)]
    [Arguments(1)]
    public BlockSize BoundedSizeCells(double frac)
    {
        var maxWidth = (int)(BLOCK_SIZE * Size.cols * frac);
        return _boundedCellsBlock.CalcSize(BlockSize.From(maxWidth, int.MaxValue));
    }
}
