using R3;
using SkiaSharp;

namespace TextileEditor.Shared.View.Common;

public interface IPainter
{
    public Task OnPaintSurfaceAsync(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, CancellationToken token);
    public ReadOnlyReactiveProperty<RenderProgress> RenderProgress { get; }
    public SKSizeI CanvasSize { get; }
}
