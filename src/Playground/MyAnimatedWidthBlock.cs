using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using FlexBlocks;
using FlexBlocks.BlockProperties;
using FlexBlocks.Blocks;

namespace Playground;

/// A custom implementation of BoundedBlock that re-renders itself periodically
/// Disposing this Block stops the re-render loop.
public class MyAnimatedWidthBlock : FixedSizeBlock, IDisposable
{
    private Task? _rerenderTask;
    private readonly CancellationTokenSource _cts = new();

    public int MinWidth { get; set; }
    public int MaxWidth { get; set; }

    public int Interval { get; set; } = 100;

    public override void Render(Span2D<char> buffer)
    {
        base.Render(buffer);

        RegisterRerenderTask();
    }

    private int _expandDir = 1;

    private void RegisterRerenderTask()
    {
        if (_rerenderTask is null && !_cts.Token.IsCancellationRequested)
        {
            _rerenderTask = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(Interval, _cts.Token);

                        if (!CanRequestRerender) continue;

                        Width = Width.Clamp(MinWidth, MaxWidth);

                        if (Width.Value >= MaxWidth) _expandDir = -1;
                        if (Width.Value <= MinWidth) _expandDir = 1;

                        Width += _expandDir;

                        RequestRerender(RerenderMode.DesiredSizeChanged);
                    }
                }
            );
        }
    }

    public void Dispose() { _cts.Dispose(); }
}

internal static class BlockLengthExtensions
{
    public static BlockLength Clamp(this BlockLength length, int min, int max)
    {
        if (length.IsUnbounded) return max;
        if (length > max) return max;
        if (length < min) return min;

        return length;
    }
}
