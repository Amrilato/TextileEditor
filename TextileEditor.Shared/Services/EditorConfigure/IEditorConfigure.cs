using SkiaSharp;
using System.ComponentModel;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Services;

public interface IEditorConfigure : INotifyPropertyChanged
{
    SKColor SelectBorderColor { get; set; }
    SKColor BorderColor { get; set; }
    SKColor FillColor { get; set; }
    SKColor PastePreviewFillColor { get; set; }
    Corner TieupPosition { get; set; }
    GridSize GridSize { get; set; }
    int PreviewHorizontalRepeat { get; set; }
    int PreviewVerticalRepeat { get; set; }
    SKSizeI PreviewPixelSize { get; set; }

    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
}