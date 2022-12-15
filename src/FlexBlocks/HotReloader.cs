using System.Reflection.Metadata;
using FlexBlocks;

[assembly: MetadataUpdateHandler(typeof(HotReloader))]

namespace FlexBlocks;

internal static class HotReloader
{
    public static event Action? OnHotReloaded;

    public static void ClearCache(Type[]? types) { }

    public static void UpdateApplication(Type[]? types)
    {
        OnHotReloaded?.Invoke();
    }
}
