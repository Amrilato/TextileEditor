using SkiaSharp;
using System.Runtime.InteropServices;

namespace TextileEditor.Shared.Painters;

internal abstract class SKSurfacePainter(SKSizeI initialBuffer) : ISKSurfacePainter, IAsyncDisposable
{
    private SKSizeI pixelSize = initialBuffer;
    private byte[]? pixels;
    private GCHandle pixelsHandle;
    private bool disposedValue;
    protected bool DisposedValue => disposedValue;
    public event Action? RequestSurface;
    protected void InvokeRequestSurface() => RequestSurface?.Invoke();
    public virtual void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        using var cache = CreateSurface(out var _);
        surface.Canvas.DrawSurface(cache, new(0, 0));
    }

    protected SKSurface CreateSurface(out SKImageInfo info)
    {
        info = CreateBitmap();
        return (SKSurface?)SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes) ?? throw new InvalidOperationException("configuration is not supported.");
    }

    protected virtual void OnCreateBitmap() { }

    protected virtual bool SizeChange(SKSizeI newSize)
    {
        if (pixelSize.Width == newSize.Width && pixelSize.Height == newSize.Height)
            return false;
        pixelSize = newSize;
        FreeBitmap();
        return true;
    }

    private SKImageInfo CreateBitmap()
    {
        var size = CreateSize();
        var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);

        if (pixels == null || pixelSize.Width != info.Width || pixelSize.Height != info.Height)
        {
            FreeBitmap();

            pixels = new byte[info.BytesSize];
            pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            pixelSize = info.Size;
            OnCreateBitmap();
        }

        return info;
    }

    private SKSizeI CreateSize()
    {
        var w = pixelSize.Width;
        var h = pixelSize.Height;

        if (!IsPositive(w) || !IsPositive(h))
            return SKSizeI.Empty;

        return new SKSizeI(w, h);

        static bool IsPositive(int value) => value > 0;
    }

    private void FreeBitmap()
    {
        if (pixels != null)
        {
            pixelsHandle.Free();
            pixels = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            FreeBitmap();
            disposedValue = true;
        }
    }

    protected virtual ValueTask DisposeAsync(bool disposing) => ValueTask.CompletedTask;

    ~SKSurfacePainter()
    {
        Dispose(disposing: false);
    }

    public async ValueTask DisposeAsync()
    {
        Dispose(disposing: true);
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
}
