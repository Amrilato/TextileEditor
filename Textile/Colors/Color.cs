namespace Textile.Colors;

public struct Color
{
    private uint Value;

    public byte Alpha
    {
        readonly get => (byte)((Value & 0xFF000000) >> 24);
        set => Value = Value & 0x00FFFFFF | (uint)value << 24;
    }

    public byte Red
    {
        readonly get => (byte)((Value & 0x00FF0000) >> 16);
        set => Value = Value & 0xFF00FFFF | (uint)value << 16;
    }

    public byte Green
    {
        readonly get => (byte)((Value & 0x0000FF00) >> 8);
        set => Value = Value & 0xFFFF00FF | (uint)value << 8;
    }

    public byte Blue
    {
        readonly get => (byte)(Value & 0x000000FF);
        set => Value = Value & 0xFFFFFF00 | value;
    }

    public Color(byte alpha, byte red, byte green, byte blue)
    {
        Value = (uint)(alpha << 24 | red << 16 | green << 8 | blue);
    }

    public Color(uint value)
    {
        Value = value;
    }

    public static bool operator ==(Color lhs, Color rhs)
    {
        return lhs.Value == rhs.Value;
    }

    public static bool operator !=(Color lhs, Color rhs)
    {
        return lhs.Value != rhs.Value;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Color color && Value == color.Value;
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override readonly string ToString()
    {
        return $"ARGB({Alpha}, {Red}, {Green}, {Blue})";
    }
}
