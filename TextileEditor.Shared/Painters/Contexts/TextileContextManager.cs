using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.EventHandlers;
using TextileEditor.Shared.Painters.Renderers;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;

public class TextileContextManager
{
    public TextileContextManager(
                                ITextileIntersectionRenderer<TextileIndex, bool, TextileStructure> textileRenderer,
                                ITextileIntersectionRenderer<TextileIndex, bool, SKColor> settingRenderer,
                                ITextileIntersectionRenderer<int, Color, IReadOnlyTextileColor> heddleColorRenderer,
                                ITextileIntersectionRenderer<int, Color, IReadOnlyTextileColor> pedalColorRenderer,
                                ITextileBorderRenderer<SKColor> borderRenderProvider,
                                TextileSession session,
                                IBackgroundWorkerService backgroundWorkerService,
                                IEditorConfigure editorConfigure)
    {
        eventHandlers = new(editorConfigure);

        var editorContextBackgroundWorker = backgroundWorkerService.CreateContext<ConcurrencyBackgroundWorkContext>();
        editorContextBackgroundWorker.Name = $"Editor Render";
        textileSession = session;
        textileEditorContext = new(
            textileRenderer,
            settingRenderer,
            heddleColorRenderer,
            pedalColorRenderer,
            borderRenderProvider,
            session,
            editorConfigure,
            editorContextBackgroundWorker);
        var previewContextBackgroundWorker = backgroundWorkerService.CreateContext<ConcurrencyBackgroundWorkContext>();
        previewContextBackgroundWorker.Name = $"Preview Render";
        textilePreviewContext = new(
            session,
            previewContextBackgroundWorker,
            editorConfigure);
        this.SetEventHandler<TextileClickEventHandler>();
        this.SetEventHandler<TextileColorClickEventHandler>();
    }

    private TextileSession textileSession;
    internal EventHandlerBag eventHandlers;
    internal TextileEditorContext textileEditorContext;
    internal TextilePreviewContext textilePreviewContext;

    public ITextileEditorContext TextileEditorContext
    {
        get
        {
            if(textileEditorContext.Session != textileSession)
                _ = textileEditorContext.SetSessionAsync(textileSession);
            return textileEditorContext;
        }
    }

    public ITextilePreviewContext TextilePreviewContext
    {
        get
        {
            if(textilePreviewContext.Session != textileSession)
                textilePreviewContext.SetSessionAsync(textileSession);
            return textilePreviewContext;
        }
    }

    public TextileSession TextileSession
    {
        get => textileSession;
        set => textileSession = value;
    }
    public void SetTextileColorClickValue(SKColor sKColor) => eventHandlers.ColorClickKHandler.Color = sKColor;
    public ITextileGridEventHandler<TextileIndex, bool> TextileGridEventHandler => textileEditorContext.TextileGridEventHandler;
    public ITextileGridEventHandler<int, Color> TextileColorGridEventHandler => textileEditorContext.TextileColorGridEventHandler;
    public void CopyClipboard()
    {
        if (TextileGridEventHandler is TextileRangeSelectEventHandler handler)
        {
            eventHandlers.PasteHandler.Textile = handler.Clip();
        }
    }
}
