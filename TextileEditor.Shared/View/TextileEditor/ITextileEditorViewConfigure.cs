using SkiaSharp;
using System.ComponentModel;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor;

public interface INotifyPropertyTextileEditorViewConfigure : ITextileEditorViewConfigure, INotifyPropertyChanged;
public interface ITextileEditorViewConfigure
{
    GridSize GridSize { get; }
    SKColor BorderColor { get; }
    SKColor AreaSelectBorderColor { get; }
    SKColor IntersectionColor { get; }
    SKColor PastPreviewIntersectionColor { get; }
    Corner TieupPosition { get; }
}