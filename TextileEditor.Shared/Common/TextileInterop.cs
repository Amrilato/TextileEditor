using SkiaSharp;
using System.Buffers;
using System.Runtime.CompilerServices;
using Textile.Colors;
using Textile.Common;

namespace TextileEditor.Shared.Shared.Common;

public static class TextileInterop
{
    public static GridIndex AsGridIndex(this TextileIndex index) => Unsafe.As<TextileIndex, GridIndex>(ref index);
    public static TextileIndex AsTextileIndex(this GridIndex index) => Unsafe.As<GridIndex, TextileIndex>(ref index);
    public static TextileRange ToTextileRange(this GridRange range) => new(range.Top, range.Left, range.Bottom - range.Top + 1, range.Right - range.Left + 1);
    public static SKColor AsSKColor(this Color color) => Unsafe.As<Color, SKColor>(ref color);
    public static Color AsColor(this SKColor color) => Unsafe.As<SKColor, Color>(ref color);
    public static RentArray<T> ToRentArray<T>(this ReadOnlySpan<T> span)
    {
        RentArray<T> value = new(span.Length, ArrayPool<T>.Shared);
        span.CopyTo(value.Values.Span);
        return value;
    }
    public static RentArray<T> ToRentArray<T>(this List<T> list)
    {
        RentArray<T> value = new(list.Count, ArrayPool<T>.Shared);
        list.CopyTo(value.Values.Span);
        return value;
    }
}
