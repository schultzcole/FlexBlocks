﻿using CommunityToolkit.HighPerformance;
using FlexBlocks.Blocks;
using JetBrains.Annotations;

namespace FlexBlocks.Renderables.Debug;

[PublicAPI]
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

        string str;
        if (delta is { } dt) {
            str = ShowFps
                ? $"{dt.TotalMilliseconds:F2}ms ({(int)(1 / dt.TotalSeconds)})"
                : $"{dt.TotalMilliseconds:F2}ms";
        } else {
            str = ShowFps ? "0ms (0)" : "0ms";
        }

        if (str.Length > buffer.Width) return;

        var strBuffer = buffer.GetRowSpan(0)[(buffer.Width - str.Length)..];
        str.CopyTo(strBuffer);
    }
}
