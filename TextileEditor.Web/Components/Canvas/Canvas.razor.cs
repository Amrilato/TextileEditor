using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;
using SkiaSharp;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.EventHandlers;

namespace TextileEditor.Web.Components;

public partial class Canvas : IDisposable
{
    private string SizedStyle => $"width: {Size.Width}px; height: {Size.Height}px; {Style ?? ""}";

    [Parameter]
    public string? Style { get; set; }

    private ISKSurfacePainter? PreviousSKSurfacePainter { get; set; }
    [Parameter, EditorRequired]
    public ISKSurfacePainter? SKSurfacePainter { get; set; }

    [Parameter]
    public ICanvasEventHandler? EventHandlers { get; set; }

    [Parameter]
    public SKSizeI Size { get; set; }

    private SKCanvasView? SKCanvasView;
    private void OnPaintSurface(SKPaintSurfaceEventArgs eventArgs) => SKSurfacePainter?.OnPaintSurface(eventArgs.Surface, eventArgs.Info, eventArgs.RawInfo);

    protected override void OnAfterRender(bool firstRender) => Invalidate();
    public void Invalidate() => SKCanvasView?.Invalidate();
    private void InvokeStateHasChanged() => InvokeAsync(StateHasChanged);

    protected override void OnParametersSet()
    {
        if (PreviousSKSurfacePainter != SKSurfacePainter)
        {
            if (PreviousSKSurfacePainter is not null)
                PreviousSKSurfacePainter.RequestSurface -= InvokeStateHasChanged;
            if (SKSurfacePainter is not null)
                SKSurfacePainter.RequestSurface += InvokeStateHasChanged;

            PreviousSKSurfacePainter = SKSurfacePainter;
        }
    }

    private void OnClick(MouseEventArgs eventArgs) => EventHandlers?.OnClick(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerMove(PointerEventArgs eventArgs) => EventHandlers?.OnPointerMove(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerEnter(PointerEventArgs eventArgs) => EventHandlers?.OnPointerEnter(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerLeave(PointerEventArgs eventArgs) => EventHandlers?.OnPointerLeave(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerDown(PointerEventArgs eventArgs) => EventHandlers?.OnPointerDown(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerUp(PointerEventArgs eventArgs) => EventHandlers?.OnPointerUp(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (SKSurfacePainter is not null)
            SKSurfacePainter.RequestSurface -= InvokeStateHasChanged;
    }
}
