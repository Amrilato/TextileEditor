using SkiaSharp;

namespace TextileEditor.Shared.Painters;

public interface ISKSurfacePainter
{
    void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo);
    event Action RequestSurface;
}
