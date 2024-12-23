using SkiaSharp;
using System.ComponentModel;

namespace TextileEditor.Shared.View.TextilePreview;

public interface INotifyPropertyTextilePreviewConfigure : ITextilePreviewConfigure, INotifyPropertyChanged;
public interface ITextilePreviewConfigure
{
    int RepeatVertical { get; }
    int RepeatHorizontal { get; }
    SKSizeI PixelSize { get; }
}