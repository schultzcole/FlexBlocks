using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks.Debug;

namespace Playground;

/// A custom implementation of RandomStringBlock that re-renders itself every 100 ms
/// Disposing this Block stops the re-render loop.
public class MyCustomRandomStringBlock : RandomStringBlock, IDisposable
{
    private Task? _rerenderTask;
    private readonly CancellationTokenSource _cts = new();

    public override void Render(Span2D<char> buffer)
    {
        base.Render(buffer);

        RegisterRerenderTask();
    }

    private void RegisterRerenderTask()
    {
        if (_rerenderTask is null && !_cts.Token.IsCancellationRequested)
        {
            _rerenderTask = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(50, _cts.Token);

                        if (Container is null) continue;

                        RequestRerender();
                    }
                }
            );
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}
