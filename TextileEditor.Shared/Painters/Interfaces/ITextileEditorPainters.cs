using System.ComponentModel;
using TextileEditor.Shared.EventHandlers;
using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.Painters
{
    public interface ITextileEditorPainters : INotifyPropertyChanged
    {
        bool AlreadyRender { get; }
        ICanvasEventHandler HeddleColorEventHandler { get; }
        ISKSurfacePainter HeddleColorPainter { get; }
        ICanvasEventHandler HeddleEventHandler { get; }
        ISKSurfacePainter HeddlePainter { get; }
        ICanvasEventHandler PedalColorEventHandler { get; }
        ISKSurfacePainter PedalColorPainter { get; }
        ICanvasEventHandler PedalEventHandler { get; }
        ISKSurfacePainter PedalPainter { get; }
        ICanvasEventHandler TextileEventHandler { get; }
        ISKSurfacePainter TextilePainter { get; }
        TextileSession TextileSession { get; }
        ICanvasEventHandler TieupEventHandler { get; }
        ISKSurfacePainter TieupPainter { get; }
    }
}