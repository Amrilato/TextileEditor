using SkiaSharp;
using Textile.Data;
using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.Painters;

internal class TextilePreviewContext : TextilePainterContext, ITextilePreviewContext
{
    private readonly TextileDataPreviewSKSurfacePainter previewPainter;

    public TextilePreviewContext(TextileSession textileSession, ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext, IEditorConfigure editorConfigure) : base(textileSession)
    {
        previewPainter = new(textileSession.TextileStructure,
                             editorConfigure.PreviewPixelSize,
                             editorConfigure.PreviewHorizontalRepeat,
                             editorConfigure.PreviewVerticalRepeat,
                             concurrencyBackgroundWorkContext,
                             new());

        NotifyTaskCompleteInvokePropertyChangedAsync(previewPainter.PrerenderAsync(), nameof(AlreadyRender));
    }

    public void Rerender() => NotifyTaskCompleteInvokePropertyChangedAsync(previewPainter.PrerenderAsync(), nameof(AlreadyRender));

    public override Task SetSessionAsync(TextileSession textileSession)
    {
        previewPainter.TextileStructure = textileSession.TextileStructure;
        SetSession(textileSession);
        var prerender = previewPainter.PrerenderAsync();
        InvokePropertyChanged(nameof(CanvasSize));
        NotifyTaskCompleteInvokePropertyChangedAsync(prerender, nameof(AlreadyRender));
        InvokePropertyChanged(nameof(AlreadyRender));
        return prerender;
    }

    public SKSizeI PixelSize
    {
        get => previewPainter.PixelSize;
        set
        {
            if (previewPainter.PixelSize == value)
                return;
            previewPainter.PixelSize = value;
            InvokePropertyChanged();
            InvokePropertyChanged(nameof(CanvasSize));
            NotifyTaskCompleteInvokePropertyChangedAsync(previewPainter.PrerenderAsync(), nameof(AlreadyRender));
            InvokePropertyChanged(nameof(AlreadyRender));
        }
    }
    public int RepeatHorizontal
    {
        get => previewPainter.RepeatHorizontal;
        set
        {
            if(previewPainter.RepeatHorizontal == value) 
                return;
            previewPainter.RepeatHorizontal = value;
            InvokePropertyChanged();
            InvokePropertyChanged(nameof(CanvasSize));
        }
    }
    public int RepeatVertical
    {
        get => previewPainter.RepeatVertical;
        set
        {
            if (previewPainter.RepeatVertical == value)
                return;
            previewPainter.RepeatVertical = value;
            InvokePropertyChanged();
            InvokePropertyChanged(nameof(CanvasSize));
        }
    }

    public ISKSurfacePainter PreviewPainter => previewPainter;

    public SKSizeI CanvasSize => new(
        previewPainter.PixelSize.Width * previewPainter.TextileStructure.Textile.Width * previewPainter.RepeatHorizontal, 
        previewPainter.PixelSize.Height * previewPainter.TextileStructure.Textile.Height * previewPainter.RepeatVertical);

    public override bool AlreadyRender => previewPainter.AlreadyRender;

    protected override void OnTextileStructureChanged(TextileStructure textileStructure)
    {
        var prerender = previewPainter.PrerenderAsync();
        InvokePropertyChanged(nameof(CanvasSize));
        NotifyTaskCompleteInvokePropertyChangedAsync(prerender, nameof(AlreadyRender));
        InvokePropertyChanged(nameof(AlreadyRender));
    }
}