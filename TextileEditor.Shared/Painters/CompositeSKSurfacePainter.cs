using SkiaSharp;

namespace TextileEditor.Shared.Painters;

public class CompositeSKSurfacePainter : ISKSurfacePainter, ICollection<ISKSurfacePainter>, IDisposable
{
    public event Action? RequestSurface;
    private void InvokeRequestSurface() => RequestSurface?.Invoke();
    private readonly List<ISKSurfacePainter> SKSurfacePainters = [];

    public int Count => SKSurfacePainters.Count;

    bool ICollection<ISKSurfacePainter>.IsReadOnly => false;

    public void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        foreach (var painter in SKSurfacePainters)
            painter.OnPaintSurface(surface, info, rawInfo);
    }

    public void Add(ISKSurfacePainter painter)
    {
        if (SKSurfacePainters.Contains(painter))
            return;
        painter.RequestSurface += InvokeRequestSurface;
        SKSurfacePainters.Add(painter);
        InvokeRequestSurface();
    }

    public bool Remove(ISKSurfacePainter painter)
    {
        bool removed = SKSurfacePainters.Remove(painter);
        if (removed)
        {
            painter.RequestSurface -= InvokeRequestSurface;
            InvokeRequestSurface();
        }
        return removed;
    }

    public void Clear() => SKSurfacePainters.Clear();

    public bool Contains(ISKSurfacePainter item) => SKSurfacePainters.Contains(item);

    public void CopyTo(ISKSurfacePainter[] array, int arrayIndex) => SKSurfacePainters.CopyTo(array, arrayIndex);

    public IEnumerator<ISKSurfacePainter> GetEnumerator() => SKSurfacePainters.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => SKSurfacePainters.GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var painter in SKSurfacePainters)
            painter.RequestSurface -= InvokeRequestSurface;
        SKSurfacePainters.Clear();
    }
}