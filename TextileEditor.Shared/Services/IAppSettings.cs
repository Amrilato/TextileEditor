using SkiaSharp;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor;
using TextileEditor.Shared.View.TextilePreview;

namespace TextileEditor.Shared.Services;

public interface IAppSettings : INotifyPropertyTextileEditorViewConfigure, INotifyPropertyTextilePreviewConfigure
{
    new GridSize GridSize { get; set; }
    new SKColor BorderColor { get; set; }
    new SKColor AreaSelectBorderColor { get; set; }
    new SKColor IntersectionColor { get; set; }
    new SKColor PastPreviewIntersectionColor { get; set; }
    new Corner TieupPosition { get; set; }


    new int RepeatVertical { get; set; }
    new int RepeatHorizontal { get; set; }
    new SKSizeI PixelSize { get; set; }

    Task SaveAsync();
}