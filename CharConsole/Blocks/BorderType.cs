namespace CharConsole.Blocks;

public enum BorderType { None, Block, Square, Rounded, Debug }

internal static class BorderTypeExtensions
{
    public static char TopLeft(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u250c',
            BorderType.Rounded => '\u256d',
            BorderType.Debug   => '/',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char Top(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2500',
            BorderType.Rounded => '\u2500',
            BorderType.Debug   => 'T',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char TopRight(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2510',
            BorderType.Rounded => '\u256e',
            BorderType.Debug   => '\\',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char Right(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2502',
            BorderType.Rounded => '\u2502',
            BorderType.Debug   => 'R',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char BottomRight(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2518',
            BorderType.Rounded => '\u256f',
            BorderType.Debug   => '/',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char Bottom(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2500',
            BorderType.Rounded => '\u2500',
            BorderType.Debug   => 'B',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char BottomLeft(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2514',
            BorderType.Rounded => '\u2570',
            BorderType.Debug   => '\\',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
    public static char Left(this BorderType borderType)
    {
        return borderType switch
        {
            BorderType.None    => '\0',
            BorderType.Block   => '\u2588',
            BorderType.Square  => '\u2502',
            BorderType.Rounded => '\u2502',
            BorderType.Debug   => 'L',
            _                  => throw new ArgumentOutOfRangeException(nameof(borderType), borderType, null)
        };
    }
}
