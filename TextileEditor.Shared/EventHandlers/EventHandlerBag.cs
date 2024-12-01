using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.EventHandlers;

internal readonly struct EventHandlerBag(IEditorConfigure configure)
{
    public readonly TextileClickEventHandler ClickHandler = new();
    public readonly TextileRangeSelectEventHandler RangeHandler = new() { BorderColor = configure.SelectBorderColor };
    public readonly TextilePasteEventHandler PasteHandler = new() { FillColor = configure.PastePreviewFillColor };
    public readonly TextileClearEventHandler ClearHandler = new();
    public readonly TextileColorClickEventHandler ColorClickKHandler = new();

    public event Action RequestSurface
    {
        add
        {
            ClickHandler.RequestSurface += value;
            RangeHandler.RequestSurface += value;
            PasteHandler.RequestSurface += value;
            ColorClickKHandler.RequestSurface += value;
        }
        remove
        {
            ClickHandler.RequestSurface -= value;
            RangeHandler.RequestSurface -= value;
            PasteHandler.RequestSurface -= value;
            ColorClickKHandler.RequestSurface -= value;
        }
    }
}
