using SkiaSharp;
using Textile.Data;
using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.Painters;

public interface ITextileDataPreviewSKSurfacePainter : ISKSurfacePainter, IAsyncDisposable
{
    SKSizeI PixelSize { get; set; }
    int RepeatHorizontal { get; set; }
    int RepeatVertical { get; set; }
    TextileStructure TextileStructure { get; set; }
    static ITextileDataPreviewSKSurfacePainter Create(ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext, SKSizeI pixelSize, int repeatHorizontal, int repeatVertical, TextileStructure structure)
    {
        TextileDataPreviewSKSurfacePainter result = new(structure, pixelSize, repeatHorizontal, repeatVertical, concurrencyBackgroundWorkContext, new(pixelSize.Width * structure.Textile.Width, pixelSize.Height * structure.Textile.Height));
        var _ = result.PrerenderAsync();
        return result;
    }
}
