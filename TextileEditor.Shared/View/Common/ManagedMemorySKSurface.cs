using SkiaSharp;
using System.Runtime.InteropServices;

namespace TextileEditor.Shared.View.Common;

internal class ManagedMemorySKSurface()
{
    public SKImageInfo SKImageInfo { get; private set; } = new SKImageInfo(0, 0);
    private byte[]? pixels;
    private GCHandle pixelsHandle;

    public SKSurface CreateSurface(out SKImageInfo info)
    {
        CreateBitmap(SKImageInfo);
        info = SKImageInfo;
        return (SKSurface?)SKSurface.Create(SKImageInfo, pixelsHandle.AddrOfPinnedObject(), SKImageInfo.RowBytes) ?? throw new InvalidOperationException("configuration is not supported.");
    }
    public SKSurface CreateSurface(SKImageInfo sourceInfo)
    {
        CreateBitmap(sourceInfo);
        return (SKSurface?)SKSurface.Create(sourceInfo, pixelsHandle.AddrOfPinnedObject(), sourceInfo.RowBytes) ?? throw new InvalidOperationException("configuration is not supported.");
    }
    public void ChangeImageInfo(SKImageInfo info) => CreateBitmap(info);
    private void CreateBitmap(SKImageInfo info)
    {
        if (pixels == null || !SKImageInfo.Equals(info))
        {
            FreeBitmap();

            pixels = new byte[info.BytesSize];
            pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            SKImageInfo = info;
        }
    }

    private void FreeBitmap()
    {
        if (pixels != null)
        {
            pixelsHandle.Free();
            pixels = null;
        }
    }

    public void Dispose() => FreeBitmap();
}