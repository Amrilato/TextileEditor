using SkiaSharp;

namespace TextileEditor.Shared.Painters;

public interface ITextilePreviewContext : ITextilePainterContext
{
    ISKSurfacePainter PreviewPainter { get; }
    SKSizeI CanvasSize { get; }
    SKSizeI PixelSize { get; set; }
    int RepeatHorizontal { get; set; }
    int RepeatVertical { get; set; }
    void Rerender();
}
