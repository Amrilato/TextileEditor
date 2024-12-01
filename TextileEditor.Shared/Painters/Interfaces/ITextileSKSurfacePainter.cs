using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;

public interface ITextileSKSurfacePainter<TIndex, TValue>
{
    void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IReadOnlyTextile<TIndex, TValue> textileData, GridSize size);
    event Action RequestSurface;
}
