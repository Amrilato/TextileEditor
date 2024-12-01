using SkiaSharp;
using Textile.Data;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;

internal class TextileDataPreviewSKSurfacePainter(TextileStructure textileStructure, SKSizeI pixelSize, int repeatHorizontal, int repeatVertical, ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext, SKSizeI sKSizeI) : AsyncSKSurfaceRenderer(concurrencyBackgroundWorkContext, sKSizeI), ITextileDataPreviewSKSurfacePainter
{
    static TextileDataPreviewSKSurfacePainter() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    protected readonly static SKPaint SKPaint;

    public TextileStructure TextileStructure
    {
        get => textileStructure;
        set => textileStructure = value;
    }


    public int RepeatVertical
    {
        get => repeatVertical;
        set => repeatVertical = value;
    }
    public int RepeatHorizontal
    {
        get => repeatHorizontal;
        set => repeatHorizontal = value;
    }
    public SKSizeI PixelSize
    {
        get => pixelSize;
        set => pixelSize = value;
    }

    public override void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        if (!AlreadyRender)
            return;

        var canvas = surface.Canvas;

        using var repeatImage = CreateSurface(out _);
        using var snapshot = repeatImage.Snapshot();
        using var bitmap = SKBitmap.FromImage(snapshot);
        using var shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(canvas.LocalClipBounds, paint);
    }

    internal Task PrerenderAsync()
    {
        var work = CreateWork(0);
        return Post(() => PrerenderAsync(work.CancellationToken), work);
    }
    private Task PrerenderAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var structure = TextileStructure;
        var textile = TextileStructure.Textile;
        var SKPaint = TextileDataPreviewSKSurfacePainter.SKPaint;
        SizeChange(new(PixelSize.Width * textile.Width, PixelSize.Height * textile.Height));
        using var surface = CreateSurface(out _);

        for (int y = 0; y < textile.Height; y++)
        {
            for (int x = 0; x < textile.Width; x++)
            {
                token.ThrowIfCancellationRequested();
                SKPoint sKPoint = new(x * PixelSize.Width, y * PixelSize.Height);
                SKRect sKRect = new(sKPoint.X, sKPoint.Y, sKPoint.X + PixelSize.Width, sKPoint.Y + PixelSize.Height);
                SKPaint.Color = (textile[new(x, y)] ? structure.HeddleColor[x] : structure.PedalColor[y]).AsSKColor();
                surface.Canvas.DrawRect(sKRect, SKPaint);
            }
        }
        return Task.CompletedTask;
    }
    //private void Prerender()
    //{
    //    var structure = TextileStructure;
    //    var textile = TextileStructure.Textile;
    //    var SKPaint = TextileDataPreviewSKSurfacePainter.SKPaint;
    //    SizeChange(new(PixelSize.Width * textile.Width, PixelSize.Height * textile.Height));
    //    using var surface = CreateSurface(out _);

    //    for (int y = 0; y < textile.Height; y++)
    //    {
    //        for (int x = 0; x < textile.Width; x++)
    //        {
    //            SKPoint sKPoint = new(x * PixelSize.Width, y * PixelSize.Height);
    //            SKRect sKRect = new(sKPoint.X, sKPoint.Y, sKPoint.X + PixelSize.Width, sKPoint.Y + PixelSize.Height);
    //            SKPaint.Color = (textile[new(x, y)] ? structure.HeddleColor[x] : structure.PedalColor[y]).AsSKColor();
    //            surface.Canvas.DrawRect(sKRect, SKPaint);
    //        }
    //    }
    //}
}
