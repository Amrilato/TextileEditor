using TextileEditor.Shared.Painters;

namespace TextileEditor.Shared.EventHandlers;


public static class TextileEditorContextEventHandlerExtensions
{
    public static TextileClickEventHandler SetEventHandler<T>(this TextileContextManager context)
        where T : TextileClickEventHandler
    {
        var eventHandler = context.eventHandlers.ClickHandler;
        context.textileEditorContext.SetTextileEventHandler(eventHandler);
        return eventHandler;
    }
    public static TextileRangeSelectEventHandler SetEventHandler<T>(this TextileContextManager context, int _ = 0)
        where T : TextileRangeSelectEventHandler
    {
        var eventHandler = context.eventHandlers.RangeHandler;
        context.textileEditorContext.SetTextileEventHandler(eventHandler);
        return eventHandler;
    }
    public static TextilePasteEventHandler SetEventHandler<T>(this TextileContextManager context, int _ = 0, int _1 = 0)
        where T : TextilePasteEventHandler
    {
        var eventHandler = context.eventHandlers.PasteHandler;
        context.textileEditorContext.SetTextileEventHandler(eventHandler);
        return eventHandler;
    }
    public static TextileClearEventHandler SetEventHandler<T>(this TextileContextManager context, int _ = 0, int _1 = 0, int _2 = 0)
        where T : TextileClearEventHandler
    {
        var eventHandler = context.eventHandlers.ClearHandler;
        context.textileEditorContext.SetTextileEventHandler(eventHandler);
        return eventHandler;
    }


    public static TextileColorClickEventHandler SetEventHandler<T>(this TextileContextManager context, short _ = 0)
        where T : TextileColorClickEventHandler
    {
        var eventHandler = context.eventHandlers.ColorClickKHandler;
        context.textileEditorContext.SetTextileEventHandler(eventHandler);
        return eventHandler;
    }
}
