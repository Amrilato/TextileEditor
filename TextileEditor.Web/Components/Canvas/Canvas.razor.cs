using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;
using SkiaSharp;
using TextileEditor.Shared.View.Common;
using R3;

namespace TextileEditor.Web.Components;

public partial class Canvas : IDisposable
{
    private string SizedStyle => $"width: {Size.Width}px; height: {Size.Height}px; {Style ?? ""}";

    [Parameter]
    public string? Style { get; set; }

    private IDisposable? PreviousPainter { get; set; }
    [Parameter, EditorRequired]
    public IPainter Painter { get; set; } = default!;

    [Parameter]
    public ICanvasEventHandler? EventHandler { get; set; }

    private SKSizeI Size => Painter.CanvasSize;

    private SKCanvasView? SKCanvasView;
    private void OnPaintSurface(SKPaintSurfaceEventArgs eventArgs) => Painter?.OnPaintSurface(eventArgs.Surface, eventArgs.Info, eventArgs.RawInfo, CancellationToken.None);

    protected override void OnAfterRender(bool firstRender) => SKCanvasView?.Invalidate();

    protected override void OnParametersSet()
    {
        if (PreviousPainter is not null)
            PreviousPainter.Dispose();
        if (Painter is not null)
            PreviousPainter = Painter.RenderProgress.SubscribeAwait(async (renderProgress, token) =>
            {
                if (renderProgress.Status == RenderProgressStates.Ready)
                    await InvokeAsync(StateHasChanged);
            });
    }

    private void OnClick(MouseEventArgs eventArgs) => EventHandler?.OnClick(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerMove(PointerEventArgs eventArgs) => EventHandler?.OnPointerMove(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerEnter(PointerEventArgs eventArgs) => EventHandler?.OnPointerEnter(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerLeave(PointerEventArgs eventArgs) => EventHandler?.OnPointerLeave(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerDown(PointerEventArgs eventArgs) => EventHandler?.OnPointerDown(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));
    private void OnPointerUp(PointerEventArgs eventArgs) => EventHandler?.OnPointerUp(new((float)eventArgs.OffsetX, (float)eventArgs.OffsetY));

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        PreviousPainter?.Dispose();
    }
}
