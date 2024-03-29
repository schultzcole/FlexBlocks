﻿using FlexBlocks.Blocks;

namespace FlexBlocks;

/// <summary>
/// Thrown when attempting to use a UiBlock's <see cref="UiBlock.Container"/> before a render has been initiated by the
/// container.
/// </summary>
public class UnattachedUiBlockException : Exception
{
    public UnattachedUiBlockException(string message) : base(message) { }
}
