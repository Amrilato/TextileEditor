using SkiaSharp;
using System.Runtime.CompilerServices;
using Textile.Colors;
using Textile.Common;
using TextileEditor.Shared.View.TextileEditor;

namespace TextileEditor.Shared.View.Common;

public static class TextileInterop
{
    public static GridIndex AsGridIndex(this TextileIndex index) => Unsafe.As<TextileIndex, GridIndex>(ref index);
    public static TextileIndex AsTextileIndex(this GridIndex index) => Unsafe.As<GridIndex, TextileIndex>(ref index);
    public static TextileRange ToTextileRange(this GridRange range) => new(range.Top, range.Left, range.Bottom - range.Top + 1, range.Right - range.Left + 1);
    public static SKColor AsSKColor(this Color color) => Unsafe.As<Color, SKColor>(ref color);
    public static Color AsColor(this SKColor color) => Unsafe.As<SKColor, Color>(ref color);
}
