using SkiaSharp;
using TextileEditor.Shared.EventHandlers;

namespace TextileEditor.Shared.Painters;

public interface ITextileEditorContext : ITextilePainterContext
{
    SKSizeI TextileCanvasSize { get; }
    ISKSurfacePainter TextilePainter { get; }
    ICanvasEventHandler TextileEventHandler { get; }

    SKSizeI HeddleCanvasSize { get; }
    ISKSurfacePainter HeddlePainter { get; }
    ICanvasEventHandler HeddleEventHandler { get; }

    SKSizeI PedalCanvasSize { get; }
    ISKSurfacePainter PedalPainter { get; }
    ICanvasEventHandler PedalEventHandler { get; }

    SKSizeI TieupCanvasSize { get; }
    ISKSurfacePainter TieupPainter { get; }
    ICanvasEventHandler TieupEventHandler { get; }

    SKSizeI HeddleColorCanvasSize { get; }
    ISKSurfacePainter HeddleColorPainter { get; }
    ICanvasEventHandler HeddleColorEventHandler { get; }

    SKSizeI PedalColorCanvasSize { get; }
    ISKSurfacePainter PedalColorPainter { get; }
    ICanvasEventHandler PedalColorEventHandler { get; }
}
