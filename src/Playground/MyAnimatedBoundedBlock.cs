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
public class MyAnimatedBoundedBlock : BoundedBlock, IDisposable
{
    private Task? _rerenderTask;
    private readonly CancellationTokenSource _cts = new();

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
                        await Task.Delay(10, _cts.Token);

                        if (!CanRequestRerender) continue;

                        if (Width != BlockLength.Unbounded)
                        {
                            _expandDir = Width.Value switch
                            {
                                >= 80 => -1,
                                <= 15 => 1,
                                _     => _expandDir
                            };

                            Width += _expandDir;
                        }

                        RequestRerender(RerenderMode.DesiredSizeChanged);
                    }
                }
            );
        }
    }

    public void Dispose() { _cts.Dispose(); }
}
