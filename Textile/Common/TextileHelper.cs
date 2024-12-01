using Textile.Interfaces;

namespace Textile.Common;

internal static class TextileHelper
{
    public const int BitLength = 32;
    public const int BitSize = 31;
    public static uint[][] Create(int arrayLength, int bitLength)
    {
        var value = new uint[arrayLength][];
        for (int i = 0; i < value.Length; i++)
            value[i] = new uint[bitLength.GetArraySize()];
        return value;
    }
    //private static bool CheckRange(this IReadOnlyTextile textile, TextileIndex index) => CheckRange(textile.Width, textile.Height, index);
    private static bool CheckRange(int width, int height, TextileIndex index) => (uint)width > (uint)index.X && (uint)height > (uint)index.Y;
    public static void ThrowIfIndexWasOutOfBound(this IReadOnlyTextile<TextileIndex, bool> textile, TextileIndex index) => ThrowIfIndexWasOutOfBound(textile.Width, textile.Height, index);
    public static void ThrowIfIndexWasOutOfBound(int width, int height, TextileIndex index)
    {
        if (!CheckRange(width, height, index))
            throw new ArgumentOutOfRangeException(nameof(index), $"The index ({index.X}, {index.Y}) is outside the valid range for a textile with dimensions ({width}, {height}).");
    }
}
