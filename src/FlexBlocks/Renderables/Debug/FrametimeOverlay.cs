using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;

namespace FlexBlocks.Renderables.Debug;

public sealed class FrametimeOverlay : IRenderable
{
    public bool ShowFps { get; set; }

    private long? _prevTimeTicks;

    /// <inheritdoc />
    public void Render(Span2D<char> buffer)
    {
        var currentTimeTicks = DateTime.Now.Ticks;
        TimeSpan? delta = _prevTimeTicks is not null ? TimeSpan.FromTicks(currentTimeTicks - _prevTimeTicks.Value) : null;
        _prevTimeTicks = currentTimeTicks;

        if (delta is not { } dt) return;

        var str = ShowFps
            ? $"{dt.TotalMilliseconds:F2}ms ({(int)(1 / dt.TotalSeconds)})"
            : $"{dt.TotalMilliseconds:F2}ms";

        if (str.Length > buffer.Width) return;

        var strBuffer = buffer.GetRowSpan(0)[(buffer.Width - str.Length)..];
        str.CopyTo(strBuffer);
    }
}
